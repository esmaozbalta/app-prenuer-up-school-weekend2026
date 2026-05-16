using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Auth;
using Archi.Api.Contracts.Search;
using Archi.Api.Models;
using Archi.Api.Services.Search;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Archi.Api.Tests;

public sealed class Sprint2EndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public Sprint2EndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task OmniSearch_ReturnsGroupedResults_WithStubProviders()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/search/omni?q=matrix");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OmniSearchResponse>();
        Assert.NotNull(body);
        Assert.NotEmpty(body!.Movies);
        Assert.NotEmpty(body.Books);
        Assert.NotEmpty(body.Games);
    }

    [Fact]
    public async Task OmniSearch_ReturnsBadRequest_WhenQueryMissing()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/search/omni?q=");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task OmniSearch_SecondCall_UsesCache_NotProvider()
    {
        var counting = new CountingOmniSearchService();
        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IOmniSearchService>();
                services.AddSingleton<IOmniSearchService>(counting);
            });
        });

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var first = await client.GetAsync("/api/v1/search/omni?q=matrix");
        var second = await client.GetAsync("/api/v1/search/omni?q=matrix");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(1, counting.CallCount);
    }

    [Fact]
    public async Task ArchiveAdd_ThenList_ReturnsItem()
    {
        var client = CreateClient();
        var token = await RegisterAndGetTokenAsync(client, "archive1@example.com", "archiveuser1");

        var addRequest = new AddArchiveRequest(
            "tmdb-603",
            MediaCategories.Movie,
            "The Matrix",
            new ArchiveMetadata { Year = 1999 },
            ArchiveItemStatus.InProgress,
            ["mind-bending", "classic"],
            null);

        using var addMessage = new HttpRequestMessage(HttpMethod.Post, "/api/v1/archive/add");
        addMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        addMessage.Content = JsonContent.Create(addRequest);

        var addResponse = await client.SendAsync(addMessage);
        Assert.Equal(HttpStatusCode.Created, addResponse.StatusCode);
        var created = await addResponse.Content.ReadFromJsonAsync<ArchiveItemResponse>();
        Assert.NotNull(created);
        Assert.Equal("tmdb-603", created!.ExternalId);
        Assert.Equal(2, created.Tags.Count);

        var listResponse = await client.GetAsync($"/api/v1/archive/{created.UserId}");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<ArchiveListResponse>();
        Assert.NotNull(list);
        Assert.Single(list!.Items);
        Assert.Equal(created.Id, list.Items[0].Id);
    }

    [Fact]
    public async Task ArchiveAdd_ReturnsConflict_WhenDuplicate()
    {
        var client = CreateClient();
        var token = await RegisterAndGetTokenAsync(client, "dup@example.com", "dupuser");

        var addRequest = new AddArchiveRequest(
            "ext-1",
            MediaCategories.Book,
            "Duplicate Book",
            null,
            ArchiveItemStatus.Wishlist,
            null,
            null);

        using var first = new HttpRequestMessage(HttpMethod.Post, "/api/v1/archive/add");
        first.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        first.Content = JsonContent.Create(addRequest);
        Assert.Equal(HttpStatusCode.Created, (await client.SendAsync(first)).StatusCode);

        using var second = new HttpRequestMessage(HttpMethod.Post, "/api/v1/archive/add");
        second.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        second.Content = JsonContent.Create(addRequest);
        var duplicateResponse = await client.SendAsync(second);

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task ArchiveList_ReturnsNotFound_WhenUserIsPrivateAndNotOwner()
    {
        var client = CreateClient();
        var ownerId = await RegisterAndGetUserIdAsync(client, "private-archive@example.com", "privatearchive");
        var ownerToken = await LoginAsync(client, "private-archive@example.com");

        using var lockProfile = new HttpRequestMessage(HttpMethod.Patch, "/api/v1/profile/privacy")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", ownerToken) },
            Content = JsonContent.Create(new { isPrivate = true })
        };
        (await client.SendAsync(lockProfile)).EnsureSuccessStatusCode();

        var listResponse = await client.GetAsync($"/api/v1/archive/{ownerId}");
        Assert.Equal(HttpStatusCode.NotFound, listResponse.StatusCode);
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private static async Task<string> RegisterAndGetTokenAsync(
        HttpClient client,
        string email,
        string username)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, username, "testpass123"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(body);
        return body!.AccessToken;
    }

    private static async Task<Guid> RegisterAndGetUserIdAsync(
        HttpClient client,
        string email,
        string username)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, username, "testpass123"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(body);
        return body!.UserId;
    }

    private static async Task<string> LoginAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(email, "testpass123"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        return body!.AccessToken;
    }

    private sealed class CountingOmniSearchService : IOmniSearchService
    {
        private int _callCount;

        public int CallCount => _callCount;

        public Task<OmniSearchResponse> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            return Task.FromResult(new OmniSearchResponse(
                [new ExternalMediaItemDto("1", MediaCategories.Movie, $"Movie {query}", new ArchiveMetadata())],
                [new ExternalMediaItemDto("2", MediaCategories.Book, $"Book {query}", new ArchiveMetadata())],
                [new ExternalMediaItemDto("3", MediaCategories.Game, $"Game {query}", new ArchiveMetadata())]));
        }
    }
}
