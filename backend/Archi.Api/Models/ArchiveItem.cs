using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Archi.Api.Contracts.Archive;

namespace Archi.Api.Models;

public sealed class ArchiveItem
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("external_id")]
    [MaxLength(128)]
    public string ExternalId { get; set; } = string.Empty;

    [Column("category")]
    [MaxLength(20)]
    public string Category { get; set; } = string.Empty;

    [Column("title")]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Column("metadata")]
    public ArchiveMetadata Metadata { get; set; } = new();

    [Column("status")]
    public ArchiveItemStatus Status { get; set; }

    [Column("referral_url")]
    [MaxLength(2048)]
    public string? ReferralUrl { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public ICollection<VibeTag> VibeTags { get; set; } = new List<VibeTag>();
}
