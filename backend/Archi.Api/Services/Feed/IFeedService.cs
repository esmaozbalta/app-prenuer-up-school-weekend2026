using Archi.Api.Contracts.Feed;

namespace Archi.Api.Services.Feed;

public interface IFeedService
{
    Task<GlobalFeedResponse> GetGlobalFeedAsync(
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default);
}
