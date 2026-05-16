using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Archi.Api.Models;

public sealed class VibeTag
{
    [Column("id")]
    public Guid Id { get; set; }

    [Column("item_id")]
    public Guid ItemId { get; set; }

    [Column("tag_name")]
    [MaxLength(30)]
    public string TagName { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    public ArchiveItem Item { get; set; } = null!;
}
