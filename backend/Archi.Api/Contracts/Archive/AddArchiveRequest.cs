using Archi.Api.Models;

namespace Archi.Api.Contracts.Archive;

public sealed record AddArchiveRequest(
    string ExternalId,
    string Category,
    string Title,
    ArchiveMetadata? Metadata,
    ArchiveItemStatus Status,
    IReadOnlyList<string>? Tags,
    string? ReferralUrl);
