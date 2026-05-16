namespace Archi.Api.Services.Cache;

public interface ICacheService
{
    string ProviderName { get; }

    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class;

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
        where T : class;

    Task<bool> PingAsync(CancellationToken cancellationToken = default);
}
