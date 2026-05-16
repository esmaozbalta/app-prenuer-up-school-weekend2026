using Archi.Api.Contracts.Archive;

namespace Archi.Api.Contracts.Search;

public sealed record ExternalMediaItemDto(
    string ExternalId,
    string Category,
    string Title,
    ArchiveMetadata Metadata);
