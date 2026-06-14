using System.Collections.Generic;

namespace Archi.Api.Contracts.Chat;

public sealed record AiSuggestionDto(
    string Title,
    string Category,
    string Description,
    string Year,
    string ImageUrl);

public sealed record ChatResponse(
    string Type,
    IReadOnlyList<AiSuggestionDto>? Suggestions, // ARTIK LİSTE (ARRAY) BEKLİYORUZ
    string? Message)
{
    public const string TypeSuggestion = "suggestion";
    public const string TypeMessage = "message";

    public static ChatResponse FromSuggestions(IReadOnlyList<AiSuggestionDto> suggestions) =>
        new(TypeSuggestion, suggestions, null);

    public static ChatResponse FromMessage(string message) =>
        new(TypeMessage, null, message);
}