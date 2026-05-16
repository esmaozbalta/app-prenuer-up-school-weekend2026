using Archi.Api.Options;
using Archi.Api.Services.Archive;
using Archi.Api.Services.Share;
using Archi.Api.Services.Sync;

namespace Archi.Api.DependencyInjection;

public static class SyncServiceCollectionExtensions
{
    public static IServiceCollection AddArchiSyncServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SyncOptions>(configuration.GetSection(SyncOptions.SectionName));
        services.Configure<SteamOptions>(configuration.GetSection(SteamOptions.SectionName));

        services.AddSingleton<ISyncJobStore, InMemorySyncJobStore>();
        services.AddSingleton<SyncJobQueue>();
        services.AddHostedService<SyncJobBackgroundService>();

        services.AddScoped<SyncJobProcessor>();
        services.AddScoped<ISyncService, SyncService>();
        services.AddScoped<IArchiveBulkImporter, ArchiveBulkImporter>();
        services.AddSingleton<CsvImportParser>();
        services.AddScoped<ISteamLibraryClient, SteamLibraryClient>();

        services.AddHttpClient("steam", client =>
        {
            client.BaseAddress = new Uri("https://api.steampowered.com/");
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Archi.Api/1.0");
        });

        services.AddSingleton<IShareCardRenderer, SkiaShareCardRenderer>();
        services.AddScoped<IShareCardService, ShareCardService>();

        return services;
    }
}
