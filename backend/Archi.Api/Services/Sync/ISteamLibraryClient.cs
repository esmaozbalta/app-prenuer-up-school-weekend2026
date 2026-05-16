namespace Archi.Api.Services.Sync;

public interface ISteamLibraryClient
{
    Task<IReadOnlyList<SteamGameEntry>> GetOwnedGamesAsync(
        string steamProfileUrl,
        int maxGames,
        CancellationToken cancellationToken = default);
}

public sealed record SteamGameEntry(
    string AppId,
    string Name,
    int? PlaytimeMinutes,
    string? CoverUrl);
