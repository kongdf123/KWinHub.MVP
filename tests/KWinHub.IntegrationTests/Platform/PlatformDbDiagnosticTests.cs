using KWinHub.Platform.Infrastructure.Persistence;
using KWinHub.IntegrationTests.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Platform;

public class PlatformDbDiagnosticTests : IClassFixture<PlatformApiSqliteFactory>
{
    private readonly PlatformApiSqliteFactory _factory;

    public PlatformDbDiagnosticTests(PlatformApiSqliteFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Diagnostic_CheckTables()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        await db.Database.EnsureCreatedAsync();
        var tables = await db.Database.SqlQueryRaw<string>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name").ToListAsync();
        Assert.Contains("organizations", tables);
        Assert.Contains("audit_logs", tables);
        Assert.Contains("organization_lifecycle_events", tables);
    }
}