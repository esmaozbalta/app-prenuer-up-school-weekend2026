using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Search;
using Archi.Api.Options;
using Microsoft.Extensions.Options;

namespace Archi.Api.Services.Search;

public sealed class TmdbClient(
    IHttpClientFactory httpClientFactory,
    IOptions<TmdbOptions> options,
    IOptions<ExternalApiOptions> externalApiOptions,
    ILogger<TmdbClient> logger) : ITmdbClient
{
    private static readonly string ImageBaseUrl = "https://image.tmdb.org/t/p/w500";

    public async Task<IReadOnlyList<ExternalMediaItemDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (externalApiOptions.Value.UseStubs)
        {
            return SearchStubs.Movies(query);
        }

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            logger.LogWarning("TMDB ApiKey is not configured.");
            return [];
        }

        var client = httpClientFactory.CreateClient("tmdb");
        var encodedQuery = Uri.EscapeDataString(query.Trim());
        var path =
            $"/search/movie?api_key={settings.ApiKey}&query={encodedQuery}&include_adult=false&language=en-US&page=1";

        using var response = await client.GetAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("TMDB search failed with status {StatusCode}", response.StatusCode);
            return [];
        }

        var payload = await response.Content.ReadFromJsonAsync<TmdbSearchResponse>(cancellationToken);
        if (payload?.Results is null)
        {
            return [];
        }

        return payload.Results
            .Where(result => result.Id > 0 && !string.IsNullOrWhiteSpace(result.Title))
            .Take(10)
            .Select(result => new ExternalMediaItemDto(
                result.Id.ToString(),
                MediaCategories.Movie,
                result.Title!,
                new ArchiveMetadata
                {
                    CoverUrl = string.IsNullOrWhiteSpace(result.PosterPath)
                        ? null
                        : $"{ImageBaseUrl}{result.PosterPath}",
                    Year = ParseYear(result.ReleaseDate),
                    Summary = result.Overview
                }))
            .ToList();
    }

    private static int? ParseYear(string? releaseDate) =>
        DateTime.TryParse(releaseDate, out var parsed) ? parsed.Year : null;

    private sealed class TmdbSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbMovieResult>? Results { get; set; }
    }

    private sealed class TmdbMovieResult
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }
    }
}
