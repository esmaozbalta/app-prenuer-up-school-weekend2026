using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Search;
using Archi.Api.Services.Search;
using Microsoft.Extensions.Logging.Abstractions;

namespace Archi.Api.Tests;

public sealed class OmniSearchServiceTests
{
    [Fact]
    public async Task SearchAsync_aggregates_all_providers_in_parallel()
    {
        var service = new OmniSearchService(
            new FakeTmdbClient(),
            new FakeGoogleBooksClient(),
            new FakeIgdbClient(),
            NullLogger<OmniSearchService>.Instance);

        var result = await service.SearchAsync("matrix");

        Assert.Single(result.Movies);
        Assert.Single(result.Books);
        Assert.Single(result.Games);
        Assert.Equal(MediaCategories.Movie, result.Movies[0].Category);
        Assert.Equal(MediaCategories.Book, result.Books[0].Category);
        Assert.Equal(MediaCategories.Game, result.Games[0].Category);
    }

    [Fact]
    public async Task SearchAsync_continues_when_one_provider_throws()
    {
        var service = new OmniSearchService(
            new ThrowingTmdbClient(),
            new FakeGoogleBooksClient(),
            new FakeIgdbClient(),
            NullLogger<OmniSearchService>.Instance);

        var result = await service.SearchAsync("matrix");

        Assert.Empty(result.Movies);
        Assert.Single(result.Books);
        Assert.Single(result.Games);
    }

    private sealed class FakeTmdbClient : ITmdbClient
    {
        public Task<IReadOnlyList<ExternalMediaItemDto>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ExternalMediaItemDto>>(
            [
                new("1", MediaCategories.Movie, $"Movie {query}", new ArchiveMetadata())
            ]);
    }

    private sealed class FakeGoogleBooksClient : IGoogleBooksClient
    {
        public Task<IReadOnlyList<ExternalMediaItemDto>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ExternalMediaItemDto>>(
            [
                new("2", MediaCategories.Book, $"Book {query}", new ArchiveMetadata())
            ]);
    }

    private sealed class FakeIgdbClient : IIgdbClient
    {
        public Task<IReadOnlyList<ExternalMediaItemDto>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ExternalMediaItemDto>>(
            [
                new("3", MediaCategories.Game, $"Game {query}", new ArchiveMetadata())
            ]);
    }

    private sealed class ThrowingTmdbClient : ITmdbClient
    {
        public Task<IReadOnlyList<ExternalMediaItemDto>> SearchAsync(
            string query,
            CancellationToken cancellationToken = default) =>
            throw new HttpRequestException("TMDB unavailable");
    }
}
