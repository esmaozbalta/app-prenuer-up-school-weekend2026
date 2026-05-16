using Archi.Api.Data;
using Microsoft.Extensions.Configuration;

namespace Archi.Api.Tests;

public sealed class DatabaseConnectionTests
{
    [Fact]
    public void Resolve_uses_canonical_when_config_empty()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var resolved = DatabaseConnection.Resolve(configuration);

        Assert.Contains("aws-0-eu-central-1.pooler.supabase.com", resolved);
        Assert.Contains("postgres.gpwwqjbpfgqodkyckrjy", resolved);
    }

    [Fact]
    public void Resolve_prefers_config_over_stale_aws1_environment()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = DatabaseConnection.DefaultConnectionString
            })
            .Build();

        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Host=aws-1-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.wrong;Password=x;SSL Mode=Require;");

        try
        {
            var resolved = DatabaseConnection.Resolve(configuration);
            Assert.Contains("aws-0-eu-central-1", resolved);
            Assert.Contains("gpwwqjbpfgqodkyckrjy", resolved);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        }
    }
}
