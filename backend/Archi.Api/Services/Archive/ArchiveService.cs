using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Common;
using Archi.Api.Contracts.Search;
using Archi.Api.Data;
using Archi.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Archi.Api.Services.Archive;

public sealed class ArchiveService(AppDbContext dbContext) : IArchiveService
{
    public async Task<ArchiveItemResponse> AddAsync(
        Guid userId,
        AddArchiveRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateAddRequest(request);

        var externalId = request.ExternalId.Trim();
        var category = request.Category.Trim().ToLowerInvariant();
        var title = request.Title.Trim();

        var duplicate = await dbContext.ArchiveItems.AnyAsync(
            item => item.UserId == userId &&
                    item.ExternalId == externalId &&
                    item.Category == category,
            cancellationToken);

        if (duplicate)
        {
            throw new ArchiveDuplicateException();
        }

        var now = DateTimeOffset.UtcNow;
        var item = new ArchiveItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExternalId = externalId,
            Category = category,
            Title = title,
            Metadata = request.Metadata ?? new ArchiveMetadata(),
            Status = request.Status,
            ReferralUrl = string.IsNullOrWhiteSpace(request.ReferralUrl) ? null : request.ReferralUrl.Trim(),
            CreatedAt = now
        };

        foreach (var tag in NormalizeTags(request.Tags))
        {
            item.VibeTags.Add(new VibeTag
            {
                Id = Guid.NewGuid(),
                ItemId = item.Id,
                TagName = tag,
                CreatedAt = now
            });
        }

        dbContext.ArchiveItems.Add(item);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            throw new ArchiveDuplicateException();
        }

        return MapToResponse(item);
    }

    public async Task<ArchiveListResponse?> ListByUserAsync(
        Guid targetUserId,
        Guid? callerUserId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var targetUser = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == targetUserId, cancellationToken);

        if (targetUser is null)
        {
            return null;
        }

        var isOwner = callerUserId == targetUserId;
        if (targetUser.IsPrivate && !isOwner)
        {
            return null;
        }

        var resolvedLimit = Math.Clamp(limit, 1, 100);
        var items = await dbContext.ArchiveItems.AsNoTracking()
            .Where(item => item.UserId == targetUserId)
            .Include(item => item.VibeTags)
            .OrderByDescending(item => item.CreatedAt)
            .Take(resolvedLimit)
            .ToListAsync(cancellationToken);

        var responses = items.Select(MapToResponse).ToList();
        return new ArchiveListResponse(responses, responses.Count);
    }

    private static void ValidateAddRequest(AddArchiveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ExternalId) ||
            string.IsNullOrWhiteSpace(request.Category) ||
            string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArchiveValidationException("externalId, category and title are required.");
        }

        var category = request.Category.Trim().ToLowerInvariant();
        if (category is not (MediaCategories.Movie or MediaCategories.Book or MediaCategories.Game))
        {
            throw new ArchiveValidationException("category must be movie, book, or game.");
        }

        if (!Enum.IsDefined(request.Status))
        {
            throw new ArchiveValidationException("status is invalid.");
        }
    }

    private static IReadOnlyList<string> NormalizeTags(IReadOnlyList<string>? tags)
    {
        if (tags is null || tags.Count == 0)
        {
            return [];
        }

        return tags
            .Select(tag => tag.Trim().ToLowerInvariant())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Length > 30 ? tag[..30] : tag)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static ArchiveItemResponse MapToResponse(ArchiveItem item) =>
        new(
            item.Id,
            item.UserId,
            item.ExternalId,
            item.Category,
            item.Title,
            item.Metadata,
            item.Status,
            item.ReferralUrl,
            item.CreatedAt,
            item.VibeTags.Select(tag => tag.TagName).OrderBy(name => name).ToList());

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}

public sealed class ArchiveDuplicateException : Exception;

public sealed class ArchiveValidationException(string message) : Exception(message);
