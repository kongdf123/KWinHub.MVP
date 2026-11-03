using KWinHub.Platform.Application.Abstractions;
using KWinHub.Platform.Application.Audit;
using KWinHub.Platform.Application.Catalogs;
using KWinHub.Platform.Application.CrossContext;
using KWinHub.Platform.Application.Organizations;
using KWinHub.Platform.Application.Platform;
using KWinHub.Platform.Application.Registration;
using KWinHub.Platform.Application.Subscriptions;
using KWinHub.Platform.Infrastructure.CrossContext;
using KWinHub.Platform.Infrastructure.Identity;
using KWinHub.Platform.Infrastructure.Persistence;
using KWinHub.Platform.Infrastructure.Persistence.Repositories;
using KWinHub.Platform.Infrastructure.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.Platform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("KWinHubDatabase")
            ?? throw new InvalidOperationException("Connection string 'KWinHubDatabase' not found.");

        services.AddDbContext<PlatformDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
        services.AddScoped<IPlatformRepository, PlatformRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IEntitlementResolver, EntitlementResolver>();
        services.AddScoped<IUsageMeter, UsageMeter>();
        services.AddScoped<IOrganizationEventOutbox, OrganizationEventOutbox>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IPasswordHasher, AspNetCorePasswordHasher>();

        AddHubInternalClient(services, configuration);

        return services;
    }

    private static void AddHubInternalClient(IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["InternalApis:Hub:BaseUrl"];
        var apiKey = configuration["InternalApis:Hub:ApiKey"] ?? string.Empty;

        services.AddHttpClient<IHubGateway, HttpHubGateway>(client =>
        {
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            }

            client.DefaultRequestHeaders.Add(HttpHubGateway.ApiKeyHeaderName, apiKey);
            client.Timeout = TimeSpan.FromSeconds(15);
        });
    }
}