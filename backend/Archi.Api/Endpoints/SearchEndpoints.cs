using System.Diagnostics;
using System.Text.Json;
using System.Net.Http.Json; // HttpClient için eklendi
using Archi.Api.Contracts.Common;
using Archi.Api.Contracts.Search;
using Archi.Api.Models;
using Archi.Api.Services.Cache;
using Archi.Api.Services.Search;
using Microsoft.AspNetCore.Mvc;

namespace Archi.Api.Endpoints;

public static class SearchEndpoints
{
    private static readonly TimeSpan OmniSearchCacheTtl = TimeSpan.FromMinutes(5);

    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/search/omni", HandleOmniSearchAsync);
        app.MapGet("/api/v1/ai-search", HandleAiSearchAsync);
        
        return app;
    }

    private static async Task<IResult> HandleAiSearchAsync(
        [FromQuery] string query, 
        [FromQuery] string category, 
        IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Results.BadRequest(new Archi.Api.Contracts.Common.ErrorResponse("Arama kelimesi boş olamaz."));
        }

        var apiKey = configuration["Gemini:ApiKey"]?.Trim().Replace("\r", "").Replace("\n", "").Replace(" ", "");
        if (string.IsNullOrEmpty(apiKey))
        {
            return Results.Problem("Gemini API Key tanımlanmamış.");
        }

        string prompt = $@"
        Sen Archi uygulamasının akıllı arama motorusun. 
        Kullanıcı '{category}' kategorisinde '{query}' içeriğini arattı.
        Bana bu içerikle eşleşen en doğru popüler sonucu bul.
        Eğer kategori 'Kitap' ise Roman/Öykü gibi alt türünü, 'Film' ise direkt Film yaz.
        ImageUrl için internette var olan, kırık olmayan kaliteli bir afiş veya görsel linki bul (placeholder/unsplash/wikimedia vb. olabilir).
        
        Cevabı SADECE ama SADECE aşağıdaki JSON formatında dön, başında veya sonunda başka hiçbir metin, açıklama veya ```json bloğu olmasın:
        {{
            ""title"": ""İçeriğin Gerçek Adı"",
            ""category"": ""{category}"",
            ""description"": ""İçeriğin çok kısa, 1 cümlelik vurucu özeti veya alt başlığı"",
            ""year"": ""Çıkış Yılı (Örn: 2014)"",
            ""imageUrl"": ""Görsel Linki""
        }}";

        try
        {
            using var httpClient = new HttpClient();
            
            // İŞTE BÜYÜK SIR BURADA: gemini-2.5-flash kullanıyoruz!
            string p1 = "https://";
            string p2 = "generativelanguage.googleapis.com";
            string p3 = "/v1beta/models/gemini-2.0-flash-lite:generateContent";
            string requestUrl = $"{p1}{p2}{p3}?key={apiKey}";
            
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var response = await httpClient.PostAsJsonAsync(requestUrl, requestBody);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return Results.Problem($"Google API Hatası: {responseString}");
            }

            using var document = JsonDocument.Parse(responseString);
            var jsonResult = document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()?.Trim();

            if (jsonResult != null && jsonResult.StartsWith("```"))
            {
                jsonResult = jsonResult.Replace("```json", "").Replace("```", "").Trim();
            }

            if (string.IsNullOrEmpty(jsonResult))
            {
                return Results.Problem("Yapay zekadan boş yanıt döndü.");
            }

            var searchResult = JsonSerializer.Deserialize<SearchResultDto>(jsonResult, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            return Results.Ok(new List<SearchResultDto> { searchResult! });
        }
        catch (Exception ex)
        {
            return Results.Problem($"AI Arama motorunda kritik bir hata oluştu: {ex.Message}");
        }
    }
    private static async Task<IResult> HandleOmniSearchAsync(
        string? q,
        IOmniSearchService omniSearchService,
        ICacheService cacheService,
        ILogger<IOmniSearchService> logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest(new Archi.Api.Contracts.Common.ErrorResponse("Query parameter 'q' is required."));
        }

        var normalizedQuery = q.Trim();
        var cacheKey = CacheKeys.OmniSearch(normalizedQuery);
        var cached = await cacheService.GetAsync<OmniSearchResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogInformation(
                "Omni search cache hit for query length {Length} ({Provider})",
                normalizedQuery.Length,
                cacheService.ProviderName);
            return Results.Ok(cached);
        }

        var stopwatch = Stopwatch.StartNew();
        var result = await omniSearchService.SearchAsync(normalizedQuery, cancellationToken);
        stopwatch.Stop();

        await cacheService.SetAsync(cacheKey, result, OmniSearchCacheTtl, cancellationToken);

        logger.LogInformation(
            "Omni search completed in {ElapsedMs}ms (cache miss, provider {Provider})",
            stopwatch.ElapsedMilliseconds,
            cacheService.ProviderName);

        return Results.Ok(result);
    }
}