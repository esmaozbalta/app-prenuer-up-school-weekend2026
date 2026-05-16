using System.Net;
using Archi.Api.Services.Search;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ExternalApiOptions = Archi.Api.Options.ExternalApiOptions;
using TmdbOptions = Archi.Api.Options.TmdbOptions;

namespace Archi.Api.Tests;

public sealed class TmdbClientTests
{
    [Fact]
    public async Task SearchAsync_maps_movie_results_from_http_response()
    {
        const string json = """
            {
              "results": [
                {
                  "id": 603,
                  "title": "The Matrix",
                  "overview": "Neo story",
                  "poster_path": "/poster.jpg",
                  "release_date": "1999-03-31"
                }
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var factory = new StubHttpClientFactory(handler);
        var client = new TmdbClient(
            factory,
            Microsoft.Extensions.Options.Options.Create(new TmdbOptions { ApiKey = "test-key", BaseUrl = "https://api.themoviedb.org/3" }),
            Microsoft.Extensions.Options.Options.Create(new ExternalApiOptions { UseStubs = false }),
            NullLogger<TmdbClient>.Instance);

        var results = await client.SearchAsync("matrix");

        Assert.Single(results);
        Assert.Equal("603", results[0].ExternalId);
        Assert.Equal("The Matrix", results[0].Title);
        Assert.Equal(1999, results[0].Metadata.Year);
        Assert.Contains("poster.jpg", results[0].Metadata.CoverUrl, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SearchAsync_returns_stub_results_when_enabled()
    {
        var factory = new StubHttpClientFactory(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)));
        var client = new TmdbClient(
            factory,
            Microsoft.Extensions.Options.Options.Create(new TmdbOptions()),
            Microsoft.Extensions.Options.Options.Create(new ExternalApiOptions { UseStubs = true }),
            NullLogger<TmdbClient>.Instance);

        var results = await client.SearchAsync("query");

        Assert.Single(results);
        Assert.Contains("Stub Film", results[0].Title, StringComparison.Ordinal);
    }

    private sealed class StubHttpClientFactory(StubHttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new(handler)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/")
        };
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(responder(request));
    }
}
