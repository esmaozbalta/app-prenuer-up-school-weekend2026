using Archi.Api.Contracts.Chat;

namespace Archi.Api.Services.Ai;

public interface IAiService
{
    Task<ChatResponse> ChatAsync(string userMessage, CancellationToken cancellationToken = default);
}
