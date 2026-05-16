using Archi.Api.Contracts.Archive;
using Archi.Api.Contracts.Search;

namespace Archi.Api.Services.Search;

internal static class SearchStubs
{
    public static IReadOnlyList<ExternalMediaItemDto> Movies(string query) =>
    [
        new(
            "stub-movie-1",
            MediaCategories.Movie,
            $"Stub Film: {query}",
            new ArchiveMetadata
            {
                CoverUrl = "https://placehold.co/300x450",
                Year = 2024,
                Director = "Stub Director",
                Summary = "Stub TMDB result for development."
            })
    ];

    public static IReadOnlyList<ExternalMediaItemDto> Books(string query) =>
    [
        new(
            "stub-book-1",
            MediaCategories.Book,
            $"Stub Book: {query}",
            new ArchiveMetadata
            {
                CoverUrl = "https://placehold.co/300x450",
                Year = 2023,
                Author = "Stub Author",
                Summary = "Stub Google Books result for development."
            })
    ];

    public static IReadOnlyList<ExternalMediaItemDto> Games(string query) =>
    [
        new(
            "stub-game-1",
            MediaCategories.Game,
            $"Stub Game: {query}",
            new ArchiveMetadata
            {
                CoverUrl = "https://placehold.co/300x450",
                Year = 2022,
                Platform = "PC",
                Summary = "Stub IGDB result for development."
            })
    ];
}
