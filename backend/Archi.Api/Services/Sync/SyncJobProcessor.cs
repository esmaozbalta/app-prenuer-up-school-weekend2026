using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Search;
using Archi.Api.Contracts.Sync;
using Archi.Api.Models;
using Archi.Api.Options;
using Archi.Api.Services.Archive;
using Microsoft.Extensions.Options;

namespace Archi.Api.Services.Sync;

public sealed class SyncJobProcessor(
    ISyncJobStore jobStore,
    ISteamLibraryClient steamLibraryClient,
    CsvImportParser csvImportParser,
    IArchiveBulkImporter bulkImporter,
    IOptions<SyncOptions> syncOptions,
    ILogger<SyncJobProcessor> logger)
{
    public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken)
    {
        if (!jobStore.TryGet(jobId, out var job) || job is null)
        {
            return;
        }

        job.Status = SyncJobStatus.Running;
        jobStore.Update(job);

        try
        {
            var metrics = job.Type switch
            {
                SyncJobType.Steam => await ProcessSteamAsync(job, cancellationToken),
                SyncJobType.CsvImport => await ProcessCsvAsync(job, cancellationToken),
                _ => new SyncJobMetrics(0, 0, 0, 0, 0)
            };

            job.TotalProcessed = metrics.TotalProcessed;
            job.Inserted = metrics.Inserted;
            job.SkippedDuplicates = metrics.SkippedDuplicates;
            job.Failed = metrics.Failed;
            job.Status = metrics.SuccessRate >= 0.95 || metrics.TotalProcessed == 0
                ? SyncJobStatus.Completed
                : SyncJobStatus.Failed;
            job.ErrorMessage = job.Status == SyncJobStatus.Failed
                ? "Sync success rate fell below 95%."
                : null;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.CsvContent = null;
            jobStore.Update(job);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sync job {JobId} failed.", jobId);
            job.Status = SyncJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTimeOffset.UtcNow;
            job.CsvContent = null;
            jobStore.Update(job);
        }
    }

    private async Task<SyncJobMetrics> ProcessSteamAsync(SyncJobRecord job, CancellationToken cancellationToken)
    {
        var maxGames = syncOptions.Value.MaxSteamGames;
        var games = await steamLibraryClient.GetOwnedGamesAsync(
            job.SteamProfileUrl ?? string.Empty,
            maxGames,
            cancellationToken);

        var rows = games.Select(game => new ImportArchiveRow(
            $"steam-{game.AppId}",
            MediaCategories.Game,
            game.Name,
            new ArchiveMetadata
            {
                CoverUrl = game.CoverUrl,
                Platform = "Steam",
                Summary = game.PlaytimeMinutes is > 0
                    ? $"{game.PlaytimeMinutes} minutes played"
                    : null
            },
            ArchiveItemStatus.Done)).ToList();

        return await bulkImporter.ImportAsync(job.UserId, rows, cancellationToken);
    }

    private async Task<SyncJobMetrics> ProcessCsvAsync(SyncJobRecord job, CancellationToken cancellationToken)
    {
        if (job.CsvContent is null || job.CsvContent.Length == 0)
        {
            return new SyncJobMetrics(0, 0, 0, 0, 1d);
        }

        var rows = csvImportParser.Parse(job.CsvContent, syncOptions.Value.MaxCsvRows);
        return await bulkImporter.ImportAsync(job.UserId, rows, cancellationToken);
    }
}
