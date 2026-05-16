using Archi.Api.Contracts.Sync;
namespace Archi.Api.Services.Sync;

public sealed class SyncService(
    ISyncJobStore jobStore,
    SyncJobQueue jobQueue) : ISyncService
{
    public async Task<StartSteamSyncResponse> StartSteamSyncAsync(
        Guid userId,
        string steamProfileUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(steamProfileUrl))
        {
            throw new SyncValidationException("steamProfileUrl is required.");
        }

        var job = new SyncJobRecord
        {
            JobId = Guid.NewGuid(),
            UserId = userId,
            Type = SyncJobType.Steam,
            SteamProfileUrl = steamProfileUrl.Trim()
        };

        jobStore.Add(job);
        await jobQueue.EnqueueAsync(job.JobId, cancellationToken);
        return new StartSteamSyncResponse(job.JobId, job.Status);
    }

    public Task<SyncJobResponse?> GetJobAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!jobStore.TryGet(jobId, out var job) || job is null || job.UserId != userId)
        {
            return Task.FromResult<SyncJobResponse?>(null);
        }

        return Task.FromResult<SyncJobResponse?>(job.ToResponse());
    }

    public async Task<StartSteamSyncResponse?> RetryJobAsync(
        Guid jobId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (!jobStore.TryGet(jobId, out var job) || job is null || job.UserId != userId)
        {
            return null;
        }

        if (job.Type != SyncJobType.Steam || job.Status != SyncJobStatus.Failed)
        {
            throw new SyncValidationException("Only failed Steam sync jobs can be retried.");
        }

        job.Status = SyncJobStatus.Queued;
        job.ErrorMessage = null;
        job.CompletedAt = null;
        job.TotalProcessed = 0;
        job.Inserted = 0;
        job.SkippedDuplicates = 0;
        job.Failed = 0;
        jobStore.Update(job);

        await jobQueue.EnqueueAsync(job.JobId, cancellationToken);
        return new StartSteamSyncResponse(job.JobId, job.Status);
    }

    public async Task<CsvImportResponse> ImportCsvAsync(
        Guid userId,
        string fileName,
        byte[] csvBytes,
        CancellationToken cancellationToken = default)
    {
        if (csvBytes.Length == 0)
        {
            throw new SyncValidationException("CSV file is empty.");
        }

        var job = new SyncJobRecord
        {
            JobId = Guid.NewGuid(),
            UserId = userId,
            Type = SyncJobType.CsvImport,
            CsvFileName = fileName,
            CsvContent = csvBytes
        };

        jobStore.Add(job);
        await jobQueue.EnqueueAsync(job.JobId, cancellationToken);
        return new CsvImportResponse(job.JobId, job.Status);
    }
}

public sealed class SyncValidationException(string message) : Exception(message);
