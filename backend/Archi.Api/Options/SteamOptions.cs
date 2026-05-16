namespace Archi.Api.Options;

public sealed class SteamOptions
{
    public const string SectionName = "Steam";

    public string ApiKey { get; set; } = string.Empty;
}
