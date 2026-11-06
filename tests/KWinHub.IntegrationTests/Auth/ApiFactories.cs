using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Platform.Application.CrossContext;
using KWinHub.Platform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KWinHub.IntegrationTests.Auth;

public class PlatformApiFactory : WebApplicationFactory<KWinHub.Platform.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.UseSetting("Jwt:SigningKey", TestTokenFactory.TestSigningKey);
        builder.UseSetting("Jwt:Issuer", TestTokenFactory.TestIssuer);
        builder.UseSetting("Jwt:PlatformAudience", TestTokenFactory.PlatformAudience);
        builder.UseSetting("Jwt:HubAudience", TestTokenFactory.HubAudience);
        builder.UseSetting("Jwt:ValidateSecurityStamp", "false");

        builder.ConfigureServices(services =>
        {
            RemoveAllEfCoreDescriptors(services);
            services.AddDbContext<PlatformDbContext>(options =>
                options.UseInMemoryDatabase("platform_auth_test"));
        });
    }

    private static void RemoveAllEfCoreDescriptors(IServiceCollection services)
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<PlatformDbContext>) ||
                d.ServiceType == typeof(IDbContextFactory<PlatformDbContext>) ||
                (d.ServiceType.Namespace != null &&
                 d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)))
            .ToList();

        foreach (var d in toRemove)
        {
            services.Remove(d);
        }
    }
}

public class HubApiFactory : WebApplicationFactory<KWinHub.Hub.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.UseSetting("Jwt:SigningKey", TestTokenFactory.TestSigningKey);
        builder.UseSetting("Jwt:Issuer", TestTokenFactory.TestIssuer);
        builder.UseSetting("Jwt:PlatformAudience", TestTokenFactory.PlatformAudience);
        builder.UseSetting("Jwt:HubAudience", TestTokenFactory.HubAudience);
        builder.UseSetting("Jwt:ValidateSecurityStamp", "false");

        builder.ConfigureServices(services =>
        {
            RemoveAllEfCoreDescriptors(services);
            services.AddDbContext<HubDbContext>(options =>
                options.UseInMemoryDatabase("hub_auth_test")
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }

    private static void RemoveAllEfCoreDescriptors(IServiceCollection services)
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<HubDbContext>) ||
                d.ServiceType == typeof(IDbContextFactory<HubDbContext>) ||
                (d.ServiceType.Namespace != null &&
                 d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)))
            .ToList();

        foreach (var d in toRemove)
        {
            services.Remove(d);
        }
    }
}

public class HubApiSqliteFactory : WebApplicationFactory<KWinHub.Hub.Api.Program>
{
    private readonly SqliteConnection _connection;

    public HubApiSqliteFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.UseSetting("Jwt:SigningKey", TestTokenFactory.TestSigningKey);
        builder.UseSetting("Jwt:Issuer", TestTokenFactory.TestIssuer);
        builder.UseSetting("Jwt:PlatformAudience", TestTokenFactory.PlatformAudience);
        builder.UseSetting("Jwt:HubAudience", TestTokenFactory.HubAudience);
        builder.UseSetting("Jwt:ValidateSecurityStamp", "false");

        builder.ConfigureServices(services =>
        {
            RemoveAllEfCoreDescriptors(services);
            services.AddDbContext<HubDbContext>(options =>
                options.UseSqlite(_connection)
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
            services.AddHostedService<HubDatabaseInitializer>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private static void RemoveAllEfCoreDescriptors(IServiceCollection services)
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<HubDbContext>) ||
                d.ServiceType == typeof(IDbContextFactory<HubDbContext>) ||
                (d.ServiceType.Namespace != null &&
                 d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)))
            .ToList();

        foreach (var d in toRemove)
        {
            services.Remove(d);
        }
    }
}

public class HubDatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public HubDatabaseInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public class PlatformApiDevFactory : WebApplicationFactory<KWinHub.Platform.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Jwt:SigningKey", TestTokenFactory.TestSigningKey);
        builder.UseSetting("Jwt:Issuer", TestTokenFactory.TestIssuer);
        builder.UseSetting("Jwt:PlatformAudience", TestTokenFactory.PlatformAudience);
        builder.UseSetting("Jwt:HubAudience", TestTokenFactory.HubAudience);
        builder.UseSetting("Jwt:ValidateSecurityStamp", "false");

        builder.ConfigureServices(services =>
        {
            RemoveAllEfCoreDescriptors<PlatformDbContext>(services);
            services.AddDbContext<PlatformDbContext>(options =>
                options.UseInMemoryDatabase("platform_dev_test"));
        });
    }

    private static void RemoveAllEfCoreDescriptors<T>(IServiceCollection services) where T : DbContext
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<T>) ||
                d.ServiceType == typeof(IDbContextFactory<T>) ||
                (d.ServiceType.Namespace != null &&
                 d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)))
            .ToList();

        foreach (var d in toRemove)
        {
            services.Remove(d);
        }
    }
}

