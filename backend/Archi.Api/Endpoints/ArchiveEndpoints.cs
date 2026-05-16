using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Common;
using Archi.Api.Services.Archive;

namespace Archi.Api.Endpoints;

public static class ArchiveEndpoints
{
    public static IEndpointRouteBuilder MapArchiveEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/archive/add", HandleAddArchiveAsync).RequireAuthorization();
        app.MapGet("/api/v1/archive/{userId:guid}", HandleListArchiveAsync);
        return app;
    }

    private static async Task<IResult> HandleAddArchiveAsync(
        AddArchiveRequest request,
        ClaimsPrincipal principal,
        IArchiveService archiveService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId(principal);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var created = await archiveService.AddAsync(userId.Value, request, cancellationToken);
            return Results.Created($"/api/v1/archive/{created.UserId}", created);
        }
        catch (ArchiveValidationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
        catch (ArchiveDuplicateException)
        {
            return Results.Conflict(new ErrorResponse("This item is already in your archive."));
        }
    }

    private static async Task<IResult> HandleListArchiveAsync(
        Guid userId,
        int? limit,
        ClaimsPrincipal principal,
        IArchiveService archiveService,
        CancellationToken cancellationToken)
    {
        var callerId = TryGetUserId(principal);
        var list = await archiveService.ListByUserAsync(
            userId,
            callerId,
            limit ?? 50,
            cancellationToken);

        if (list is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(list);
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
