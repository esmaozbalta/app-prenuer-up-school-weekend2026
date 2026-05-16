namespace Archi.Api.Services.Sync;

public interface ISyncJobStore
{
    void Add(SyncJobRecord job);

    bool TryGet(Guid jobId, out SyncJobRecord? job);

    void Update(SyncJobRecord job);
}
