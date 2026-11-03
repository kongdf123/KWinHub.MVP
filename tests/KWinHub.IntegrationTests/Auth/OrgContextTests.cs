using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Auth;

public class OrgContextTests : IClassFixture<HubApiFactory>
{
    private readonly HubApiFactory _factory;

    public OrgContextTests(HubApiFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedAsync(Action<HubDbContext> seed)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        seed(db);
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task ListOrganizations_ReturnsUsersMemberships()
    {
        var userId = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var m1 = new OrganizationMembership(userId, orgA, OrganizationRoles.Admin);
            m1.AcceptInvite();
            db.OrganizationMemberships.Add(m1);

            var m2 = new OrganizationMembership(userId, orgB, OrganizationRoles.Member);
            m2.AcceptInvite();
            db.OrganizationMemberships.Add(m2);
        });

        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.HubAudience },
            userId: userId,
            orgId: orgA);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/v1/organizations");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(orgA.ToString(), content);
        Assert.Contains(orgB.ToString(), content);
    }

    [Fact]
    public async Task GetContext_ActiveMember_ReturnsContext()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var m = new OrganizationMembership(userId, orgId, OrganizationRoles.Admin);
            m.AcceptInvite();
            db.OrganizationMemberships.Add(m);
            db.OrganizationStatuses.Add(new OrganizationStatus(orgId, "Active", 1));
        });

        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.HubAudience },
            userId: userId,
            orgId: orgId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/organizations/{orgId}/context");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetContext_NonMember_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.HubAudience },
            userId: userId,
            orgId: orgId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/organizations/{orgId}/context");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetContext_SuspendedMembership_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var m = new OrganizationMembership(userId, orgId, OrganizationRoles.Member);
            m.AcceptInvite();
            m.Suspend();
            db.OrganizationMemberships.Add(m);
        });

        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.HubAudience },
            userId: userId,
            orgId: orgId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/organizations/{orgId}/context");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetContext_SuspendedOrganization_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var m = new OrganizationMembership(userId, orgId, OrganizationRoles.Admin);
            m.AcceptInvite();
            db.OrganizationMemberships.Add(m);
            db.OrganizationStatuses.Add(new OrganizationStatus(orgId, "Suspended", 2));
        });

        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.HubAudience },
            userId: userId,
            orgId: orgId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync($"/api/v1/organizations/{orgId}/context");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ViewerRole_CanReadButCannotWrite()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var m = new OrganizationMembership(userId, orgId, OrganizationRoles.Viewer);
            m.AcceptInvite();
            db.OrganizationMemberships.Add(m);
            db.OrganizationStatuses.Add(new OrganizationStatus(orgId, "Active", 1));
        });

        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.HubAudience },
            userId: userId,
            orgId: orgId,
            role: OrganizationRoles.Viewer);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var readResponse = await client.GetAsync("/api/v1/subjects");
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/subjects",
            new { firstName = "Test", lastName = "User", subjectType = "Athlete" });
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }
}