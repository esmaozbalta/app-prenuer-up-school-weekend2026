namespace Archi.Api.Services.Sync;

public sealed class SyncJobBackgroundService(
    SyncJobQueue jobQueue,
    IServiceScopeFactory scopeFactory,
    ILogger<SyncJobBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in jobQueue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<SyncJobProcessor>();
                await processor.ProcessAsync(workItem.JobId, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unhandled error while processing sync job {JobId}.", workItem.JobId);
            }
        }
    }
}
