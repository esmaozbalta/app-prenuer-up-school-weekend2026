using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Archi.Api.Options;
using Microsoft.Extensions.Options;

namespace Archi.Api.Services.Sync;

public sealed partial class SteamLibraryClient(
    IHttpClientFactory httpClientFactory,
    IOptions<SteamOptions> steamOptions,
    IOptions<SyncOptions> syncOptions,
    ILogger<SteamLibraryClient> logger) : ISteamLibraryClient
{
    public async Task<IReadOnlyList<SteamGameEntry>> GetOwnedGamesAsync(
        string steamProfileUrl,
        int maxGames,
        CancellationToken cancellationToken = default)
    {
        if (syncOptions.Value.UseStubs)
        {
            return BuildStubGames(Math.Min(maxGames, syncOptions.Value.MaxSteamGames));
        }

        var apiKey = steamOptions.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogWarning("Steam API key is not configured; returning empty library.");
            return [];
        }

        var steamId = await ResolveSteamIdAsync(steamProfileUrl, apiKey, cancellationToken);
        if (steamId is null)
        {
            return [];
        }

        var client = httpClientFactory.CreateClient("steam");
        var url =
            $"IPlayerService/GetOwnedGames/v1/?key={Uri.EscapeDataString(apiKey)}&steamid={steamId}&include_appinfo=1&include_played_free_games=1&format=json";

        using var response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("Steam GetOwnedGames failed with status {StatusCode}", response.StatusCode);
            return [];
        }

        var payload = await response.Content.ReadFromJsonAsync<SteamOwnedGamesResponse>(cancellationToken);
        if (payload?.Response?.Games is null)
        {
            return [];
        }

        return payload.Response.Games
            .Where(game => game.AppId > 0 && !string.IsNullOrWhiteSpace(game.Name))
            .Take(maxGames)
            .Select(game => new SteamGameEntry(
                game.AppId.ToString(),
                game.Name!,
                game.PlaytimeForever,
                $"https://cdn.cloudflare.steamstatic.com/steam/apps/{game.AppId}/header.jpg"))
            .ToList();
    }

    private async Task<string?> ResolveSteamIdAsync(
        string profileUrl,
        string apiKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(profileUrl))
        {
            return null;
        }

        var numericMatch = SteamIdRegex().Match(profileUrl);
        if (numericMatch.Success)
        {
            return numericMatch.Groups[1].Value;
        }

        var vanityMatch = VanityRegex().Match(profileUrl);
        if (!vanityMatch.Success)
        {
            return null;
        }

        var vanity = vanityMatch.Groups[1].Value;
        var client = httpClientFactory.CreateClient("steam");
        var url =
            $"ISteamUser/ResolveVanityURL/v1/?key={Uri.EscapeDataString(apiKey)}&vanityurl={Uri.EscapeDataString(vanity)}&format=json";
        using var response = await client.GetAsync(url, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var payload = await response.Content.ReadFromJsonAsync<SteamVanityResponse>(cancellationToken);
        return payload?.Response?.SteamId;
    }

    private static IReadOnlyList<SteamGameEntry> BuildStubGames(int count)
    {
        var games = new List<SteamGameEntry>(count);
        for (var i = 1; i <= count; i++)
        {
            games.Add(new SteamGameEntry(
                $"stub-{i}",
                $"Stub Game {i}",
                120 + i,
                "https://placehold.co/460x215"));
        }

        return games;
    }

    [GeneratedRegex(@"steamcommunity\.com/profiles/(\d+)", RegexOptions.IgnoreCase)]
    private static partial Regex SteamIdRegex();

    [GeneratedRegex(@"steamcommunity\.com/id/([^/""?]+)", RegexOptions.IgnoreCase)]
    private static partial Regex VanityRegex();

    private sealed class SteamOwnedGamesResponse
    {
        [JsonPropertyName("response")]
        public SteamOwnedGamesPayload? Response { get; set; }
    }

    private sealed class SteamOwnedGamesPayload
    {
        [JsonPropertyName("games")]
        public List<SteamOwnedGame>? Games { get; set; }
    }

    private sealed class SteamOwnedGame
    {
        [JsonPropertyName("appid")]
        public int AppId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("playtime_forever")]
        public int? PlaytimeForever { get; set; }
    }

    private sealed class SteamVanityResponse
    {
        [JsonPropertyName("response")]
        public SteamVanityPayload? Response { get; set; }
    }

    private sealed class SteamVanityPayload
    {
        [JsonPropertyName("steamid")]
        public string? SteamId { get; set; }
    }
}
