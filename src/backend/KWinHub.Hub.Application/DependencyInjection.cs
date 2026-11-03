using FluentValidation;
using KWinHub.Hub.Application.Authorization;
using KWinHub.Hub.Application.Groups;
using KWinHub.Hub.Application.Protocols;
using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Application.Teams;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.Hub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHubApplication(this IServiceCollection services)
    {
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IProtocolService, ProtocolService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IResultService, ResultService>();

        services.AddScoped<IOrganizationAccessService, OrganizationAccessService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IOrgContextService, OrgContextService>();
        services.AddScoped<IOrganizationLifecycleEventConsumer, OrganizationLifecycleEventConsumer>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}