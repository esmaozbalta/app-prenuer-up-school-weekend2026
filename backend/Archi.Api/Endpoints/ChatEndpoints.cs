using Archi.Api.Contracts.Chat;
using Archi.Api.Contracts.Common;
using Archi.Api.Services.Ai;

namespace Archi.Api.Endpoints;

public static class ChatEndpoints
{
    public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/chat", HandleChatAsync).RequireAuthorization();
        return app;
    }

    private static async Task<IResult> HandleChatAsync(
        ChatRequest request,
        IAiService aiService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest(new ErrorResponse("message is required."));
        }

        var response = await aiService.ChatAsync(request.Message.Trim(), cancellationToken);
        return Results.Ok(response);
    }
}
