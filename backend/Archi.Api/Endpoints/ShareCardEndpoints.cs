using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Archi.Api.Services.Share;

namespace Archi.Api.Endpoints;

public static class ShareCardEndpoints
{
    public static IEndpointRouteBuilder MapShareCardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/share-card/{itemId:guid}", HandleShareCardAsync);
        return app;
    }

    private static async Task<IResult> HandleShareCardAsync(
        Guid itemId,
        ClaimsPrincipal principal,
        IShareCardService shareCardService,
        CancellationToken cancellationToken)
    {
        var callerId = TryGetUserId(principal);
        var png = await shareCardService.RenderForItemAsync(itemId, callerId, cancellationToken);
        if (png is null)
        {
            return Results.NotFound();
        }

        return Results.File(png, "image/png", $"archi-share-{itemId:N}.png");
    }

    private static Guid? TryGetUserId(ClaimsPrincipal principal)
    {
        foreach (var claim in principal.Claims)
        {
            if (claim.Type is JwtRegisteredClaimNames.Sub or "sub" or ClaimTypes.NameIdentifier &&
                Guid.TryParse(claim.Value, out var id))
            {
                return id;
            }
        }

        return null;
    }
}
