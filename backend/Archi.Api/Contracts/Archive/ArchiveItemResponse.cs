using Archi.Api.Models;

namespace Archi.Api.Contracts.Archive;

public sealed record ArchiveItemResponse(
    Guid Id,
    Guid UserId,
    string ExternalId,
    string Category,
    string Title,
    ArchiveMetadata Metadata,
    ArchiveItemStatus Status,
    string? ReferralUrl,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> Tags);
