using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Archi.Api.Contracts.Common;
using Archi.Api.Contracts.Sync;
using Archi.Api.Services.Sync;

namespace Archi.Api.Endpoints;

public static class SyncEndpoints
{
    private const long MaxCsvBytes = 5 * 1024 * 1024;

    public static IEndpointRouteBuilder MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/sync/steam", HandleStartSteamSyncAsync).RequireAuthorization();
        app.MapGet("/api/v1/sync/jobs/{jobId:guid}", HandleGetJobAsync).RequireAuthorization();
        app.MapPost("/api/v1/sync/jobs/{jobId:guid}/retry", HandleRetryJobAsync).RequireAuthorization();
        app.MapPost("/api/v1/sync/goodreads", HandleImportCsvAsync)
            .RequireAuthorization()
            .DisableAntiforgery();
        return app;
    }

    private static async Task<IResult> HandleStartSteamSyncAsync(
        StartSteamSyncRequest request,
        ClaimsPrincipal principal,
        ISyncService syncService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId(principal);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var response = await syncService.StartSteamSyncAsync(
                userId.Value,
                request.SteamProfileUrl,
                cancellationToken);
            return Results.Accepted($"/api/v1/sync/jobs/{response.JobId}", response);
        }
        catch (SyncValidationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<IResult> HandleGetJobAsync(
        Guid jobId,
        ClaimsPrincipal principal,
        ISyncService syncService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId(principal);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var job = await syncService.GetJobAsync(jobId, userId.Value, cancellationToken);
        return job is null ? Results.NotFound() : Results.Ok(job);
    }

    private static async Task<IResult> HandleRetryJobAsync(
        Guid jobId,
        ClaimsPrincipal principal,
        ISyncService syncService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId(principal);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var response = await syncService.RetryJobAsync(jobId, userId.Value, cancellationToken);
            return response is null
                ? Results.NotFound()
                : Results.Accepted($"/api/v1/sync/jobs/{response.JobId}", response);
        }
        catch (SyncValidationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static async Task<IResult> HandleImportCsvAsync(
        HttpRequest request,
        ClaimsPrincipal principal,
        ISyncService syncService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId(principal);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        if (!request.HasFormContentType)
        {
            return Results.BadRequest(new ErrorResponse("multipart/form-data with a CSV file is required."));
        }

        var form = await request.ReadFormAsync(cancellationToken);
        var file = form.Files.FirstOrDefault();
        if (file is null || file.Length == 0)
        {
            return Results.BadRequest(new ErrorResponse("CSV file field is required."));
        }

        if (file.Length > MaxCsvBytes)
        {
            return Results.BadRequest(new ErrorResponse("CSV file exceeds 5 MB limit."));
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        try
        {
            var response = await syncService.ImportCsvAsync(
                userId.Value,
                file.FileName,
                memory.ToArray(),
                cancellationToken);
            return Results.Accepted($"/api/v1/sync/jobs/{response.JobId}", response);
        }
        catch (SyncValidationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
    }

    private static Guid? TryGetUserId(ClaimsPrincipal principal)
    {
        var candidates = new[]
        {
            principal.FindFirstValue(JwtRegisteredClaimNames.Sub),
            principal.FindFirstValue(ClaimTypes.NameIdentifier),
        };

        foreach (var candidate in candidates)
        {
            if (Guid.TryParse(candidate, out var id))
            {
                return id;
            }
        }

        return null;
    }
}
