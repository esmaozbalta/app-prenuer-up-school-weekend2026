namespace Archi.Api.Contracts.Users;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string Username,
    bool IsPrivate,
    bool IsVaultMember,
    DateTimeOffset CreatedAt,
    string? OauthId);
