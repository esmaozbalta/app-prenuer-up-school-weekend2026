using Archi.Api.Contracts.Discovery;

namespace Archi.Api.Services.Rawg;
public interface IRawgService
{
    Task<IEnumerable<DiscoveryItemDto>> SearchGamesAsync(string query, CancellationToken cancellationToken = default);
}