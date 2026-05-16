using System.Collections.Concurrent;

namespace Archi.Api.Services.Sync;

public sealed class InMemorySyncJobStore : ISyncJobStore
{
    private readonly ConcurrentDictionary<Guid, SyncJobRecord> _jobs = new();

    public void Add(SyncJobRecord job) => _jobs[job.JobId] = job;

    public bool TryGet(Guid jobId, out SyncJobRecord? job) => _jobs.TryGetValue(jobId, out job);

    public void Update(SyncJobRecord job) => _jobs[job.JobId] = job;
}
