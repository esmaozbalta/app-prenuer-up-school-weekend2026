namespace Archi.Api.Data;

/// <summary>
/// Canonical Supabase pooler connection (transaction mode, port 5432).
/// Project ref: gpwwqjbpfgqodkyckrjy — region pooler: aws-0-eu-central-1.
/// </summary>
public static class DatabaseConnection
{
    public const string DefaultConnectionString =
        "Host=aws-1-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.gpwwqjbpfgqodkyckrjy;Password=Eozb2002180505;SSL Mode=Require;";

    public static string Resolve(IConfiguration configuration)
    {
        var fromEnvironment = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        var fromConfiguration = configuration.GetConnectionString("DefaultConnection");

        // Prefer non-empty config file value when env looks stale (wrong pooler host / project ref).
        if (IsValidConnectionString(fromConfiguration) && !IsStaleEnvironmentOverride(fromEnvironment, fromConfiguration))
        {
            return fromConfiguration!.Trim();
        }

        if (IsValidConnectionString(fromEnvironment))
        {
            return fromEnvironment!.Trim();
        }

        if (IsValidConnectionString(fromConfiguration))
        {
            return fromConfiguration!.Trim();
        }

        return DefaultConnectionString;
    }

    private static bool IsValidConnectionString(string? connectionString) =>
        !string.IsNullOrWhiteSpace(connectionString) &&
        connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Reject env overrides that still point at the old aws-1 pooler or wrong Supabase project.
    /// </summary>
    private static bool IsStaleEnvironmentOverride(string? environmentValue, string? configurationValue)
    {
        if (string.IsNullOrWhiteSpace(environmentValue))
        {
            return false;
        }

        if (!IsValidConnectionString(configurationValue))
        {
            return false;
        }

        return ContainsStaleMarkers(environmentValue) &&
               !ContainsStaleMarkers(configurationValue!);
    }

    private static bool ContainsStaleMarkers(string value) =>
        value.Contains("aws-1-eu-central-1", StringComparison.OrdinalIgnoreCase) ||
        !value.Contains("gpwwqjbpfgqodkyckrjy", StringComparison.Ordinal) ||
        !value.Contains("aws-1-eu-central-1", StringComparison.OrdinalIgnoreCase);
}
