using KWinHub.Application.Abstractions;
using KWinHub.Application.Authorization;
using KWinHub.Application.Catalogs;
using KWinHub.Application.Platform;
using KWinHub.Application.Protocols;
using KWinHub.Application.Registration;
using KWinHub.Application.Results;
using KWinHub.Application.Sessions;
using KWinHub.Application.Subjects;
using KWinHub.Application.Subscriptions;
using KWinHub.Infrastructure.Identity;
using KWinHub.Infrastructure.Persistence;
using KWinHub.Infrastructure.Persistence.Repositories;
using KWinHub.Infrastructure.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("KWinHubDatabase")
            ?? throw new InvalidOperationException("Connection string 'KWinHubDatabase' not found.");

        services.AddDbContext<KWinHubDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<IProtocolRepository, ProtocolRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IResultRepository, ResultRepository>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
        services.AddScoped<IPlatformRepository, PlatformRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IEntitlementResolver, EntitlementResolver>();
        services.AddScoped<IUsageMeter, UsageMeter>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IPasswordHasher, AspNetCorePasswordHasher>();

        return services;
    }
}
