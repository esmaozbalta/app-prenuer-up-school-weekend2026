namespace Archi.Api.Contracts.Users;

public sealed record UserListResponse(IReadOnlyList<UserResponse> Items, int TotalCount);
