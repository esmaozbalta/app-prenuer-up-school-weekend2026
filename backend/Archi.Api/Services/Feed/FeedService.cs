using System.Text.Json;
using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Feed;
using Archi.Api.Data;
using Archi.Api.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Archi.Api.Services.Feed;

public sealed class FeedService(AppDbContext dbContext, ILogger<FeedService> logger) : IFeedService
{
    private static readonly TimeSpan FeedWindow = TimeSpan.FromHours(24);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<GlobalFeedResponse> GetGlobalFeedAsync(
        string? cursor,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(cursor) && !FeedCursor.TryDecode(cursor, out _, out _))
        {
            throw new FeedValidationException("cursor is invalid.");
        }

        var resolvedLimit = Math.Clamp(limit, 1, 50);
        var cutoff = DateTimeOffset.UtcNow.Subtract(FeedWindow);

        FeedCursor.TryDecode(cursor, out var cursorCreatedAt, out var cursorId);

        List<FeedRow> rows;
        if (UsesInMemoryProvider())
        {
            rows = await QueryViaEfAsync(cutoff, cursorCreatedAt, cursorId, resolvedLimit + 1, cancellationToken);
        }
        else
        {
            rows = await QueryViaDapperAsync(cutoff, cursorCreatedAt, cursorId, resolvedLimit + 1, cancellationToken);
        }

        var hasMore = rows.Count > resolvedLimit;
        if (hasMore)
        {
            rows.RemoveAt(rows.Count - 1);
        }

        var itemIds = rows.Select(row => row.Id).ToList();
        var topTagsByItem = await LoadTopVibeTagsAsync(itemIds, cancellationToken);

        var items = rows.Select(row => new GlobalFeedItemResponse(
            row.Id,
            row.UserId,
            row.Username,
            row.ExternalId,
            row.Category,
            row.Title,
            row.Metadata,
            row.Status,
            row.CreatedAt,
            topTagsByItem.GetValueOrDefault(row.Id, []))).ToList();

        string? nextCursor = null;
        if (hasMore && items.Count > 0)
        {
            var last = items[^1];
            nextCursor = FeedCursor.Encode(last.CreatedAt, last.Id);
        }

        return new GlobalFeedResponse(items, nextCursor);
    }

    private async Task<List<FeedRow>> QueryViaEfAsync(
        DateTimeOffset cutoff,
        DateTimeOffset cursorCreatedAt,
        Guid cursorId,
        int take,
        CancellationToken cancellationToken)
    {
        var query =
            from item in dbContext.ArchiveItems.AsNoTracking()
            join user in dbContext.Users.AsNoTracking() on item.UserId equals user.Id
            where !user.IsPrivate && item.CreatedAt >= cutoff
            select new FeedRow
            {
                Id = item.Id,
                UserId = item.UserId,
                Username = user.Username,
                ExternalId = item.ExternalId,
                Category = item.Category,
                Title = item.Title,
                Metadata = item.Metadata,
                Status = item.Status,
                CreatedAt = item.CreatedAt
            };

        if (cursorId != Guid.Empty)
        {
            query = query.Where(row =>
                row.CreatedAt < cursorCreatedAt ||
                (row.CreatedAt == cursorCreatedAt && row.Id < cursorId));
        }

        return await query
            .OrderByDescending(row => row.CreatedAt)
            .ThenByDescending(row => row.Id)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<FeedRow>> QueryViaDapperAsync(
        DateTimeOffset cutoff,
        DateTimeOffset cursorCreatedAt,
        Guid cursorId,
        int take,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                ai.id AS "Id",
                ai.user_id AS "UserId",
                u."Username" AS "Username",
                ai.external_id AS "ExternalId",
                ai.category AS "Category",
                ai.title AS "Title",
                ai.metadata::text AS "MetadataJson",
                ai.status AS "Status",
                ai.created_at AS "CreatedAt"
            FROM archive_items ai
            INNER JOIN users u ON ai.user_id = u."Id"
            WHERE u."IsPrivate" = false
              AND ai.created_at >= @Cutoff
              AND (
                    @HasCursor = false
                    OR ai.created_at < @CursorCreatedAt
                    OR (ai.created_at = @CursorCreatedAt AND ai.id < @CursorId)
                  )
            ORDER BY ai.created_at DESC, ai.id DESC
            LIMIT @Take
            """;

        await using var connection = dbContext.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        var hasCursor = cursorId != Guid.Empty;
        var rawRows = await connection.QueryAsync<FeedDapperRow>(
            sql,
            new
            {
                Cutoff = cutoff,
                HasCursor = hasCursor,
                CursorCreatedAt = hasCursor ? cursorCreatedAt : DateTimeOffset.MinValue,
                CursorId = hasCursor ? cursorId : Guid.Empty,
                Take = take
            });

        return rawRows.Select(row => new FeedRow
        {
            Id = row.Id,
            UserId = row.UserId,
            Username = row.Username,
            ExternalId = row.ExternalId,
            Category = row.Category,
            Title = row.Title,
            Metadata = DeserializeMetadata(row.MetadataJson, logger),
            Status = (ArchiveItemStatus)row.Status,
            CreatedAt = row.CreatedAt
        }).ToList();
    }

    private async Task<Dictionary<Guid, IReadOnlyList<string>>> LoadTopVibeTagsAsync(
        IReadOnlyList<Guid> itemIds,
        CancellationToken cancellationToken)
    {
        if (itemIds.Count == 0)
        {
            return [];
        }

        var tags = await dbContext.VibeTags.AsNoTracking()
            .Where(tag => itemIds.Contains(tag.ItemId))
            .GroupBy(tag => new { tag.ItemId, tag.TagName })
            .Select(group => new { group.Key.ItemId, group.Key.TagName, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return tags
            .GroupBy(tag => tag.ItemId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .OrderByDescending(tag => tag.Count)
                    .ThenBy(tag => tag.TagName)
                    .Take(5)
                    .Select(tag => tag.TagName)
                    .ToList());
    }

    private bool UsesInMemoryProvider() =>
        dbContext.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true;

    private static ArchiveMetadata DeserializeMetadata(string? json, ILogger logger)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new ArchiveMetadata();
        }

        try
        {
            return JsonSerializer.Deserialize<ArchiveMetadata>(json, JsonOptions) ?? new ArchiveMetadata();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize feed metadata JSON.");
            return new ArchiveMetadata();
        }
    }

    private sealed class FeedDapperRow
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string ExternalId { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? MetadataJson { get; set; }

        public short Status { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }
}

public sealed class FeedValidationException(string message) : Exception(message);
