using Archi.Api.Contracts.Archive;
using Archi.Api.Models;

namespace Archi.Api.Services.Feed;

internal sealed class FeedRow
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string ExternalId { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public ArchiveMetadata Metadata { get; set; } = new();

    public ArchiveItemStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
