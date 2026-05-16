using Archi.Api.Services.Feed;
using Archi.Api.Services.Vibe;

namespace Archi.Api.DependencyInjection;

public static class FeedServiceCollectionExtensions
{
    public static IServiceCollection AddArchiFeedServices(this IServiceCollection services)
    {
        services.AddScoped<IFeedService, FeedService>();
        services.AddScoped<IVibeService, VibeService>();
        return services;
    }
}
