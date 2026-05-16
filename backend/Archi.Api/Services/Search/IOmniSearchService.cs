using Archi.Api.Contracts.Search;

namespace Archi.Api.Services.Search;

public interface IOmniSearchService
{
    Task<OmniSearchResponse> SearchAsync(string query, CancellationToken cancellationToken = default);
}
