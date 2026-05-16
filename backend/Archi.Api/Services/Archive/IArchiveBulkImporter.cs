using Archi.Api.Contracts.Sync;
using Archi.Api.Services.Sync;

namespace Archi.Api.Services.Archive;

public interface IArchiveBulkImporter
{
    Task<SyncJobMetrics> ImportAsync(
        Guid userId,
        IReadOnlyList<ImportArchiveRow> rows,
        CancellationToken cancellationToken = default);
}
