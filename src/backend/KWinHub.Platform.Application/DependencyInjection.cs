using FluentValidation;
using KWinHub.Platform.Application.Audit;
using KWinHub.Platform.Application.Authentication;
using KWinHub.Platform.Application.Catalogs;
using KWinHub.Platform.Application.Platform;
using KWinHub.Platform.Application.Registration;
using KWinHub.Platform.Application.Subscriptions;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.Platform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlatformApplication(this IServiceCollection services)
    {
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPlatformService, PlatformService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPlatformSubscriptionService, PlatformSubscriptionService>();
        services.AddScoped<IAuditService, AuditService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}