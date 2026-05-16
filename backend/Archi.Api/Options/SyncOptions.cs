namespace Archi.Api.Options;

public sealed class SyncOptions
{
    public const string SectionName = "Sync";

    public bool UseStubs { get; set; } = true;

    public int MaxSteamGames { get; set; } = 500;

    public int MaxCsvRows { get; set; } = 500;
}
