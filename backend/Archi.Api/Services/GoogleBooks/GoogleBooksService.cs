using System.Text.Json;
using Archi.Api.Contracts.Discovery;

namespace Archi.Api.Services.GoogleBooks;

public interface IGoogleBooksService
{
    Task<List<DiscoveryItemDto>> SearchBooksAsync(string query, CancellationToken cancellationToken);
}

public class GoogleBooksService(HttpClient httpClient) : IGoogleBooksService
{
    public async Task<List<DiscoveryItemDto>> SearchBooksAsync(string query, CancellationToken cancellationToken)
    {
        var results = new List<DiscoveryItemDto>();
        if (string.IsNullOrWhiteSpace(query)) return results;

        try
        {
            var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(query)}&limit=10";
            var response = await httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode) return results;

            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(jsonString);
            
            if (!doc.RootElement.TryGetProperty("docs", out var docs)) return results;

            foreach (var item in docs.EnumerateArray())
            {
                var externalId = item.TryGetProperty("key", out var keyProp) ? keyProp.GetString() ?? Guid.NewGuid().ToString() : Guid.NewGuid().ToString();
                var title = item.TryGetProperty("title", out var t) ? t.GetString() ?? "Bilinmeyen Kitap" : "Bilinmeyen Kitap";
                
                var description = "";
                if (item.TryGetProperty("author_name", out var authors) && authors.GetArrayLength() > 0)
                {
                    description = authors[0].GetString() ?? "";
                }

                string imageUrl = "";
                if (item.TryGetProperty("cover_i", out var coverId))
                {
                    // ToString() kullanarak parse hatası riskini tamamen ortadan kaldırdık
                    imageUrl = $"https://covers.openlibrary.org/b/id/{coverId.ToString()}-L.jpg";
                }

                string year = "";
                if (item.TryGetProperty("first_publish_year", out var pubYear))
                {
                    year = pubYear.ToString();
                }

                results.Add(new DiscoveryItemDto(
                    externalId,   // 1. Parametre: ID
                    title,        // 2. Parametre: Başlık
                    "Book",       // 3. Parametre: Kategori
                    year,         // 4. Parametre: Yıl (Flutter buradaki ilk 4 harfi alıyor)
                    description,  // 5. Parametre: Açıklama
                    imageUrl      // 6. Parametre: Resim URL'si
                ));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n--- OPENLIBRARY HATASI ---");
            Console.WriteLine(ex.Message);
            Console.WriteLine($"---------------------------\n");
        }

        return results;
    }
}