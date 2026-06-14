using System.Diagnostics;
using Archi.Api.Contracts.Common;
using Archi.Api.Contracts.Search;
using Archi.Api.Services.Cache;
using Archi.Api.Services.Search;

namespace Archi.Api.Endpoints;

public static class SearchEndpoints
{
    private static readonly TimeSpan OmniSearchCacheTtl = TimeSpan.FromMinutes(5);

    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/search/omni", HandleOmniSearchAsync);
        return app;
    }

    private static async Task<IResult> HandleOmniSearchAsync(
        string? q,
        IOmniSearchService omniSearchService,
        ICacheService cacheService,
        ILogger<IOmniSearchService> logger,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Results.BadRequest(new ErrorResponse("Query parameter 'q' is required."));
        }

        var normalizedQuery = q.Trim();
        var cacheKey = CacheKeys.OmniSearch(normalizedQuery);
        var cached = await cacheService.GetAsync<OmniSearchResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogInformation(
                "Omni search cache hit for query length {Length} ({Provider})",
                normalizedQuery.Length,
                cacheService.ProviderName);
            return Results.Ok(cached);
        }

        var stopwatch = Stopwatch.StartNew();
        var result = await omniSearchService.SearchAsync(normalizedQuery, cancellationToken);
        stopwatch.Stop();

        await cacheService.SetAsync(cacheKey, result, OmniSearchCacheTtl, cancellationToken);

        logger.LogInformation(
            "Omni search completed in {ElapsedMs}ms (cache miss, provider {Provider})",
            stopwatch.ElapsedMilliseconds,
            cacheService.ProviderName);

        return Results.Ok(result);
    }
}
