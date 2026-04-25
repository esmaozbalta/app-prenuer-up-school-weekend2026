namespace Archi.Api.Contracts.Profile;

public sealed record UserProfileResponse(
    Guid UserId,
    string Email,
    string Username,
    bool IsPrivate);
