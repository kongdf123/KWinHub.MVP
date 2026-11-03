using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Platform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
                options.UseInMemoryDatabase("hub_auth_test"));
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
                options.UseInMemoryDatabase("hub_dev_test"));
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