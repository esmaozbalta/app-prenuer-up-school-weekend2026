using Archi.Api.Contracts.Search;

namespace Archi.Api.Services.Search;

public interface ITmdbClient
{
    Task<IReadOnlyList<ExternalMediaItemDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
