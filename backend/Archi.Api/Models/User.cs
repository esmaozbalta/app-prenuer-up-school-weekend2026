using System.ComponentModel.DataAnnotations;

namespace Archi.Api.Models;

public sealed class User
{
    public Guid Id { get; set; }

    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(254)]
    public string NormalizedEmail { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(50)]
    public string NormalizedUsername { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsPrivate { get; set; }
}
