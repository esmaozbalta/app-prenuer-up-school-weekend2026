using Archi.Api.Options;
using Archi.Api.Services.Search;
using Polly;
using Polly.Extensions.Http;

namespace Archi.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddArchiSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExternalApiOptions>(configuration.GetSection(ExternalApiOptions.SectionName));
        services.Configure<TmdbOptions>(configuration.GetSection(TmdbOptions.SectionName));
        services.Configure<GoogleBooksOptions>(configuration.GetSection(GoogleBooksOptions.SectionName));
        services.Configure<IgdbOptions>(configuration.GetSection(IgdbOptions.SectionName));

        var igdbRetryPolicy = CreateIgdbRetryPolicy();

        services.AddHttpClient("tmdb", (sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TmdbOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Archi.Api/1.0");
        });

        services.AddHttpClient("googlebooks", (sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<GoogleBooksOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Archi.Api/1.0");
        });

        services.AddHttpClient("igdb", (sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<IgdbOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(8);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Archi.Api/1.0");
        }).AddPolicyHandler(igdbRetryPolicy);

        services.AddHttpClient("twitch", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Archi.Api/1.0");
        }).AddPolicyHandler(igdbRetryPolicy);

        services.AddScoped<ITmdbClient, TmdbClient>();
        services.AddScoped<IGoogleBooksClient, GoogleBooksClient>();
        services.AddScoped<IIgdbClient, IgdbClient>();
        services.AddScoped<IOmniSearchService, OmniSearchService>();

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateIgdbRetryPolicy() =>
        HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(response => (int)response.StatusCode is 429 or 503)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromMilliseconds(200 * Math.Pow(2, retryAttempt)));
}
