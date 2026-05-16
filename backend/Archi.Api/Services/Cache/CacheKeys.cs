namespace Archi.Api.Services.Cache;

public static class CacheKeys
{
    private const string Prefix = "archi:";

    public static string OmniSearch(string query) =>
        $"{Prefix}search:omni:{NormalizeQuery(query)}";

    public static string IgdbOAuthToken() => $"{Prefix}igdb:oauth:token";

    public static string GlobalFeed(string? cursor, int limit) =>
        $"{Prefix}feed:global:{cursor ?? "first"}:{limit}";

    private static string NormalizeQuery(string query) =>
        query.Trim().ToLowerInvariant();
}
