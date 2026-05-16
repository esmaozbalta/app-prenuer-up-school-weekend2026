namespace Archi.Api.Options;

public sealed class IgdbOptions
{
    public const string SectionName = "Igdb";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string BaseUrl { get; set; } = "https://api.igdb.com/v4";

    public string TokenUrl { get; set; } = "https://id.twitch.tv/oauth2/token";

    /// <summary>Minimum delay between IGDB search calls (rate limit guard).</summary>
    public int MinIntervalMilliseconds { get; set; } = 250;
}
