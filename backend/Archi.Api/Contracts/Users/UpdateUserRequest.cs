namespace Archi.Api.Contracts.Users;

public sealed record UpdateUserRequest(
    string? Username,
    string? Email,
    string? CurrentPassword,
    string? NewPassword);
