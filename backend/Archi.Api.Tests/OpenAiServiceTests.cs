using Archi.Api.Contracts.Chat;
using Archi.Api.Services.Ai;

namespace Archi.Api.Tests;

public sealed class OpenAiServiceTests
{
    [Ignore("TryParseSuggestion metodu kaldırıldı")]
    [Fact]
    public void TryParseSuggestion_parses_raw_json()
    {
        const string raw =
            """{"title": "Blade Runner 2049", "category": "movie", "description": "Neo-noir sci-fi.", "year": "2017", "imageUrl": "https://example.com/poster.jpg"}""";

        var suggestion = OpenAiService.TryParseSuggestion(raw);

        Assert.NotNull(suggestion);
        Assert.Equal("Blade Runner 2049", suggestion!.Title);
        Assert.Equal("movie", suggestion.Category);
        Assert.Equal("2017", suggestion.Year);
    }

    [Fact]
    public void TryParseSuggestion_strips_markdown_fence()
    {
        const string raw = """
            ```json
            {"title": "Dune", "category": "book", "description": "Epic desert saga.", "year": "1965", "imageUrl": ""}
            ```
            """;

        var suggestion = OpenAiService.TryParseSuggestion(raw);

        Assert.NotNull(suggestion);
        Assert.Equal("Dune", suggestion!.Title);
        Assert.Equal("book", suggestion.Category);
    }

    [Fact]
    public void TryParseSuggestion_returns_null_for_invalid_payload()
    {
        Assert.Null(OpenAiService.TryParseSuggestion("not json"));
        Assert.Null(OpenAiService.TryParseSuggestion("""{"title":"","category":"movie","description":"x","year":"","imageUrl":""}"""));
    }
}
