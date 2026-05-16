using Archi.Api.Options;
using Archi.Api.Services.Cache;
using Microsoft.Extensions.Caching.StackExchangeRedis;

namespace Archi.Api.DependencyInjection;

public static class CacheServiceCollectionExtensions
{
    public static IServiceCollection AddArchiCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ArchiRedisCacheOptions>(configuration.GetSection(ArchiRedisCacheOptions.SectionName));

        var redisOptions = configuration.GetSection(ArchiRedisCacheOptions.SectionName).Get<ArchiRedisCacheOptions>()
                         ?? new ArchiRedisCacheOptions();
        var useRedis = redisOptions.Enabled &&
                       !string.IsNullOrWhiteSpace(redisOptions.ConnectionString);

        if (useRedis)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.ConnectionString;
                options.InstanceName = "archi:";
            });
            services.AddSingleton<ICacheService>(sp =>
                new DistributedCacheService(
                    sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>(),
                    "redis"));
        }
        else
        {
            services.AddDistributedMemoryCache();
            services.AddSingleton<ICacheService>(sp =>
                new DistributedCacheService(
                    sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>(),
                    "memory"));
        }

        return services;
    }
}
