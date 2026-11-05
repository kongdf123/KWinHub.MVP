using FluentValidation;
using KWinHub.Hub.Application.Authorization;
using KWinHub.Hub.Application.History;
using KWinHub.Hub.Application.Ingestion;
using KWinHub.Hub.Application.Groups;
using KWinHub.Hub.Application.Organizations;
using KWinHub.Hub.Application.Protocols;
using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Application.Teams;
using KWinHub.Hub.Assets;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.Hub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHubApplication(this IServiceCollection services)
    {
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISourceIdentityMappingService, SourceIdentityMappingService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IProtocolService, ProtocolService>();
        services.AddScoped<IMetricDefinitionService, MetricDefinitionService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<ITrialService, TrialService>();
        services.AddScoped<IDataSourceService, DataSourceService>();
        services.AddScoped<IDeviceService, DeviceService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<IHistoryService, HistoryService>();
        services.AddScoped<IIngestionService, IngestionService>();
        services.AddScoped<IAssetService, AssetService>();

        services.AddScoped<IOrganizationAccessService, OrganizationAccessService>();
        services.AddScoped<IMembershipService, MembershipService>();
        services.AddScoped<IOrgContextService, OrgContextService>();
        services.AddScoped<IOrganizationLifecycleEventConsumer, OrganizationLifecycleEventConsumer>();
        services.AddScoped<IOrganizationMemberService, OrganizationMemberService>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}