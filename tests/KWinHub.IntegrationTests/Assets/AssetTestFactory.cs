using KWinHub.Hub.Assets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Assets;

public class AssetTestFactory : Auth.HubApiSqliteFactory
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
}