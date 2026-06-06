using Archi.Api.Options;
using Archi.Api.Services.Ai;

namespace Archi.Api.DependencyInjection;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddArchiAiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));

        services.AddHttpClient<OpenAiService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Archi.Api/1.0");
        });

        services.AddScoped<IAiService, OpenAiService>();

        return services;
    }
}
