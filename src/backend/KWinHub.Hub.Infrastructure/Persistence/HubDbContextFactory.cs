using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KWinHub.Hub.Infrastructure.Persistence;

/// <summary>
/// Used by the EF Core tooling (`dotnet ef migrations`) at design time so the
/// Hub model can be built without booting the API host. The connection string is
/// only used to pick the provider; migrations are applied separately.
/// </summary>
public sealed class HubDbContextFactory : IDesignTimeDbContextFactory<HubDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=kwinhub;Username=postgres;Password=postgres";

    public HubDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("KWINHUB_CONNECTION_STRING")
            ?? DefaultConnectionString;

        var options = new DbContextOptionsBuilder<HubDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new HubDbContext(options);
    }
}
