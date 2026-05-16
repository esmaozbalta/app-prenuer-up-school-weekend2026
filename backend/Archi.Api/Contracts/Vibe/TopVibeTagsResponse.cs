namespace Archi.Api.Contracts.Vibe;

public sealed record TopVibeTagsResponse(
    Guid ItemId,
    IReadOnlyList<VibeTagSummary> Tags);