public class HubApiDevFactory : WebApplicationFactory<KWinHub.Hub.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("Jwt:SigningKey", TestTokenFactory.TestSigningKey);
        builder.UseSetting("Jwt:Issuer", TestTokenFactory.TestIssuer);
        builder.UseSetting("Jwt:PlatformAudience", TestTokenFactory.PlatformAudience);
        builder.UseSetting("Jwt:HubAudience", TestTokenFactory.HubAudience);
        builder.UseSetting("Jwt:ValidateSecurityStamp", "false");

        builder.ConfigureServices(services =>
        {
            RemoveAllEfCoreDescriptors<HubDbContext>(services);
            services.AddDbContext<HubDbContext>(options =>
                options.UseInMemoryDatabase("hub_dev_test")
                    .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }

    private static void RemoveAllEfCoreDescriptors<T>(IServiceCollection services) where T : DbContext
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<T>) ||
                d.ServiceType == typeof(IDbContextFactory<T>) ||
                (d.ServiceType.Namespace != null &&
                 d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)))
            .ToList();

        foreach (var d in toRemove)
        {
            services.Remove(d);
        }
    }
}

/// <summary>
/// Records lifecycle events relayed to the Hub so tests can assert cross-context
/// convergence without a running Hub API.
/// </summary>
public class StubHubGateway : IHubGateway
{
    private readonly List<(Guid EventId, Guid OrganizationId, string EventType, long Version)> _published = [];

    public IReadOnlyList<(Guid EventId, Guid OrganizationId, string EventType, long Version)> PublishedLifecycleEvents => _published;

    public Task ProvisionAdminMembershipAsync(Guid organizationId, Guid userId, string role, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<HubPrimaryMembership?> GetPrimaryMembershipAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult<HubPrimaryMembership?>(null);

    public Task<IReadOnlyList<HubMembership>> ListMembershipsAsync(Guid organizationId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<HubMembership>>([]);

    public Task<IReadOnlyList<HubOrganizationStats>> GetOrganizationStatsAsync(IReadOnlyCollection<Guid> organizationIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<HubOrganizationStats>>(
            organizationIds.Select(id => new HubOrganizationStats(id, 0, 0, 0, 0)).ToList());

    public Task<IReadOnlyDictionary<Guid, int>> GetUserMembershipCountsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<Guid, int>>(
            userIds.ToDictionary(id => id, _ => 0));

    public Task PublishOrganizationLifecycleEventAsync(
        Guid eventId,
        Guid organizationId,
        string eventType,
        long eventVersion,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        _published.Add((eventId, organizationId, eventType, eventVersion));
        return Task.CompletedTask;
    }
}

public class PlatformApiSqliteFactory : WebApplicationFactory<KWinHub.Platform.Api.Program>
{
    private readonly SqliteConnection _connection;
    public StubHubGateway HubGateway { get; } = new();

    public PlatformApiSqliteFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.UseSetting("Jwt:SigningKey", TestTokenFactory.TestSigningKey);
        builder.UseSetting("Jwt:Issuer", TestTokenFactory.TestIssuer);
        builder.UseSetting("Jwt:PlatformAudience", TestTokenFactory.PlatformAudience);
        builder.UseSetting("Jwt:HubAudience", TestTokenFactory.HubAudience);
        builder.UseSetting("Jwt:ValidateSecurityStamp", "false");

        builder.ConfigureServices(services =>
        {
            RemoveAllEfCoreDescriptors<PlatformDbContext>(services);
            services.AddDbContext<PlatformDbContext>(options =>
                options.UseSqlite(_connection)
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

            var hubDescriptors = services
                .Where(d => d.ServiceType == typeof(IHubGateway))
                .ToList();
            foreach (var d in hubDescriptors)
            {
                services.Remove(d);
            }
            services.AddSingleton<IHubGateway>(HubGateway);
            services.AddHostedService<PlatformDatabaseInitializer>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    private static void RemoveAllEfCoreDescriptors<T>(IServiceCollection services) where T : DbContext
    {
        var toRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(DbContextOptions<T>) ||
                d.ServiceType == typeof(IDbContextFactory<T>) ||
                (d.ServiceType.Namespace != null &&
                 d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)))
            .ToList();

        foreach (var d in toRemove)
        {
            services.Remove(d);
        }
    }
}

public class PlatformDatabaseInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public PlatformDatabaseInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        await db.Database.EnsureDeletedAsync(cancellationToken);
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}