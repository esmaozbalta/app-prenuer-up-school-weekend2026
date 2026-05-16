namespace Archi.Api.Contracts.Feed;

public sealed record GlobalFeedResponse(
    IReadOnlyList<GlobalFeedItemResponse> Items,
    string? NextCursor);
