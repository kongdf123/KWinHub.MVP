using KWinHub.Hub.Application.Authorization;
using KWinHub.Hub.Application.CrossContext;
using KWinHub.Hub.Application.Groups;
using KWinHub.Hub.Application.Organizations;
using KWinHub.Hub.Application.Protocols;
using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Application.Teams;
using KWinHub.Hub.Infrastructure.CrossContext;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.Hub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHubInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("KWinHubDatabase")
            ?? throw new InvalidOperationException("Connection string 'KWinHubDatabase' not found.");

        services.AddDbContext<HubDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISourceIdentityMappingRepository, SourceIdentityMappingRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IProtocolRepository, ProtocolRepository>();
        services.AddScoped<IMetricDefinitionRepository, MetricDefinitionRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ITrialRepository, TrialRepository>();
        services.AddScoped<IDataSourceRepository, DataSourceRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IResultRepository, ResultRepository>();
        services.AddScoped<IMembershipRepository, MembershipRepository>();
        services.AddScoped<IOrganizationInvitationRepository, OrganizationInvitationRepository>();
        services.AddScoped<IOrganizationStatusRepository, OrganizationStatusRepository>();
        services.AddScoped<IOrganizationEventConsumptionRepository, OrganizationEventConsumptionRepository>();
        services.AddScoped<IWorkspaceStatsReader, WorkspaceStatsReader>();

        AddPlatformInternalClient(services, configuration);

        return services;
    }

    private static void AddPlatformInternalClient(IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["InternalApis:Platform:BaseUrl"];
        var apiKey = configuration["InternalApis:Platform:ApiKey"] ?? string.Empty;

        services.AddHttpClient<IPlatformGateway, HttpPlatformGateway>(client =>
        {
            if (!string.IsNullOrWhiteSpace(baseUrl))
            {
                client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
            }

            client.DefaultRequestHeaders.Add(HttpPlatformGateway.ApiKeyHeaderName, apiKey);
            client.Timeout = TimeSpan.FromSeconds(15);
        });
    }
}