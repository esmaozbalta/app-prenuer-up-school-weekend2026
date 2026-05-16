using Archi.Api.Contracts.Archive;

namespace Archi.Api.Services.Share;

public interface IShareCardRenderer
{
    byte[] Render(ShareCardModel model);
}

public sealed record ShareCardModel(
    string Title,
    string Category,
    string Username,
    ArchiveMetadata Metadata);
