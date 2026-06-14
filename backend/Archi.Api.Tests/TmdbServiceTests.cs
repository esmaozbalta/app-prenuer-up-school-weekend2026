using System.Net;
using Archi.Api.Services.Tmdb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Archi.Api.Tests;

public sealed class TmdbServiceTests
{
    [Fact]
    public async Task SearchAsync_maps_movie_and_tv_from_multi_search()
    {
        const string json = """
            {
              "results": [
                {
                  "id": 603,
                  "media_type": "movie",
                  "title": "The Matrix",
                  "overview": "Neo story",
                  "poster_path": "/matrix.jpg",
                  "release_date": "1999-03-31"
                },
                {
                  "id": 1399,
                  "media_type": "tv",
                  "name": "Breaking Bad",
                  "overview": "Chemistry teacher",
                  "poster_path": "/bb.jpg",
                  "first_air_date": "2008-01-20"
                },
                {
                  "id": 1,
                  "media_type": "person",
                  "name": "Ignored Person"
                }
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/")
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tmdb:ApiKey"] = "test-key",
                ["Tmdb:BaseUrl"] = "https://api.themoviedb.org/3"
            })
            .Build();

        var service = new TmdbService(httpClient, configuration, NullLogger<TmdbService>.Instance);
        var results = await service.SearchAsync("matrix");

        Assert.Equal(2, results.Count);

        Assert.Equal("603", results[0].ExternalId);
        Assert.Equal("The Matrix", results[0].Title);
        Assert.Equal("Movie", results[0].Category);
        Assert.Equal("1999", results[0].Year);
        Assert.Equal("Neo story", results[0].Description);
        Assert.Equal("https://image.tmdb.org/t/p/w500/matrix.jpg", results[0].ImageUrl);

        Assert.Equal("1399", results[1].ExternalId);
        Assert.Equal("Breaking Bad", results[1].Title);
        Assert.Equal("TV Show", results[1].Category);
        Assert.Equal("2008", results[1].Year);
    }

    [Fact]
    public async Task SearchAsync_returns_empty_poster_path_as_empty_image_url()
    {
        const string json = """
            {
              "results": [
                {
                  "id": 10,
                  "media_type": "movie",
                  "title": "No Poster",
                  "overview": "Test",
                  "poster_path": null,
                  "release_date": "2020-01-01"
                }
              ]
            }
            """;

        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/")
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tmdb:ApiKey"] = "test-key",
                ["Tmdb:BaseUrl"] = "https://api.themoviedb.org/3"
            })
            .Build();

        var service = new TmdbService(httpClient, configuration, NullLogger<TmdbService>.Instance);
        var results = await service.SearchAsync("test");

        Assert.Single(results);
        Assert.Equal(string.Empty, results[0].ImageUrl);
    }

    [Fact]
    public async Task SearchAsync_returns_empty_list_when_api_key_missing()
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)))
        {
            BaseAddress = new Uri("https://api.themoviedb.org/3/")
        };

        var configuration = new ConfigurationBuilder().Build();
        var service = new TmdbService(httpClient, configuration, NullLogger<TmdbService>.Instance);

        var results = await service.SearchAsync("matrix");

        Assert.Empty(results);
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
