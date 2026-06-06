namespace Archi.Api.Contracts.Chat;

public sealed record AiSuggestionDto(
    string Title,
    string Category,
    string Description,
    string Year,
    string ImageUrl);

public sealed record ChatResponse(
    string Type,
    AiSuggestionDto? Suggestion,
    string? Message)
{
    public const string TypeSuggestion = "suggestion";
    public const string TypeMessage = "message";

    public static ChatResponse FromSuggestion(AiSuggestionDto suggestion) =>
        new(TypeSuggestion, suggestion, null);

    public static ChatResponse FromMessage(string message) =>
        new(TypeMessage, null, message);
}
