using Archi.Api.Contracts.Archive;

namespace Archi.Api.Services.Archive;

public interface IArchiveService
{
    Task<ArchiveItemResponse> AddAsync(Guid userId, AddArchiveRequest request, CancellationToken cancellationToken = default);

    Task<ArchiveListResponse?> ListByUserAsync(
        Guid targetUserId,
        Guid? callerUserId,
        int limit,
        CancellationToken cancellationToken = default);
}
