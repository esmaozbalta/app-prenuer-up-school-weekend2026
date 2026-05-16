using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Archi.Api.Models;

public sealed class User
{
    [Column("Id")]
    public Guid Id { get; set; }

    /// <summary>Harici kimlik sağlayıcısı (ör. Firebase) kullanıcı kimliği.</summary>
    [Column("OauthId")]
    [MaxLength(255)]
    public string? OauthId { get; set; }

    [Column("Email")]
    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [Column("NormalizedEmail")]
    [MaxLength(254)]
    public string NormalizedEmail { get; set; } = string.Empty;

    [Column("Username")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Column("NormalizedUsername")]
    [MaxLength(50)]
    public string NormalizedUsername { get; set; } = string.Empty;

    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("IsPrivate")]
    public bool IsPrivate { get; set; }

    /// <summary>Premium / Vault üyeliği.</summary>
    [Column("IsVaultMember")]
    public bool IsVaultMember { get; set; }

    /// <summary>Supabase şemasında sütun adı snake_case: <c>created_at</c>.</summary>
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
}
