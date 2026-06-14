namespace Archi.Api.Contracts.Discovery;

public sealed record DiscoveryItemDto(
    string ExternalId,
    string Title,
    string Category,
    string Year,
    string Description,
    string ImageUrl);
