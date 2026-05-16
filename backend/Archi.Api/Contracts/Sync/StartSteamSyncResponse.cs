namespace Archi.Api.Contracts.Sync;

public sealed record StartSteamSyncResponse(Guid JobId, SyncJobStatus Status);
