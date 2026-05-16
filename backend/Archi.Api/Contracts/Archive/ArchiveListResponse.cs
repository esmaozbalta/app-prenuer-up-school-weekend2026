namespace Archi.Api.Contracts.Archive;

public sealed record ArchiveListResponse(
    IReadOnlyList<ArchiveItemResponse> Items,
    int TotalCount);
