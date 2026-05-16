using Archi.Api.Contracts.Vibe;
using Archi.Api.Data;
using Archi.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Archi.Api.Services.Vibe;

public sealed class VibeService(AppDbContext dbContext) : IVibeService
{
    public async Task<VibeTagSummary> AddTagAsync(
        Guid userId,
        Guid itemId,
        string tagName,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeTag(tagName);
        if (normalized is null)
        {
            throw new VibeValidationException("tagName is required.");
        }

        var item = await dbContext.ArchiveItems.AsNoTracking()
            .Where(archiveItem => archiveItem.Id == itemId)
            .Select(archiveItem => new { archiveItem.Id, archiveItem.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            throw new VibeNotFoundException();
        }

        var ownerIsPublic = await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == item.UserId)
            .Select(user => !user.IsPrivate)
            .FirstOrDefaultAsync(cancellationToken);

        if (!ownerIsPublic)
        {
            var isOwner = item.UserId == userId;
            if (!isOwner)
            {
                throw new VibeNotFoundException();
            }
        }

        var now = DateTimeOffset.UtcNow;
        dbContext.VibeTags.Add(new VibeTag
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            TagName = normalized,
            CreatedAt = now
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        var count = await dbContext.VibeTags.AsNoTracking()
            .CountAsync(tag => tag.ItemId == itemId && tag.TagName == normalized, cancellationToken);

        return new VibeTagSummary(normalized, count);
    }

    public async Task<TopVibeTagsResponse?> GetTopTagsAsync(
        Guid itemId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ArchiveItems.AsNoTracking()
            .Where(archiveItem => archiveItem.Id == itemId)
            .Select(archiveItem => new { archiveItem.Id, archiveItem.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        var canView = await CanViewItemAsync(item.UserId, cancellationToken);
        if (!canView)
        {
            return null;
        }

        var resolvedLimit = Math.Clamp(limit, 1, 20);
        var tags = await dbContext.VibeTags.AsNoTracking()
            .Where(tag => tag.ItemId == itemId)
            .GroupBy(tag => tag.TagName)
            .Select(group => new { TagName = group.Key, Count = group.Count() })
            .OrderByDescending(group => group.Count)
            .ThenBy(group => group.TagName)
            .Take(resolvedLimit)
            .ToListAsync(cancellationToken);

        return new TopVibeTagsResponse(
            itemId,
            tags.Select(group => new VibeTagSummary(group.TagName, group.Count)).ToList());
    }

    private async Task<bool> CanViewItemAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        return await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == ownerUserId)
            .Select(user => !user.IsPrivate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static string? NormalizeTag(string tagName)
    {
        if (string.IsNullOrWhiteSpace(tagName))
        {
            return null;
        }

        var normalized = tagName.Trim().ToLowerInvariant();
        return normalized.Length > 30 ? normalized[..30] : normalized;
    }
}

public sealed class VibeValidationException(string message) : Exception(message);

public sealed class VibeNotFoundException : Exception;
