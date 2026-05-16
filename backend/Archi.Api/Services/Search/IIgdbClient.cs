using Archi.Api.Contracts.Search;

namespace Archi.Api.Services.Search;

public interface IIgdbClient
{
    Task<IReadOnlyList<ExternalMediaItemDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
