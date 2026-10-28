using KWinHub.Application.Authentication;
using KWinHub.Application.Authorization;
using KWinHub.Application.Catalogs;
using KWinHub.Application.Platform;
using KWinHub.Application.Protocols;
using KWinHub.Application.Registration;
using KWinHub.Application.Results;
using KWinHub.Application.Sessions;
using KWinHub.Application.Subjects;
using KWinHub.Application.Subscriptions;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IProtocolService, ProtocolService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IResultService, ResultService>();

        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IPlatformService, PlatformService>();
        services.AddScoped<ICatalogService, CatalogService>();

        services.AddScoped<IOrganizationAccessService, OrganizationAccessService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IPlatformSubscriptionService, PlatformSubscriptionService>();

        return services;
    }
}
