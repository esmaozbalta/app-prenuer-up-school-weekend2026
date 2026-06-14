using Archi.Api.Contracts.Discovery;

namespace Archi.Api.Services.Tmdb;

public interface ITmdbService
{
    Task<List<DiscoveryItemDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
