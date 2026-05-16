using System.Diagnostics;
using Archi.Api.Contracts.Common;
using Archi.Api.Contracts.Feed;
using Archi.Api.Services.Cache;
using Archi.Api.Services.Feed;

namespace Archi.Api.Endpoints;

public static class FeedEndpoints
{
    private static readonly TimeSpan FeedCacheTtl = TimeSpan.FromSeconds(60);

    public static IEndpointRouteBuilder MapFeedEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/feed/global", HandleGlobalFeedAsync);
        return app;
    }

    private static async Task<IResult> HandleGlobalFeedAsync(
        string? cursor,
        int? limit,
        IFeedService feedService,
        ICacheService cacheService,
        ILogger<IFeedService> logger,
        CancellationToken cancellationToken)
    {
        var resolvedLimit = limit ?? 20;
        var cacheKey = CacheKeys.GlobalFeed(cursor, resolvedLimit);

        var cached = await cacheService.GetAsync<GlobalFeedResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            logger.LogInformation(
                "Global feed cache hit (cursor={HasCursor}, limit={Limit}, provider={Provider})",
                !string.IsNullOrWhiteSpace(cursor),
                resolvedLimit,
                cacheService.ProviderName);
            return Results.Ok(cached);
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await feedService.GetGlobalFeedAsync(cursor, resolvedLimit, cancellationToken);
            stopwatch.Stop();

            await cacheService.SetAsync(cacheKey, response, FeedCacheTtl, cancellationToken);

            logger.LogInformation(
                "Global feed loaded in {ElapsedMs}ms (cache miss, provider={Provider})",
                stopwatch.ElapsedMilliseconds,
                cacheService.ProviderName);

            return Results.Ok(response);
        }
        catch (FeedValidationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }
}
