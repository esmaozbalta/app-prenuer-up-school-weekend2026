namespace Archi.Api.Contracts.Sync;

public sealed record CsvImportResponse(Guid JobId, SyncJobStatus Status);
