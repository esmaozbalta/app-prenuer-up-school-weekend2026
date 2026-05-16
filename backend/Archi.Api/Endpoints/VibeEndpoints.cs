using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Archi.Api.Contracts.Common;
using Archi.Api.Contracts.Vibe;
using Archi.Api.Services.Vibe;

namespace Archi.Api.Endpoints;

public static class VibeEndpoints
{
    public static IEndpointRouteBuilder MapVibeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/archive/{itemId:guid}/vibes", HandleAddVibeAsync).RequireAuthorization();
        app.MapGet("/api/v1/archive/{itemId:guid}/vibes/top", HandleTopVibesAsync);
        return app;
    }

    private static async Task<IResult> HandleAddVibeAsync(
        Guid itemId,
        AddVibeTagRequest request,
        ClaimsPrincipal principal,
        IVibeService vibeService,
        CancellationToken cancellationToken)
    {
        var userId = TryGetUserId(principal);
        if (userId is null)
        {
            return Results.Unauthorized();
        }

        try
        {
            var summary = await vibeService.AddTagAsync(
                userId.Value,
                itemId,
                request.TagName,
                cancellationToken);
            return Results.Created($"/api/v1/archive/{itemId}/vibes/top", summary);
        }
        catch (VibeValidationException ex)
        {
            return Results.BadRequest(new ErrorResponse(ex.Message));
        }
        catch (VibeNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> HandleTopVibesAsync(
        Guid itemId,
        int? limit,
        IVibeService vibeService,
        CancellationToken cancellationToken)
    {
        var response = await vibeService.GetTopTagsAsync(itemId, limit ?? 5, cancellationToken);
        if (response is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(response);
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
