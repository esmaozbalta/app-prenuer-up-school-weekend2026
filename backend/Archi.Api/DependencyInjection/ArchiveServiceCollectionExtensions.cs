using Archi.Api.Services.Archive;

namespace Archi.Api.DependencyInjection;

public static class ArchiveServiceCollectionExtensions
{
    public static IServiceCollection AddArchiArchiveServices(this IServiceCollection services)
    {
        services.AddScoped<IArchiveService, ArchiveService>();
        return services;
    }
}
