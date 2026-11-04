using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Subjects;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Auth;

public class SourceIdentityMappingIntegrationTests : IClassFixture<HubApiFactory>
{
    private readonly HubApiFactory _factory;

    public SourceIdentityMappingIntegrationTests(HubApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<Guid> SeedAsync(Func<HubDbContext, Guid> seed)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var result = seed(db);
        await db.SaveChangesAsync();
        return result;
    }

    private HttpClient CreateAuthorizedClient(Guid userId, Guid orgId, string role = OrganizationRoles.Admin)
    {
        var client = _factory.CreateClient();
        var token = TestTokenFactory.CreateToken(
            new[] { TestTokenFactory.HubAudience },
            userId: userId,
            orgId: orgId,
            role: role);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static void SeedActiveMembership(HubDbContext db, Guid userId, Guid orgId, string role)
    {
        var membership = new OrganizationMembership(userId, orgId, role);
        membership.AcceptInvite();
        db.OrganizationMemberships.Add(membership);
        db.OrganizationStatuses.Add(new OrganizationStatus(orgId, "Active", 1));
    }

    [Fact]
    public async Task RegisterMapping_NewMapping_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectId
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var mapping = await response.Content.ReadFromJsonAsync<MappingResponseDto>();
        Assert.NotNull(mapping);
        Assert.Equal("Resolved", mapping.Status);
        Assert.Equal(subjectId, mapping.SubjectId);
    }

    [Fact]
    public async Task RegisterMapping_SameSourceIdentity_SameSubject_ReturnsExisting()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var payload = new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectId
        };

        var first = await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", payload);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", payload);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);

        var firstMapping = await first.Content.ReadFromJsonAsync<MappingResponseDto>();
        var secondMapping = await second.Content.ReadFromJsonAsync<MappingResponseDto>();
        Assert.Equal(firstMapping!.Id, secondMapping!.Id);
    }

    [Fact]
    public async Task RegisterMapping_SameSourceIdentity_DifferentSubject_MarksConflict()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var subjectA = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
        var subjectB = new Subject(orgId, "John", "Athlete", SubjectType.Athlete);

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.Subjects.AddRange(subjectA, subjectB);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectA.Id
        });

        var second = await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectB.Id
        });

        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        var mapping = await second.Content.ReadFromJsonAsync<MappingResponseDto>();
        Assert.Equal("Conflict", mapping!.Status);
        Assert.NotNull(mapping.ConflictReason);
    }

    [Fact]
    public async Task ResolveConflict_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var subjectA = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
        var subjectB = new Subject(orgId, "John", "Athlete", SubjectType.Athlete);

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.Subjects.AddRange(subjectA, subjectB);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectA.Id
        });

        var conflictResponse = await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectB.Id
        });

        var conflictMapping = await conflictResponse.Content.ReadFromJsonAsync<MappingResponseDto>();
        Assert.Equal("Conflict", conflictMapping!.Status);

        var resolveResponse = await client.PostAsJsonAsync(
            $"/api/v1/sourceidentitymappings/{conflictMapping.Id}/resolve",
            new { subjectId = subjectB.Id });

        Assert.Equal(HttpStatusCode.OK, resolveResponse.StatusCode);
        var resolved = await resolveResponse.Content.ReadFromJsonAsync<MappingResponseDto>();
        Assert.Equal("Resolved", resolved!.Status);
        Assert.Equal(subjectB.Id, resolved.SubjectId);
        Assert.NotNull(resolved.ResolvedBy);
        Assert.NotNull(resolved.ResolvedAt);
    }

    [Fact]
    public async Task ResolveConflict_NonConflict_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var created = await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectId
        });

        var mapping = await created.Content.ReadFromJsonAsync<MappingResponseDto>();

        var resolveResponse = await client.PostAsJsonAsync(
            $"/api/v1/sourceidentitymappings/{mapping!.Id}/resolve",
            new { subjectId = subjectId });

        Assert.Equal(HttpStatusCode.NotFound, resolveResponse.StatusCode);
    }

    [Fact]
    public async Task ListMappings_FiltersByStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var subjectA = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
        var subjectB = new Subject(orgId, "John", "Athlete", SubjectType.Athlete);

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.Subjects.AddRange(subjectA, subjectB);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectA.Id
        });

        await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectB.Id
        });

        var allResponse = await client.GetAsync("/api/v1/sourceidentitymappings");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        var all = await allResponse.Content.ReadFromJsonAsync<List<MappingResponseDto>>();
        Assert.Single(all);

        var conflictResponse = await client.GetAsync("/api/v1/sourceidentitymappings?status=Conflict");
        Assert.Equal(HttpStatusCode.OK, conflictResponse.StatusCode);
        var conflicts = await conflictResponse.Content.ReadFromJsonAsync<List<MappingResponseDto>>();
        Assert.Single(conflicts);
        Assert.Equal("Conflict", conflicts![0].Status);
    }

    [Fact]
    public async Task GetBySourceIdentity_Existing_ReturnsMapping()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectId
        });

        var response = await client.GetAsync(
            "/api/v1/sourceidentitymappings/by-source?sourceApp=KWinCapture&installationScope=default&externalId=EXT-001");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var mapping = await response.Content.ReadFromJsonAsync<MappingResponseDto>();
        Assert.NotNull(mapping);
        Assert.Equal(subjectId, mapping!.SubjectId);
    }

    [Fact]
    public async Task ListBySubject_ReturnsMappings()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectId
        });

        var response = await client.GetAsync($"/api/v1/sourceidentitymappings/by-subject/{subjectId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var mappings = await response.Content.ReadFromJsonAsync<List<MappingResponseDto>>();
        Assert.Single(mappings);
    }

    [Fact]
    public async Task RegisterMapping_Viewer_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Viewer);
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId, OrganizationRoles.Viewer);

        var response = await client.PostAsJsonAsync("/api/v1/sourceidentitymappings", new
        {
            sourceApp = "KWinCapture",
            installationScope = "default",
            externalId = "EXT-001",
            subjectId = subjectId
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private record MappingResponseDto(
        Guid Id,
        Guid OrganizationId,
        string SourceApp,
        string InstallationScope,
        string ExternalId,
        Guid SubjectId,
        string Status,
        string? ConflictReason,
        Guid? ResolvedBy,
        DateTimeOffset? ResolvedAt,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);
}