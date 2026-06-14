using System.Text.Json;
using Archi.Api.Contracts.Discovery;

namespace Archi.Api.Services.Rawg;

public sealed class RawgService(HttpClient httpClient, IConfiguration configuration, ILogger<RawgService> logger) : IRawgService
{
    // Arayüz ile tam uyumlu olması için burada DiscoveryItemDto kullanıyoruz
    public async Task<IEnumerable<DiscoveryItemDto>> SearchGamesAsync(string query, CancellationToken cancellationToken = default)
    {
        var apiKey = configuration["RawgApiKey"]; 
        
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("RAWG API Key eksik!");
            return Array.Empty<DiscoveryItemDto>();
        }

        try
        {
            var url = $"https://api.rawg.io/api/games?search={Uri.EscapeDataString(query)}&key={apiKey}&page_size=5";
            
            var response = await httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var document = JsonDocument.Parse(jsonString);
            
            var results = new List<DiscoveryItemDto>();

            if (document.RootElement.TryGetProperty("results", out var resultsArray))
            {
                foreach (var game in resultsArray.EnumerateArray())
                {
                    var title = game.GetProperty("name").GetString() ?? "Bilinmeyen Oyun";
                    var externalId = $"rawg-{game.GetProperty("id").GetInt32()}";
                    
                    var imageUrl = game.TryGetProperty("background_image", out var bgElement) && bgElement.ValueKind == JsonValueKind.String 
                        ? bgElement.GetString() 
                        : "";

                    var year = "";
                    if (game.TryGetProperty("released", out var releasedElement) && releasedElement.ValueKind == JsonValueKind.String)
                    {
                        var released = releasedElement.GetString();
                        if (!string.IsNullOrEmpty(released) && released.Length >= 4)
                        {
                            year = released[..4];
                        }
                    }

                    results.Add(new DiscoveryItemDto(
                        ExternalId: externalId,
                        Title: title,
                        Category: "game",
                        Year: year,
                        Description: "Video Oyunu",
                        ImageUrl: imageUrl
                    ));
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RAWG API arama hatası");
            // Burası da DiscoveryItemDto dönmeli
            return Array.Empty<DiscoveryItemDto>();
        }
    }
}