using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Archi.Api.Contracts.Discovery;

namespace Archi.Api.Services.Tmdb;

public sealed class TmdbService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<TmdbService> logger) : ITmdbService
{
    private const string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";

    public async Task<List<DiscoveryItemDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var apiKey = configuration["Tmdb:ApiKey"];
        var baseUrl = configuration["Tmdb:BaseUrl"] ?? "https://api.themoviedb.org/3";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Tmdb:ApiKey is not configured.");
            return [];
        }

        var encodedQuery = Uri.EscapeDataString(query.Trim());
        var requestUri =
            $"{baseUrl.TrimEnd('/')}/search/multi?query={encodedQuery}&language=tr-TR&include_adult=false&api_key={apiKey}";

        try
        {
            using var response = await httpClient.GetAsync(requestUri, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("TMDB multi search failed with status {StatusCode}", response.StatusCode);
                return [];
            }

            var payload = await response.Content.ReadFromJsonAsync<TmdbMultiSearchResponse>(cancellationToken);
            if (payload?.Results is null || payload.Results.Count == 0)
            {
                return [];
            }

            return payload.Results
                .Where(result => result.MediaType is "movie" or "tv")
                .Select(MapResult)
                .Where(item => !string.IsNullOrWhiteSpace(item.Title))
                .ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "TMDB multi search request failed.");
            return [];
        }
    }

    private static DiscoveryItemDto MapResult(TmdbMultiSearchResult result)
    {
        var isMovie = result.MediaType == "movie";
        var title = isMovie ? result.Title : result.Name;
        var releaseDate = isMovie ? result.ReleaseDate : result.FirstAirDate;

        return new DiscoveryItemDto(
            ExternalId: result.Id.ToString(),
            Title: title?.Trim() ?? string.Empty,
            Category: isMovie ? "Movie" : "TV Show",
            Year: ExtractYear(releaseDate),
            Description: result.Overview?.Trim() ?? string.Empty,
            ImageUrl: BuildImageUrl(result.PosterPath));
    }

    private static string ExtractYear(string? date) =>
        DateTime.TryParse(date, out var parsed) ? parsed.Year.ToString() : string.Empty;

    private static string BuildImageUrl(string? posterPath) =>
        string.IsNullOrWhiteSpace(posterPath) ? string.Empty : $"{ImageBaseUrl}{posterPath}";

    private sealed class TmdbMultiSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbMultiSearchResult>? Results { get; set; }
    }

    private sealed class TmdbMultiSearchResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("media_type")]
        public string? MediaType { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("first_air_date")]
        public string? FirstAirDate { get; set; }
    }
}
