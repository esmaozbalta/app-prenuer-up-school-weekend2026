namespace Archi.Api.Contracts.Archive;

/// <summary>
/// JSONB metadata for <c>archive_items</c>. Optional fields vary by category (movie / book / game).
/// </summary>
public sealed class ArchiveMetadata
{
    public string? CoverUrl { get; set; }

    public int? Year { get; set; }

    /// <summary>Books.</summary>
    public string? Author { get; set; }

    /// <summary>Movies / TV.</summary>
    public string? Director { get; set; }

    /// <summary>Games.</summary>
    public string? Platform { get; set; }

    public string? Summary { get; set; }

    /// <summary>AI discovery assistant — same role as <see cref="Summary"/>.</summary>
    public string? Description { get; set; }

    /// <summary>AI discovery assistant — same role as <see cref="CoverUrl"/>.</summary>
    public string? ImageUrl { get; set; }
}
