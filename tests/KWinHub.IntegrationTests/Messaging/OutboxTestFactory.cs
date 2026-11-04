using KWinHub.Hub.Application.Messaging;
using KWinHub.Hub.Assets;
using KWinHub.Hub.Infrastructure.Messaging;
using KWinHub.IntegrationTests.Assets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KWinHub.IntegrationTests.Messaging;

public class OutboxTestFactory : Auth.HubApiSqliteFactory
{
    public InMemoryObjectStorage ObjectStorage { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseSetting("Cloud:Provider", "aws");
        builder.UseSetting("ObjectStorage:ServiceUrl", "http://localhost:9000");
        builder.UseSetting("ObjectStorage:AccessKey", "minioadmin");
        builder.UseSetting("ObjectStorage:SecretKey", "minioadmin");
        builder.UseSetting("ObjectStorage:Bucket", "kwinhub-assets");
        builder.UseSetting("ObjectStorage:Region", "us-east-1");
        builder.UseSetting("ObjectStorage:ForcePathStyle", "true");

        builder.ConfigureServices(services =>
        {
            var storageDescriptors = services.Where(d => d.ServiceType == typeof(IObjectStorage)).ToList();
            foreach (var descriptor in storageDescriptors)
            {
                services.Remove(descriptor);
            }
            services.AddSingleton<IObjectStorage>(ObjectStorage);
        });
    }

    public LocalMessageBus GetMessageBus()
    {
        return (LocalMessageBus)Services.GetRequiredService<IMessageBus>();
    }

    public OutboxRelayService GetRelay()
    {
        return (OutboxRelayService)Services.GetServices<IHostedService>()
            .First(s => s is OutboxRelayService);
    }
}