using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KWinHub.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KWinHubDbContext>
{
    public KWinHubDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("KWinHubDatabase")
            ?? "Host=localhost;Port=5432;Database=kwinhub;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<KWinHubDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new KWinHubDbContext(optionsBuilder.Options);
    }
}