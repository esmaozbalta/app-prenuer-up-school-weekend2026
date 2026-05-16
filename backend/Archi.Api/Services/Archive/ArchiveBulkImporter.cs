using Archi.Api.Contracts.Sync;
using Archi.Api.Data;
using Archi.Api.Models;
using Archi.Api.Services.Sync;
using Microsoft.EntityFrameworkCore;

namespace Archi.Api.Services.Archive;

public sealed class ArchiveBulkImporter(AppDbContext dbContext) : IArchiveBulkImporter
{
    public async Task<SyncJobMetrics> ImportAsync(
        Guid userId,
        IReadOnlyList<ImportArchiveRow> rows,
        CancellationToken cancellationToken = default)
    {
        var inserted = 0;
        var skipped = 0;
        var failed = 0;
        var processed = 0;

        if (rows.Count == 0)
        {
            return new SyncJobMetrics(0, 0, 0, 0, 1d);
        }

        var existingKeys = await dbContext.ArchiveItems.AsNoTracking()
            .Where(item => item.UserId == userId)
            .Select(item => item.ExternalId + "|" + item.Category)
            .ToListAsync(cancellationToken);

        var existing = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTimeOffset.UtcNow;
        var batch = new List<ArchiveItem>();

        foreach (var row in rows)
        {
            processed++;
            try
            {
                var externalId = row.ExternalId.Trim();
                var category = row.Category.Trim().ToLowerInvariant();
                var key = $"{externalId}|{category}";

                if (string.IsNullOrWhiteSpace(externalId) ||
                    string.IsNullOrWhiteSpace(row.Title) ||
                    existing.Contains(key))
                {
                    skipped++;
                    continue;
                }

                var item = new ArchiveItem
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ExternalId = externalId,
                    Category = category,
                    Title = row.Title.Trim(),
                    Metadata = row.Metadata,
                    Status = row.Status,
                    CreatedAt = now
                };

                batch.Add(item);
                existing.Add(key);
                inserted++;
            }
            catch
            {
                failed++;
            }
        }

        if (batch.Count > 0)
        {
            dbContext.ArchiveItems.AddRange(batch);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var successRate = processed == 0 ? 1d : (double)(inserted + skipped) / processed;
        return new SyncJobMetrics(processed, inserted, skipped, failed, successRate);
    }
}
