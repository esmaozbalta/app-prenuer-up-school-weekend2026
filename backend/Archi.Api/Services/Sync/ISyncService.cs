using Archi.Api.Contracts.Sync;

namespace Archi.Api.Services.Sync;

public interface ISyncService
{
    Task<StartSteamSyncResponse> StartSteamSyncAsync(
        Guid userId,
        string steamProfileUrl,
        CancellationToken cancellationToken = default);

    Task<SyncJobResponse?> GetJobAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default);

    Task<StartSteamSyncResponse?> RetryJobAsync(
        Guid jobId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<CsvImportResponse> ImportCsvAsync(
        Guid userId,
        string fileName,
        byte[] csvBytes,
        CancellationToken cancellationToken = default);
}
