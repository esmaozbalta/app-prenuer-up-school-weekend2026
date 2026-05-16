using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Search;
using Archi.Api.Options;
using Archi.Api.Services.Cache;
using Microsoft.Extensions.Options;

namespace Archi.Api.Services.Search;

public sealed class IgdbClient(
    IHttpClientFactory httpClientFactory,
    IOptions<IgdbOptions> options,
    IOptions<ExternalApiOptions> externalApiOptions,
    ICacheService cacheService,
    ILogger<IgdbClient> logger) : IIgdbClient
{
    private static readonly SemaphoreSlim RequestThrottle = new(1, 1);
    private static DateTimeOffset _lastSearchRequestAt = DateTimeOffset.MinValue;

    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _tokenExpiresAt = DateTimeOffset.MinValue;

    public async Task<IReadOnlyList<ExternalMediaItemDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (externalApiOptions.Value.UseStubs)
        {
            return SearchStubs.Games(query);
        }

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ClientId) ||
            string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            logger.LogWarning("IGDB ClientId or ClientSecret is not configured.");
            return [];
        }

        var token = await GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(token))
        {
            return [];
        }

        await EnforceRateLimitAsync(settings.MinIntervalMilliseconds, cancellationToken);

        var client = httpClientFactory.CreateClient("igdb");
        var escaped = query.Trim().Replace("\"", "\\\"", StringComparison.Ordinal);
        using var request = new HttpRequestMessage(HttpMethod.Post, "/games")
        {
            Content = new StringContent(
                $"search \"{escaped}\"; fields id,name,summary,cover.url,first_release_date; limit 10;",
                Encoding.UTF8,
                "text/plain")
        };
        request.Headers.Add("Client-ID", settings.ClientId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("IGDB search failed with status {StatusCode}", response.StatusCode);
            return [];
        }

        var games = await response.Content.ReadFromJsonAsync<List<IgdbGame>>(cancellationToken);
        if (games is null)
        {
            return [];
        }

        return games
            .Where(game => game.Id > 0 && !string.IsNullOrWhiteSpace(game.Name))
            .Select(game => new ExternalMediaItemDto(
                game.Id.ToString(),
                MediaCategories.Game,
                game.Name!,
                new ArchiveMetadata
                {
                    CoverUrl = game.Cover?.Url?.StartsWith("//", StringComparison.Ordinal) == true
                        ? $"https:{game.Cover.Url}"
                        : game.Cover?.Url,
                    Year = game.FirstReleaseDate is > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(game.FirstReleaseDate.Value).Year
                        : null,
                    Summary = game.Summary
                }))
            .ToList();
    }

    private static async Task EnforceRateLimitAsync(int minIntervalMs, CancellationToken cancellationToken)
    {
        await RequestThrottle.WaitAsync(cancellationToken);
        try
        {
            var minInterval = TimeSpan.FromMilliseconds(Math.Max(0, minIntervalMs));
            var elapsed = DateTimeOffset.UtcNow - _lastSearchRequestAt;
            if (elapsed < minInterval)
            {
                await Task.Delay(minInterval - elapsed, cancellationToken);
            }

            _lastSearchRequestAt = DateTimeOffset.UtcNow;
        }
        finally
        {
            RequestThrottle.Release();
        }
    }

    private async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var cachedToken = await cacheService.GetAsync<CachedIgdbToken>(CacheKeys.IgdbOAuthToken(), cancellationToken);
        if (cachedToken is not null && cachedToken.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return cachedToken.AccessToken;
        }

        if (!string.IsNullOrWhiteSpace(_accessToken) && _tokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return _accessToken;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            cachedToken = await cacheService.GetAsync<CachedIgdbToken>(CacheKeys.IgdbOAuthToken(), cancellationToken);
            if (cachedToken is not null && cachedToken.ExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return cachedToken.AccessToken;
            }

            if (!string.IsNullOrWhiteSpace(_accessToken) && _tokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return _accessToken;
            }

            var settings = options.Value;
            var tokenClient = httpClientFactory.CreateClient("twitch");
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = settings.ClientId,
                ["client_secret"] = settings.ClientSecret,
                ["grant_type"] = "client_credentials"
            });

            using var response = await tokenClient.PostAsync(settings.TokenUrl, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("IGDB token request failed with status {StatusCode}", response.StatusCode);
                return null;
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TwitchTokenResponse>(cancellationToken);
            if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
            {
                return null;
            }

            _accessToken = tokenResponse.AccessToken;
            var expiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, tokenResponse.ExpiresIn - 60));
            _tokenExpiresAt = expiresAt;

            var cacheTtl = TimeSpan.FromSeconds(Math.Max(60, tokenResponse.ExpiresIn - 120));
            await cacheService.SetAsync(
                CacheKeys.IgdbOAuthToken(),
                new CachedIgdbToken(_accessToken, expiresAt),
                cacheTtl,
                cancellationToken);

            return _accessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private sealed record CachedIgdbToken(string AccessToken, DateTimeOffset ExpiresAt);

    private sealed class TwitchTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    private sealed class IgdbGame
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("first_release_date")]
        public long? FirstReleaseDate { get; set; }

        [JsonPropertyName("cover")]
        public IgdbCover? Cover { get; set; }
    }

    private sealed class IgdbCover
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
