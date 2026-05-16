using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Auth;
using Archi.Api.Contracts.Search;
using Archi.Api.Contracts.Sync;
using Archi.Api.Data;
using Archi.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Archi.Api.Tests;

public sealed class Sprint4EndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public Sprint4EndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SteamSync_CompletesWithHighSuccessRate()
    {
        var client = CreateClient();
        var token = await RegisterAndGetTokenAsync(client, "steam@example.com", "steamuser");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/sync/steam")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = JsonContent.Create(new StartSteamSyncRequest(
                "https://steamcommunity.com/profiles/76561198000000000"))
        };

        var startResponse = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Accepted, startResponse.StatusCode);
        var started = await startResponse.Content.ReadFromJsonAsync<StartSteamSyncResponse>();
        Assert.NotNull(started);

        var job = await WaitForJobAsync(client, token, started!.JobId);
        Assert.Equal(SyncJobStatus.Completed, job.Status);
        Assert.NotNull(job.Metrics);
        Assert.True(job.Metrics!.SuccessRate >= 0.95);
        Assert.True(job.Metrics.Inserted > 0);
    }

    [Fact]
    public async Task GoodreadsCsv_ImportsBooks_AndSkipsDuplicatesOnSecondRun()
    {
        var client = CreateClient();
        var token = await RegisterAndGetTokenAsync(client, "csv@example.com", "csvuser");
        var csv = """
            Title,Author,ISBN,Bookshelves
            Dune,Frank Herbert,9780441172719,read
            Hyperion,Dan Simmons,9780553283686,currently-reading
            """;

        var firstJobId = await UploadCsvAsync(client, token, csv);
        var firstJob = await WaitForJobAsync(client, token, firstJobId);
        Assert.Equal(SyncJobStatus.Completed, firstJob.Status);
        Assert.Equal(2, firstJob.Metrics!.Inserted);

        var secondJobId = await UploadCsvAsync(client, token, csv);
        var secondJob = await WaitForJobAsync(client, token, secondJobId);
        Assert.Equal(SyncJobStatus.Completed, secondJob.Status);
        Assert.Equal(2, secondJob.Metrics!.SkippedDuplicates);
        Assert.Equal(0, secondJob.Metrics.Inserted);
    }

    [Fact]
    public async Task LetterboxdCsv_ImportsMovies()
    {
        var client = CreateClient();
        var token = await RegisterAndGetTokenAsync(client, "letterboxd@example.com", "lbuser");
        var csv = """
            Date,Name,Year,Letterboxd URI
            2024-01-01,Inception,2010,https://boxd.it/inception
            """;

        var jobId = await UploadCsvAsync(client, token, csv);
        var job = await WaitForJobAsync(client, token, jobId);
        Assert.Equal(SyncJobStatus.Completed, job.Status);
        Assert.Equal(1, job.Metrics!.Inserted);

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var movie = await db.ArchiveItems.AsNoTracking()
            .FirstOrDefaultAsync(item => item.ExternalId.Contains("inception", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(movie);
        Assert.Equal(MediaCategories.Movie, movie!.Category);
    }

    [Fact]
    public async Task ShareCard_ReturnsPng_ForPublicArchiveItem()
    {
        var client = CreateClient();
        var token = await RegisterAndGetTokenAsync(client, "share@example.com", "shareuser");
        var itemId = await AddArchiveItemViaApiAsync(client, token, "share-item-1");

        var response = await client.GetAsync($"/api/v1/share-card/{itemId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 1000);
        Assert.Equal(0x89, bytes[0]);
        Assert.Equal(0x50, bytes[1]);
    }

    [Fact]
    public async Task FailedSteamJob_CanRetry()
    {
        using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var token = await RegisterAndGetTokenAsync(client, "retry@example.com", "retryuser");

        await using var scope = factory.Services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<Archi.Api.Services.Sync.ISyncJobStore>();
        var userId = await GetUserIdByEmailAsync(scope.ServiceProvider, "retry@example.com");

        var failedJob = new Archi.Api.Services.Sync.SyncJobRecord
        {
            JobId = Guid.NewGuid(),
            UserId = userId,
            Type = SyncJobType.Steam,
            Status = SyncJobStatus.Failed,
            SteamProfileUrl = "https://steamcommunity.com/profiles/76561198000000000",
            ErrorMessage = "forced failure"
        };
        store.Add(failedJob);

        using var retryRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/sync/jobs/{failedJob.JobId}/retry")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
        };

        var retryResponse = await client.SendAsync(retryRequest);
        Assert.Equal(HttpStatusCode.Accepted, retryResponse.StatusCode);

        var completed = await WaitForJobAsync(client, token, failedJob.JobId);
        Assert.Equal(SyncJobStatus.Completed, completed.Status);
    }

    private HttpClient CreateClient() =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

    private static async Task<SyncJobResponse> WaitForJobAsync(
        HttpClient client,
        string token,
        Guid jobId,
        int maxAttempts = 40)
    {
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/sync/jobs/{jobId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var job = await response.Content.ReadFromJsonAsync<SyncJobResponse>();
            Assert.NotNull(job);

            if (job!.Status is SyncJobStatus.Completed or SyncJobStatus.Failed)
            {
                return job;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException($"Sync job {jobId} did not complete in time.");
    }

    private static async Task<Guid> UploadCsvAsync(HttpClient client, string token, string csv)
    {
        using var content = new MultipartFormDataContent();
        var bytes = Encoding.UTF8.GetBytes(csv);
        content.Add(new ByteArrayContent(bytes), "file", "library.csv");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/sync/goodreads")
        {
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) },
            Content = content
        };

        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CsvImportResponse>();
        Assert.NotNull(body);
        return body!.JobId;
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
                new ArchiveMetadata { Year = 2020 },
                ArchiveItemStatus.Done,
                null,
                null))
        };

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var created = await response.Content.ReadFromJsonAsync<ArchiveItemResponse>();
        return created!.Id;
    }

    private static async Task<Guid> GetUserIdByEmailAsync(IServiceProvider services, string email)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var normalized = email.ToLowerInvariant();
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.NormalizedEmail == normalized);
        return user.Id;
    }
}
