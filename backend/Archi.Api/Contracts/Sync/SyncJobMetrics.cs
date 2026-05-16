namespace Archi.Api.Contracts.Sync;

public sealed record SyncJobMetrics(
    int TotalProcessed,
    int Inserted,
    int SkippedDuplicates,
    int Failed,
    double SuccessRate);
