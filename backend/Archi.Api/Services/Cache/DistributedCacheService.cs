using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Archi.Api.Services.Cache;

public sealed class DistributedCacheService(
    IDistributedCache distributedCache,
    string providerName) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public string ProviderName { get; } = providerName;

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        where T : class
    {
        var bytes = await distributedCache.GetAsync(key, cancellationToken);
        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }

        return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        await distributedCache.SetAsync(
            key,
            bytes,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl },
            cancellationToken);
    }

    public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
    {
        const string probeKey = "archi:health:probe";
        try
        {
            await distributedCache.SetAsync(
                probeKey,
                "1"u8.ToArray(),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5) },
                cancellationToken);
            var value = await distributedCache.GetAsync(probeKey, cancellationToken);
            return value is { Length: > 0 };
        }
        catch
        {
            return false;
        }
    }
}
