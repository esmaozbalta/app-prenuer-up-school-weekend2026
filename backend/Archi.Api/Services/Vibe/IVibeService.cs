using Archi.Api.Contracts.Vibe;

namespace Archi.Api.Services.Vibe;

public interface IVibeService
{
    Task<VibeTagSummary> AddTagAsync(
        Guid userId,
        Guid itemId,
        string tagName,
        CancellationToken cancellationToken = default);

    Task<TopVibeTagsResponse?> GetTopTagsAsync(
        Guid itemId,
        int limit,
        CancellationToken cancellationToken = default);
}
