using Archi.Api.Contracts.Search;

namespace Archi.Api.Services.Search;

public sealed class OmniSearchService(
    ITmdbClient tmdbClient,
    IGoogleBooksClient googleBooksClient,
    IIgdbClient igdbClient,
    ILogger<OmniSearchService> logger) : IOmniSearchService
{
    public async Task<OmniSearchResponse> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new OmniSearchResponse([], [], []);
        }

        var moviesTask = SafeSearchAsync(
            "TMDB",
            ct => tmdbClient.SearchAsync(query, ct),
            cancellationToken);
        var booksTask = SafeSearchAsync(
            "Google Books",
            ct => googleBooksClient.SearchAsync(query, ct),
            cancellationToken);
        var gamesTask = SafeSearchAsync(
            "IGDB",
            ct => igdbClient.SearchAsync(query, ct),
            cancellationToken);

        await Task.WhenAll(moviesTask, booksTask, gamesTask);

        return new OmniSearchResponse(
            await moviesTask,
            await booksTask,
            await gamesTask);
    }

    private async Task<IReadOnlyList<ExternalMediaItemDto>> SafeSearchAsync(
        string providerName,
        Func<CancellationToken, Task<IReadOnlyList<ExternalMediaItemDto>>> search,
        CancellationToken cancellationToken)
    {
        try
        {
            return await search(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "{Provider} search failed for query.", providerName);
            return [];
        }
    }
}
