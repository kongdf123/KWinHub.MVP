using Amazon;
using Amazon.S3;
using KWinHub.Hub.Application.Authorization;
using KWinHub.Hub.Application.Common;
using KWinHub.Hub.Application.CrossContext;
using KWinHub.Hub.Application.Groups;
using KWinHub.Hub.Application.Media;
using KWinHub.Hub.Application.Messaging;
using KWinHub.Hub.Application.Organizations;
using KWinHub.Hub.Application.Plans;
using KWinHub.Hub.Application.Protocols;
using KWinHub.Hub.Application.Reports;
using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Application.Subjects;
using KWinHub.Hub.Application.Teams;
using KWinHub.Hub.Application.Workers;
using KWinHub.Hub.Assets;
using KWinHub.Hub.Infrastructure.CrossContext;
using KWinHub.Hub.Infrastructure.Media;
using KWinHub.Hub.Infrastructure.Messaging;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Infrastructure.Persistence.Repositories;
using KWinHub.Hub.Infrastructure.Storage;
using KWinHub.Hub.Infrastructure.Workers;
using KWinHub.Hub.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
        services.AddScoped<IIngestionOperationRepository, IngestionOperationRepository>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<IAssessmentPlanRepository, AssessmentPlanRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IWorkerJobRepository, WorkerJobRepository>();
        services.AddScoped<ITransactionManager, EfTransactionManager>();
        services.AddScoped<IWorkspaceStatsReader, WorkspaceStatsReader>();

        services.AddScoped<IMediaProcessor, LocalFfmpegMediaProcessor>();

        services.AddScoped<IOutbox, OutboxRepository>();
        services.AddSingleton<IMessageBus, LocalMessageBus>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddHostedService<OutboxRelayService>();
        services.AddHostedService<MediaWorkerService>();

        var storageSection = configuration.GetSection("ObjectStorage");
        var storageOptions = new ObjectStorageOptions();
        storageSection.Bind(storageOptions);
        services.AddSingleton(storageOptions);

        var cloudProvider = configuration["Cloud:Provider"]?.ToLowerInvariant() ?? "aliyun";

        switch (cloudProvider)
        {
            case "aliyun":
                RegisterAliyunStorage(services, configuration);
                break;
            case "aws":
                RegisterAwsStorage(services, configuration);
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown cloud provider '{cloudProvider}'. Supported values: 'aliyun', 'aws'.");
        }

        AddPlatformInternalClient(services, configuration);

        return services;
    }

    private static void RegisterAliyunStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<Aliyun.OSS.IOss>(_ =>
        {
            var endpoint = configuration["ObjectStorage:ServiceUrl"]
                ?? throw new InvalidOperationException("ObjectStorage:ServiceUrl is required for Aliyun OSS.");
            var accessKey = configuration["ObjectStorage:AccessKey"] ?? string.Empty;
            var secretKey = configuration["ObjectStorage:SecretKey"] ?? string.Empty;
            return new Aliyun.OSS.OssClient(endpoint, accessKey, secretKey);
        });
        services.AddScoped<IObjectStorage, AliyunOssObjectStorage>();
    }

    private static void RegisterAwsStorage(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var s3Config = new AmazonS3Config
            {
                ServiceURL = configuration["ObjectStorage:ServiceUrl"],
                ForcePathStyle = configuration.GetValue<bool>("ObjectStorage:ForcePathStyle", true),
                RegionEndpoint = RegionEndpoint.GetBySystemName(configuration["ObjectStorage:Region"] ?? "us-east-1")
            };

            return new AmazonS3Client(
                configuration["ObjectStorage:AccessKey"] ?? "minioadmin",
                configuration["ObjectStorage:SecretKey"] ?? "minioadmin",
                s3Config);
        });
        services.AddScoped<IObjectStorage, S3ObjectStorage>();
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