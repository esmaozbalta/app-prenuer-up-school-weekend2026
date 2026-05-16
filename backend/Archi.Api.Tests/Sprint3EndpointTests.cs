using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Auth;
using Archi.Api.Contracts.Feed;
using Archi.Api.Contracts.Search;
using Archi.Api.Contracts.Vibe;
using Archi.Api.Data;
using Archi.Api.Models;
using Archi.Api.Services.Feed;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Archi.Api.Tests;

public sealed class Sprint3EndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public Sprint3EndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GlobalFeed_ReturnsOnlyPublicUsersItems()
    {
        var client = CreateClient();
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var publicUser = await CreateUserAsync(db, "feed-public@example.com", "feedpublic", isPrivate: false);
        var privateUser = await CreateUserAsync(db, "feed-private@example.com", "feedprivate", isPrivate: true);

        await AddArchiveItemAsync(db, publicUser.Id, "public-1", minutesAgo: 10);
        await AddArchiveItemAsync(db, privateUser.Id, "private-1", minutesAgo: 5);

        var response = await client.GetAsync("/api/v1/feed/global?limit=20");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var feed = await response.Content.ReadFromJsonAsync<GlobalFeedResponse>();
        Assert.NotNull(feed);
        Assert.Contains(feed!.Items, item => item.ExternalId == "public-1");
        Assert.DoesNotContain(feed.Items, item => item.ExternalId == "private-1");
    }

    [Fact]
    public async Task GlobalFeed_SupportsCursorPagination()
    {
        using var isolatedFactory = new CustomWebApplicationFactory();
        var client = isolatedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        await using var scope = isolatedFactory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await CreateUserAsync(db, "feed-cursor@example.com", "feedcursor", isPrivate: false);
        await AddArchiveItemAsync(db, user.Id, "item-oldest", minutesAgo: 30);
        await AddArchiveItemAsync(db, user.Id, "item-middle", minutesAgo: 20);
        await AddArchiveItemAsync(db, user.Id, "item-newest", minutesAgo: 10);

        var firstPage = await client.GetFromJsonAsync<GlobalFeedResponse>("/api/v1/feed/global?limit=2");
        Assert.NotNull(firstPage);
        Assert.Equal(2, firstPage!.Items.Count);
        Assert.False(string.IsNullOrWhiteSpace(firstPage.NextCursor));

        var secondPage = await client.GetFromJsonAsync<GlobalFeedResponse>(
            $"/api/v1/feed/global?limit=2&cursor={Uri.EscapeDataString(firstPage.NextCursor!)}");
        Assert.NotNull(secondPage);
        Assert.Single(secondPage!.Items);
        Assert.DoesNotContain(secondPage.Items, item => firstPage.Items.Any(first => first.Id == item.Id));
    }

    [Fact]
    public async Task GlobalFeed_SecondCall_UsesCache()
    {
        var counting = new CountingFeedService();
        using var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IFeedService>();
                services.AddSingleton<IFeedService>(counting);
            });
        });

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/feed/global")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/feed/global")).StatusCode);
        Assert.Equal(1, counting.CallCount);
    }

    [Fact]
    public async Task AddVibe_ReturnsTopTags_OnContentPage()
    {
        var client = CreateClient();
        var token = await RegisterAndGetTokenAsync(client, "vibe@example.com", "vibeuser");

        var itemId = await AddArchiveItemViaApiAsync(client, token, "vibe-item-1");

        for (var i = 0; i < 3; i++)
        {
            await AddVibeViaApiAsync(client, token, itemId, "cozy");
        }

        await AddVibeViaApiAsync(client, token, itemId, "epic");
        await AddVibeViaApiAsync(client, token, itemId, "epic");

        var topResponse = await client.GetAsync($"/api/v1/archive/{itemId}/vibes/top?limit=5");
        Assert.Equal(HttpStatusCode.OK, topResponse.StatusCode);

        var top = await topResponse.Content.ReadFromJsonAsync<TopVibeTagsResponse>();
        Assert.NotNull(top);
        Assert.Equal(itemId, top!.ItemId);
        Assert.Equal(2, top.Tags.Count);
        Assert.Equal("cozy", top.Tags[0].TagName);
        Assert.Equal(3, top.Tags[0].Count);
        Assert.Equal("epic", top.Tags[1].TagName);
        Assert.Equal(2, top.Tags[1].Count);
    }

    [Fact]
    public async Task TopVibes_ReturnsNotFound_ForPrivateItem_WhenNotOwner()
    {
        var client = CreateClient();
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var privateUser = await CreateUserAsync(db, "vibe-private@example.com", "vibeprivate", isPrivate: true);
        var itemId = await AddArchiveItemAsync(db, privateUser.Id, "hidden-item", minutesAgo: 1);

        var response = await client.GetAsync($"/api/v1/archive/{itemId}/vibes/top");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GlobalFeed_ReturnsBadRequest_ForInvalidCursor()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/api/v1/feed/global?cursor=not-a-valid-cursor");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private static async Task<User> CreateUserAsync(
        AppDbContext db,
        string email,
        string username,
        bool isPrivate)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var normalizedUsername = username.ToLowerInvariant();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = normalizedEmail,
            Username = username,
            NormalizedUsername = normalizedUsername,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("testpass123"),
            IsPrivate = isPrivate,
            CreatedAt = DateTimeOffset.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    private static async Task<Guid> AddArchiveItemAsync(
        AppDbContext db,
        Guid userId,
        string externalId,
        int minutesAgo)
    {
        var item = new ArchiveItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExternalId = externalId,
            Category = MediaCategories.Movie,
            Title = $"Title {externalId}",
            Metadata = new ArchiveMetadata { Year = 2024 },
            Status = ArchiveItemStatus.Done,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-minutesAgo)
        };
        db.ArchiveItems.Add(item);
        await db.SaveChangesAsync();
        return item.Id;
    }

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
        return body!.AccessToken;
    }

    private static async Task<Guid> AddArchiveItemViaApiAsync(HttpClient client, string token, string externalId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/archive/add")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new AddArchiveRequest(
                externalId,
                MediaCategories.Movie,
                $"Movie {externalId}",
                null,
                ArchiveItemStatus.Done,
                null,
                null))
        };

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<ArchiveItemResponse>();
        return created!.Id;
    }

    private static async Task AddVibeViaApiAsync(
        HttpClient client,
        string token,
        Guid itemId,
        string tagName)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/archive/{itemId}/vibes")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new AddVibeTagRequest(tagName))
        };
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private sealed class CountingFeedService : IFeedService
    {
        private int _callCount;

        public int CallCount => _callCount;

        public Task<GlobalFeedResponse> GetGlobalFeedAsync(
            string? cursor,
            int limit,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _callCount);
            return Task.FromResult(new GlobalFeedResponse([], null));
        }
    }
}
