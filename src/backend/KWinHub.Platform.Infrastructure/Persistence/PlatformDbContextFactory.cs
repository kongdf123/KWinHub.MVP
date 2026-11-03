using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KWinHub.Platform.Infrastructure.Persistence;

/// <summary>
/// Used by the EF Core tooling (`dotnet ef migrations`) at design time so the
/// Platform model can be built without booting the API host. The connection
/// string is only used to pick the provider; migrations are applied separately.
/// </summary>
public sealed class PlatformDbContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    private const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=kwinhub;Username=postgres;Password=postgres";

    public PlatformDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("KWINHUB_CONNECTION_STRING")
            ?? DefaultConnectionString;

        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new PlatformDbContext(options);
    }
}
