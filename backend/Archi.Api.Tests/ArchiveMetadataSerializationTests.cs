using System.Text.Json;
using Archi.Api.Contracts.Archive;

namespace Archi.Api.Tests;

public sealed class ArchiveMetadataSerializationTests
{
    [Fact]
    public void RoundTrip_preserves_fields()
    {
        var original = new ArchiveMetadata
        {
            CoverUrl = "https://example.com/cover.jpg",
            Year = 2024,
            Author = "Author",
            Director = "Director",
            Platform = "PC",
            Summary = "Summary text"
        };

        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<ArchiveMetadata>(json);

        Assert.NotNull(restored);
        Assert.Equal(original.CoverUrl, restored.CoverUrl);
        Assert.Equal(original.Year, restored.Year);
        Assert.Equal(original.Author, restored.Author);
        Assert.Equal(original.Director, restored.Director);
        Assert.Equal(original.Platform, restored.Platform);
        Assert.Equal(original.Summary, restored.Summary);
    }
}
