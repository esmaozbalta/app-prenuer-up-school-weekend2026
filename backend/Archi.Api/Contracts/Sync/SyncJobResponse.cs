namespace Archi.Api.Contracts.Sync;

public sealed record SyncJobResponse(
    Guid JobId,
    SyncJobType Type,
    SyncJobStatus Status,
    SyncJobMetrics? Metrics,
    string? ErrorMessage,
    bool CanRetry,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
