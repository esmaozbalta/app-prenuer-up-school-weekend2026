using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Search;
using Archi.Api.Options;
using Microsoft.Extensions.Options;

namespace Archi.Api.Services.Search;

public sealed class GoogleBooksClient(
    IHttpClientFactory httpClientFactory,
    IOptions<GoogleBooksOptions> options,
    IOptions<ExternalApiOptions> externalApiOptions,
    ILogger<GoogleBooksClient> logger) : IGoogleBooksClient
{
    public async Task<IReadOnlyList<ExternalMediaItemDto>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (externalApiOptions.Value.UseStubs)
        {
            return SearchStubs.Books(query);
        }

        var settings = options.Value;
        var client = httpClientFactory.CreateClient("googlebooks");
        var encodedQuery = Uri.EscapeDataString(query.Trim());
        var path = $"/volumes?q={encodedQuery}&maxResults=10&printType=books";
        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            path += $"&key={Uri.EscapeDataString(settings.ApiKey)}";
        }

        using var response = await client.GetAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Google Books search failed with status {StatusCode}", response.StatusCode);
            return [];
        }

        var payload = await response.Content.ReadFromJsonAsync<GoogleBooksResponse>(cancellationToken);
        if (payload?.Items is null)
        {
            return [];
        }

        return payload.Items
            .Select(MapVolume)
            .Where(item => item is not null)
            .Cast<ExternalMediaItemDto>()
            .ToList();
    }

    private static ExternalMediaItemDto? MapVolume(GoogleBooksVolume volume)
    {
        var info = volume.VolumeInfo;
        if (info is null || string.IsNullOrWhiteSpace(info.Title))
        {
            return null;
        }

        var externalId = volume.Id ?? info.Title;
        var coverUrl = info.ImageLinks?.Thumbnail ?? info.ImageLinks?.SmallThumbnail;
        var year = info.PublishedDate is { Length: >= 4 } date &&
                   int.TryParse(date.AsSpan(0, 4), out var parsedYear)
            ? parsedYear
            : (int?)null;

        return new ExternalMediaItemDto(
            externalId,
            MediaCategories.Book,
            info.Title,
            new ArchiveMetadata
            {
                CoverUrl = coverUrl,
                Year = year,
                Author = info.Authors is { Count: > 0 } ? string.Join(", ", info.Authors) : null,
                Summary = info.Description
            });
    }

    private sealed class GoogleBooksResponse
    {
        [JsonPropertyName("items")]
        public List<GoogleBooksVolume>? Items { get; set; }
    }

    private sealed class GoogleBooksVolume
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("volumeInfo")]
        public GoogleBooksVolumeInfo? VolumeInfo { get; set; }
    }

    private sealed class GoogleBooksVolumeInfo
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("authors")]
        public List<string>? Authors { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("publishedDate")]
        public string? PublishedDate { get; set; }

        [JsonPropertyName("imageLinks")]
        public GoogleBooksImageLinks? ImageLinks { get; set; }
    }

    private sealed class GoogleBooksImageLinks
    {
        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; }

        [JsonPropertyName("smallThumbnail")]
        public string? SmallThumbnail { get; set; }
    }
}
