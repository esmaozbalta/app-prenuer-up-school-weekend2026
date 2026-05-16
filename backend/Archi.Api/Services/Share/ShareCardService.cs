using Archi.Api.Data;
using Archi.Api.Services.Share;
using Microsoft.EntityFrameworkCore;

namespace Archi.Api.Services.Share;

public sealed class ShareCardService(AppDbContext dbContext, IShareCardRenderer renderer) : IShareCardService
{
    public async Task<byte[]?> RenderForItemAsync(
        Guid itemId,
        Guid? callerUserId,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.ArchiveItems.AsNoTracking()
            .Where(archiveItem => archiveItem.Id == itemId)
            .Select(archiveItem => new
            {
                archiveItem.Title,
                archiveItem.Category,
                archiveItem.Metadata,
                archiveItem.UserId,
                OwnerPrivate = dbContext.Users
                    .Where(user => user.Id == archiveItem.UserId)
                    .Select(user => user.IsPrivate)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return null;
        }

        if (item.OwnerPrivate && callerUserId != item.UserId)
        {
            return null;
        }

        var username = await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == item.UserId)
            .Select(user => user.Username)
            .FirstAsync(cancellationToken);

        var png = renderer.Render(new ShareCardModel(
            item.Title,
            item.Category,
            username,
            item.Metadata));

        return png;
    }
}
