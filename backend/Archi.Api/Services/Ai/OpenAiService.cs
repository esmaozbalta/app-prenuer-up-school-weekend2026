using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Archi.Api.Contracts.Chat;
using Archi.Api.Options;
using Microsoft.Extensions.Options;

namespace Archi.Api.Services.Ai;

public sealed partial class OpenAiService(
    HttpClient httpClient,
    IOptions<OpenAiOptions> options,
    ILogger<OpenAiService> logger) : IAiService
{
    private const string SystemPrompt =
        "You are the Archi assistant. Suggest a movie, TV show, game, or book based on the user's input. " +
        "You must respond ONLY with a raw JSON object in this exact format: " +
        "{\"title\": \"...\", \"category\": \"...\", \"description\": \"...\", \"year\": \"...\", \"imageUrl\": \"...\"}";

    private const string GracefulMessage =
        "Asistan şu an kısa bir mola veriyor. Lütfen birazdan tekrar dene.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<ChatResponse> ChatAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            logger.LogWarning("OpenAI API key is not configured.");
            return ChatResponse.FromMessage(GracefulMessage);
        }

        try
        {
            var payload = new
            {
                model = settings.Model,
                messages = new object[]
                {
                    new { role = "system", content = SystemPrompt },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.7
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, $"{settings.BaseUrl.TrimEnd('/')}/chat/completions");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.ApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.PaymentRequired)
            {
                logger.LogWarning("OpenAI quota or rate limit reached: {StatusCode}", response.StatusCode);
                return ChatResponse.FromMessage(GracefulMessage);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "OpenAI request failed with {StatusCode}: {Body}",
                    response.StatusCode,
                    errorBody);
                return ChatResponse.FromMessage(GracefulMessage);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                logger.LogWarning("OpenAI returned empty content.");
                return ChatResponse.FromMessage(GracefulMessage);
            }

            var suggestion = TryParseSuggestion(content);
            return suggestion is not null
                ? ChatResponse.FromSuggestion(suggestion)
                : ChatResponse.FromMessage(GracefulMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OpenAI chat request failed.");
            return ChatResponse.FromMessage(GracefulMessage);
        }
    }

    public static AiSuggestionDto? TryParseSuggestion(string rawContent)
    {
        var json = ExtractJsonObject(rawContent);
        if (json is null)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var title = ReadString(root, "title");
            var category = ReadString(root, "category");
            var description = ReadString(root, "description");
            var year = ReadString(root, "year");
            var imageUrl = ReadString(root, "imageUrl");

            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(description))
            {
                return null;
            }

            return new AiSuggestionDto(
                title.Trim(),
                category.Trim(),
                description.Trim(),
                year?.Trim() ?? string.Empty,
                imageUrl?.Trim() ?? string.Empty);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? ReadString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? ExtractJsonObject(string content)
    {
        var trimmed = content.Trim();
        var fenced = JsonFenceRegex().Match(trimmed);
        if (fenced.Success)
        {
            trimmed = fenced.Groups["json"].Value.Trim();
        }

        var start = trimmed.IndexOf('{');
        var end = trimmed.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            return null;
        }

        return trimmed[start..(end + 1)];
    }

    [GeneratedRegex(@"```(?:json)?\s*(?<json>\{[\s\S]*?\})\s*```", RegexOptions.IgnoreCase)]
    private static partial Regex JsonFenceRegex();
}
