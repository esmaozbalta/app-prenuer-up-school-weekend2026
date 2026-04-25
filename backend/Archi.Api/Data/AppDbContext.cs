using Archi.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Archi.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

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
        users.Property(user => user.IsPrivate).HasDefaultValue(false);
    }
}
