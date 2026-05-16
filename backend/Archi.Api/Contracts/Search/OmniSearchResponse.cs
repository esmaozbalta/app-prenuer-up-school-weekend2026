namespace Archi.Api.Contracts.Search;

public sealed record OmniSearchResponse(
    IReadOnlyList<ExternalMediaItemDto> Movies,
    IReadOnlyList<ExternalMediaItemDto> Books,
    IReadOnlyList<ExternalMediaItemDto> Games);
