using System.Threading.Channels;

namespace Archi.Api.Services.Sync;

public sealed class SyncJobQueue
{
    private readonly Channel<SyncJobWorkItem> _channel =
        Channel.CreateUnbounded<SyncJobWorkItem>(new UnboundedChannelOptions
        {
            SingleReader = true
        });

    public ValueTask EnqueueAsync(Guid jobId, CancellationToken cancellationToken = default) =>
        _channel.Writer.WriteAsync(new SyncJobWorkItem(jobId), cancellationToken);

    public IAsyncEnumerable<SyncJobWorkItem> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
