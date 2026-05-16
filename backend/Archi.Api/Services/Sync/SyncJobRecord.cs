using Archi.Api.Contracts.Sync;

namespace Archi.Api.Services.Sync;

public sealed class SyncJobRecord
{
    public Guid JobId { get; init; }

    public Guid UserId { get; init; }

    public SyncJobType Type { get; init; }

    public SyncJobStatus Status { get; set; } = SyncJobStatus.Queued;

    public string? SteamProfileUrl { get; init; }

    public string? CsvFileName { get; init; }

    public byte[]? CsvContent { get; set; }

    public string? ErrorMessage { get; set; }

    public int TotalProcessed { get; set; }

    public int Inserted { get; set; }

    public int SkippedDuplicates { get; set; }

    public int Failed { get; set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? CompletedAt { get; set; }

    public SyncJobMetrics ToMetrics()
    {
        var processed = TotalProcessed;
        var successRate = processed == 0
            ? 1d
            : (double)(Inserted + SkippedDuplicates) / processed;
        return new SyncJobMetrics(processed, Inserted, SkippedDuplicates, Failed, successRate);
    }

    public SyncJobResponse ToResponse() =>
        new(
            JobId,
            Type,
            Status,
            Status is SyncJobStatus.Completed or SyncJobStatus.Failed ? ToMetrics() : null,
            ErrorMessage,
            Status == SyncJobStatus.Failed,
            CreatedAt,
            CompletedAt);
}
