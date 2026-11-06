using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Platform.Organizations;
using KWinHub.Platform.Infrastructure.Persistence;
using KWinHub.IntegrationTests.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Platform;

public class PlatformAdministrationTests : IClassFixture<PlatformApiSqliteFactory>
{
    private readonly PlatformApiSqliteFactory _factory;

    public PlatformAdministrationTests(PlatformApiSqliteFactory factory)
    {
        _factory = factory;
    }

    private async Task WithDbAsync(Func<PlatformDbContext, Task> action)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        await db.Database.EnsureCreatedAsync();
        await action(db);
    }

    private async Task<T> WithDbAsync<T>(Func<PlatformDbContext, Task<T>> action)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        await db.Database.EnsureCreatedAsync();
        return await action(db);
    }

    private async Task SeedAsync(Action<PlatformDbContext> seed)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        await db.Database.EnsureCreatedAsync();
        seed(db);
        await db.SaveChangesAsync();
    }

    private HttpClient CreateClient(bool asAdmin = true)
    {
        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.PlatformAudience },
            isPlatformAdmin: asAdmin,
            email: asAdmin ? "admin@kwinhub.test" : "user@kwinhub.test");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task NonPlatformAdmin_Denied_ConsoleApis()
    {
        var client = CreateClient(asAdmin: false);

        var orgsResponse = await client.GetAsync("/api/v1/platform/organizations");
        Assert.Equal(HttpStatusCode.Forbidden, orgsResponse.StatusCode);

        var usersResponse = await client.GetAsync("/api/v1/platform/users");
        Assert.Equal(HttpStatusCode.Forbidden, usersResponse.StatusCode);

        var catalogsResponse = await client.GetAsync("/api/v1/platform/catalogs/protocols");
        Assert.Equal(HttpStatusCode.Forbidden, catalogsResponse.StatusCode);

        var auditResponse = await client.GetAsync("/api/v1/platform/audit-logs");
        Assert.Equal(HttpStatusCode.Forbidden, auditResponse.StatusCode);
    }

    [Fact]
    public async Task PlatformAdmin_CanAccess_ConsoleApis()
    {
        var client = CreateClient(asAdmin: true);

        var orgsResponse = await client.GetAsync("/api/v1/platform/organizations");
        Assert.Equal(HttpStatusCode.OK, orgsResponse.StatusCode);

        var usersResponse = await client.GetAsync("/api/v1/platform/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        var catalogsResponse = await client.GetAsync("/api/v1/platform/catalogs/protocols");
        Assert.Equal(HttpStatusCode.OK, catalogsResponse.StatusCode);
    }

    [Fact]
    public async Task SuspendOrganization_WritesAudit_AndEmitsLifecycleEvent()
    {
        var orgId = Guid.NewGuid();
        await SeedAsync(db =>
        {
            db.Organizations.Add(new Organization(
                name: "Test Org",
                slug: "test-org",
                id: orgId));
        });

        var client = CreateClient(asAdmin: true);
        var suspendResponse = await client.PostAsJsonAsync(
            $"/api/v1/platform/organizations/{orgId}/status",
            new { action = "suspend" });

        Assert.Equal(HttpStatusCode.NoContent, suspendResponse.StatusCode);

        await WithDbAsync(async db =>
        {
            var org = await db.Organizations.FindAsync(orgId);
            Assert.NotNull(org);
            Assert.Equal(OrganizationStatus.Suspended, org!.Status);

            var auditLog = db.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault(a => a.EntityType == "Organization" && a.EntityId == orgId.ToString());
            Assert.NotNull(auditLog);
            Assert.Equal("organization.suspended", auditLog!.Action);
            Assert.Contains("suspended", auditLog.Summary, StringComparison.OrdinalIgnoreCase);

            var lifecycleEvent = db.OrganizationLifecycleEvents
                .FirstOrDefault(e => e.OrganizationId == orgId);
            Assert.NotNull(lifecycleEvent);
            Assert.Equal(OrganizationLifecycleEventTypes.Suspended, lifecycleEvent!.EventType);
            Assert.Equal(1, lifecycleEvent.EventVersion);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task SuspendOrganization_RelaysEvent_ToHub()
    {
        var orgId = Guid.NewGuid();
        await SeedAsync(db =>
        {
            db.Organizations.Add(new Organization(
                name: "Relay Org",
                slug: "relay-org",
                id: orgId));
        });

        var client = CreateClient(asAdmin: true);
        await client.PostAsJsonAsync(
            $"/api/v1/platform/organizations/{orgId}/status",
            new { action = "suspend" });

        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline)
        {
            if (_factory.HubGateway.PublishedLifecycleEvents.Any(e => e.OrganizationId == orgId))
            {
                break;
            }
            await Task.Delay(200);
        }

        var relayed = _factory.HubGateway.PublishedLifecycleEvents
            .FirstOrDefault(e => e.OrganizationId == orgId);
        Assert.True(relayed != default, "Expected a suspended lifecycle event to be relayed to the Hub.");
        Assert.Equal(OrganizationLifecycleEventTypes.Suspended, relayed.EventType);
    }

    [Fact]
    public async Task ReactivateOrganization_WritesAudit_AndEmitsReactivatedEvent()
    {
        var orgId = Guid.NewGuid();
        await SeedAsync(db =>
        {
            var org = new Organization("Reactivate Org", "reactivate-org", id: orgId);
            org.Suspend();
            db.Organizations.Add(org);
        });

        var client = CreateClient(asAdmin: true);
        var response = await client.PostAsJsonAsync(
            $"/api/v1/platform/organizations/{orgId}/status",
            new { action = "reactivate" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await WithDbAsync(async db =>
        {
            var org = await db.Organizations.FindAsync(orgId);
            Assert.NotNull(org);
            Assert.Equal(OrganizationStatus.Active, org!.Status);

            var auditLog = db.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault(a => a.EntityId == orgId.ToString() && a.Action == "organization.reactivated");
            Assert.NotNull(auditLog);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CreateProtocol_WritesAuditLog()
    {
        var client = CreateClient(asAdmin: true);
        var request = new
        {
            code = "PROTO-M6-1",
            name = "M6 Test Protocol",
            version = "1.0",
            category = (string?)null,
            sport = (string?)null,
            description = (string?)null
        };

        var response = await client.PostAsJsonAsync("/api/v1/platform/catalogs/protocols", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        await WithDbAsync(db =>
        {
            var auditLog = db.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault(a => a.EntityType == "GlobalProtocol");
            Assert.NotNull(auditLog);
            Assert.Equal("catalog.protocol.created", auditLog!.Action);
            Assert.Contains("PROTO-M6-1", auditLog.Summary);
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task UpdateProtocol_ChangesVersion_AndWritesAuditLog()
    {
        var client = CreateClient(asAdmin: true);
        var createRequest = new
        {
            code = "PROTO-M6-2",
            name = "Versioned Protocol",
            version = "1.0",
            category = (string?)null,
            sport = (string?)null,
            description = (string?)null
        };

        var createResponse = await client.PostAsJsonAsync("/api/v1/platform/catalogs/protocols", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProtocolDto>();
        Assert.NotNull(created);

        var updateRequest = new
        {
            code = "PROTO-M6-2",
            name = "Versioned Protocol v2",
            version = "2.0",
            category = (string?)null,
            sport = (string?)null,
            description = "Updated description"
        };

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/v1/platform/catalogs/protocols/{created!.Id}",
            updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ProtocolDto>();
        Assert.NotNull(updated);
        Assert.Equal("2.0", updated!.Version);

        await WithDbAsync(db =>
        {
            var updateAudit = db.AuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault(a => a.EntityType == "GlobalProtocol" && a.Action == "catalog.protocol.updated");
            Assert.NotNull(updateAudit);
            Assert.Contains("2.0", updateAudit!.Summary);
            return Task.CompletedTask;
        });
    }

    private sealed record ProtocolDto(Guid Id, string Code, string Name, string Version, string Status);
}