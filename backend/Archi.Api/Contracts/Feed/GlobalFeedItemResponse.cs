using Archi.Api.Contracts.Archive;
using Archi.Api.Models;

namespace Archi.Api.Contracts.Feed;

public sealed record GlobalFeedItemResponse(
    Guid Id,
    Guid UserId,
    string Username,
    string ExternalId,
    string Category,
    string Title,
    ArchiveMetadata Metadata,
    ArchiveItemStatus Status,
    DateTimeOffset CreatedAt,
    IReadOnlyList<string> TopVibeTags);
