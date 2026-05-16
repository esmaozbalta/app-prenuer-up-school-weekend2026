namespace Archi.Api.Options;

public sealed class ArchiRedisCacheOptions
{
    public const string SectionName = "Cache:Redis";

    public string? ConnectionString { get; set; }

    public bool Enabled { get; set; } = true;
}
