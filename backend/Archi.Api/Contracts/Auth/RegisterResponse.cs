namespace Archi.Api.Contracts.Auth;

public sealed record RegisterResponse(
    Guid UserId,
    string Email,
    string Username,
    bool IsPrivate,
    string AccessToken);
