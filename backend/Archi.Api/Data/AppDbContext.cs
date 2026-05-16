using System.Text.Json;
using Archi.Api.Contracts.Archive;
using Archi.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Archi.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<ArchiveItem> ArchiveItems => Set<ArchiveItem>();

    public DbSet<VibeTag> VibeTags => Set<VibeTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var users = modelBuilder.Entity<User>();
        users.ToTable("users");
        users.HasKey(user => user.Id);
        users.HasIndex(user => user.NormalizedUsername).IsUnique();
        users.HasIndex(user => user.NormalizedEmail).IsUnique();
        users.Property(user => user.Email).HasMaxLength(254).IsRequired();
        users.Property(user => user.NormalizedEmail).HasMaxLength(254).IsRequired();
        users.Property(user => user.Username).HasMaxLength(50).IsRequired();
        users.Property(user => user.NormalizedUsername).HasMaxLength(50).IsRequired();
        users.Property(user => user.PasswordHash).IsRequired();
        users.Property(user => user.OauthId).HasMaxLength(255);
        users.Property(user => user.IsPrivate).HasDefaultValue(false);
        users.Property(user => user.IsVaultMember).HasDefaultValue(false);
        users.Property(user => user.CreatedAt).HasDefaultValueSql("NOW()");
        users.HasIndex(user => user.OauthId)
            .IsUnique()
            .HasFilter("\"OauthId\" IS NOT NULL");

        var archiveItems = modelBuilder.Entity<ArchiveItem>();
        archiveItems.ToTable("archive_items");
        archiveItems.HasKey(item => item.Id);
        archiveItems.Property(item => item.ExternalId).HasMaxLength(128).IsRequired();
        archiveItems.Property(item => item.Category).HasMaxLength(20).IsRequired();
        archiveItems.Property(item => item.Title).HasMaxLength(500).IsRequired();
        var metadataConverter = new ValueConverter<ArchiveMetadata, string>(
            metadata => JsonSerializer.Serialize(metadata, JsonSerializerOptions.Default),
            json => JsonSerializer.Deserialize<ArchiveMetadata>(json, JsonSerializerOptions.Default) ?? new ArchiveMetadata());
        archiveItems.Property(item => item.Metadata)
            .HasColumnType("jsonb")
            .HasConversion(metadataConverter);
        archiveItems.Property(item => item.Status).HasColumnType("smallint");
        archiveItems.Property(item => item.ReferralUrl).HasMaxLength(2048);
        archiveItems.Property(item => item.CreatedAt).HasDefaultValueSql("NOW()");
        archiveItems.HasIndex(item => item.ExternalId);
        archiveItems.HasIndex(item => new { item.UserId, item.ExternalId, item.Category }).IsUnique();
        archiveItems
            .HasOne(item => item.User)
            .WithMany()
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        var vibeTags = modelBuilder.Entity<VibeTag>();
        vibeTags.ToTable("vibe_tags");
        vibeTags.HasKey(tag => tag.Id);
        vibeTags.Property(tag => tag.TagName).HasMaxLength(30).IsRequired();
        vibeTags.Property(tag => tag.CreatedAt).HasDefaultValueSql("NOW()");
        vibeTags.HasIndex(tag => tag.TagName);
        vibeTags
            .HasOne(tag => tag.Item)
            .WithMany(item => item.VibeTags)
            .HasForeignKey(tag => tag.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
