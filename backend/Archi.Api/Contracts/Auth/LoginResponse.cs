namespace Archi.Api.Contracts.Auth;

public sealed record LoginResponse(
    Guid UserId,
    string Email,
    string Username,
    bool IsPrivate,
    string AccessToken);
