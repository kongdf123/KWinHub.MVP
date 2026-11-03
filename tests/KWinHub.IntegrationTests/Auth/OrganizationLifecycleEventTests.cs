using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Auth;

public class OrganizationLifecycleEventTests : IClassFixture<OrganizationLifecycleEventTests.HubFactory>
{
    public class HubFactory : WebApplicationFactory<KWinHub.Hub.Api.Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("Jwt:SigningKey", TestTokenFactory.TestSigningKey);
            builder.UseSetting("Jwt:Issuer", TestTokenFactory.TestIssuer);
            builder.UseSetting("Jwt:PlatformAudience", TestTokenFactory.PlatformAudience);
            builder.UseSetting("Jwt:HubAudience", TestTokenFactory.HubAudience);
            builder.UseSetting("Jwt:ValidateSecurityStamp", "false");
            builder.UseSetting("InternalApis:ApiKey", "test-internal-api-key");

            builder.ConfigureServices(services =>
            {
                var toRemove = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType == typeof(DbContextOptions<HubDbContext>) ||
                        d.ServiceType == typeof(IDbContextFactory<HubDbContext>) ||
                        (d.ServiceType.Namespace != null &&
                         d.ServiceType.Namespace.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)))
                    .ToList();

                foreach (var d in toRemove)
                {
                    services.Remove(d);
                }

                services.AddDbContext<HubDbContext>(options =>
                    options.UseInMemoryDatabase("hub_lifecycle_test"));
            });
        }
    }

    private const string InternalApiKey = "test-internal-api-key";
    private readonly HubFactory _factory;

    public OrganizationLifecycleEventTests(HubFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateInternalClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Internal-Api-Key", InternalApiKey);
        return client;
    }

    private async Task<OrganizationStatus?> GetOrgStatusAsync(Guid orgId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        return await db.OrganizationStatuses.AsNoTracking().FirstOrDefaultAsync(s => s.OrganizationId == orgId);
    }

    private async Task<int> GetConsumptionCountAsync(Guid eventId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        return await db.OrganizationEventConsumptions.CountAsync(c => c.EventId == eventId);
    }

    [Fact]
    public async Task ReceiveEvent_CreatesOrganizationStatus()
    {
        var orgId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var payload = JsonSerializer.Serialize(new
        {
            OrganizationId = orgId,
            EventType = "organization.created",
            Version = 1,
            Status = "Active",
            OccurredAt = DateTimeOffset.UtcNow
        });

        var client = CreateInternalClient();
        var response = await client.PostAsJsonAsync(
            "internal/v1/organization-lifecycle-events",
            new { eventId, organizationId = orgId, eventType = "organization.created", eventVersion = 1L, payloadJson = payload });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var status = await GetOrgStatusAsync(orgId);
        Assert.NotNull(status);
        Assert.Equal("Active", status!.Status);
        Assert.Equal(1, status.Version);
    }

    [Fact]
    public async Task ReceiveEvent_Idempotent_SameEventTwiceDoesNotDuplicate()
    {
        var orgId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var payload = JsonSerializer.Serialize(new
        {
            OrganizationId = orgId,
            EventType = "organization.suspended",
            Version = 2,
            Status = "Suspended",
            OccurredAt = DateTimeOffset.UtcNow
        });

        var client = CreateInternalClient();

        var first = await client.PostAsJsonAsync(
            "internal/v1/organization-lifecycle-events",
            new { eventId, organizationId = orgId, eventType = "organization.suspended", eventVersion = 2L, payloadJson = payload });
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await client.PostAsJsonAsync(
            "internal/v1/organization-lifecycle-events",
            new { eventId, organizationId = orgId, eventType = "organization.suspended", eventVersion = 2L, payloadJson = payload });
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var consumptionCount = await GetConsumptionCountAsync(eventId);
        Assert.Equal(1, consumptionCount);
    }

    [Fact]
    public async Task ReceiveEvent_OlderVersion_DoesNotOverwriteNewerStatus()
    {
        var orgId = Guid.NewGuid();

        var suspendEventId = Guid.NewGuid();
        var suspendPayload = JsonSerializer.Serialize(new
        {
            OrganizationId = orgId,
            EventType = "organization.suspended",
            Version = 2,
            Status = "Suspended",
            OccurredAt = DateTimeOffset.UtcNow
        });

        var client = CreateInternalClient();
        var suspendResponse = await client.PostAsJsonAsync(
            "internal/v1/organization-lifecycle-events",
            new { eventId = suspendEventId, organizationId = orgId, eventType = "organization.suspended", eventVersion = 2L, payloadJson = suspendPayload });
        Assert.Equal(HttpStatusCode.OK, suspendResponse.StatusCode);

        var reactivateEventId = Guid.NewGuid();
        var reactivatePayload = JsonSerializer.Serialize(new
        {
            OrganizationId = orgId,
            EventType = "organization.reactivated",
            Version = 3,
            Status = "Active",
            OccurredAt = DateTimeOffset.UtcNow
        });

        var reactivateResponse = await client.PostAsJsonAsync(
            "internal/v1/organization-lifecycle-events",
            new { eventId = reactivateEventId, organizationId = orgId, eventType = "organization.reactivated", eventVersion = 3L, payloadJson = reactivatePayload });
        Assert.Equal(HttpStatusCode.OK, reactivateResponse.StatusCode);

        var status = await GetOrgStatusAsync(orgId);
        Assert.NotNull(status);
        Assert.Equal("Active", status!.Status);
        Assert.Equal(3, status.Version);
    }

    [Fact]
    public async Task ReceiveEvent_UnauthorizedWithoutApiKey()
    {
        var orgId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var payload = "{}";

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "internal/v1/organization-lifecycle-events",
            new { eventId, organizationId = orgId, eventType = "organization.created", eventVersion = 1L, payloadJson = payload });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}