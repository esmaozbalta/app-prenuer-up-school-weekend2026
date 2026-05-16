using Archi.Api.Contracts.Archive;
using Archi.Api.Models;

namespace Archi.Api.Services.Sync;

public sealed record ImportArchiveRow(
    string ExternalId,
    string Category,
    string Title,
    ArchiveMetadata Metadata,
    ArchiveItemStatus Status);
