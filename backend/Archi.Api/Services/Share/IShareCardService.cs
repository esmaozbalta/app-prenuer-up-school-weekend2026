namespace Archi.Api.Services.Share;

public interface IShareCardService
{
    Task<byte[]?> RenderForItemAsync(Guid itemId, Guid? callerUserId, CancellationToken cancellationToken = default);
}
