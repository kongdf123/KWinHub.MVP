using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Subjects;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Auth;

public class SubjectIntegrationTests : IClassFixture<HubApiFactory>
{
    private readonly HubApiFactory _factory;

    public SubjectIntegrationTests(HubApiFactory factory)
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
    public async Task CreateSubject_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/subjects", new
        {
            firstName = "Jane",
            lastName = "Athlete",
            subjectType = "Athlete",
            externalId = "EXT-001"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var subject = await response.Content.ReadFromJsonAsync<SubjectResponseDto>();
        Assert.NotNull(subject);
        Assert.Equal("Jane", subject.FirstName);
        Assert.Equal("Athlete", subject.SubjectType);
        Assert.Equal("Active", subject.Status);
    }

    [Fact]
    public async Task GetSubject_NonExistent_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync($"/api/v1/subjects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSubject_CrossTenant_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();

        var otherSubjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgA, OrganizationRoles.Admin);
            var other = new Subject(orgB, "Other", "Org", SubjectType.Athlete);
            db.Subjects.Add(other);
            return other.Id;
        });

        var client = CreateAuthorizedClient(userId, orgA);

        var response = await client.GetAsync($"/api/v1/subjects/{otherSubjectId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListSubjects_FiltersByStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var active = new Subject(orgId, "Active", "User", SubjectType.Athlete);
            var archived = new Subject(orgId, "Archived", "User", SubjectType.Athlete);
            archived.Archive();
            db.Subjects.AddRange(active, archived);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var allResponse = await client.GetAsync("/api/v1/subjects");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        var all = await allResponse.Content.ReadFromJsonAsync<List<SubjectResponseDto>>();
        Assert.Equal(2, all!.Count);

        var activeResponse = await client.GetAsync("/api/v1/subjects?status=Active");
        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);
        var activeOnly = await activeResponse.Content.ReadFromJsonAsync<List<SubjectResponseDto>>();
        Assert.Single(activeOnly);
        Assert.Equal("Active", activeOnly[0].FirstName);
    }

    [Fact]
    public async Task ListSubjects_SearchByFirstName()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.Subjects.AddRange(
                new Subject(orgId, "Alpha", "One", SubjectType.Athlete),
                new Subject(orgId, "Beta", "Two", SubjectType.Patient));
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync("/api/v1/subjects?search=Alpha");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<SubjectResponseDto>>();
        Assert.Single(results);
        Assert.Equal("Alpha", results![0].FirstName);
    }

    [Fact]
    public async Task UpdateSubject_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var subject = new Subject(orgId, "Old", "Name", SubjectType.Athlete);
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PutAsJsonAsync($"/api/v1/subjects/{subjectId}", new
        {
            firstName = "Updated",
            lastName = "Name",
            subjectType = "Patient",
            externalId = "NEW-EXT"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<SubjectResponseDto>();
        Assert.Equal("Updated", updated!.FirstName);
        Assert.Equal("Patient", updated.SubjectType);
        Assert.Equal("NEW-EXT", updated.ExternalId);
    }

    [Fact]
    public async Task UpdateSubject_Viewer_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Viewer);
            var subject = new Subject(orgId, "Test", "User", SubjectType.Athlete);
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId, OrganizationRoles.Viewer);

        var response = await client.PutAsJsonAsync($"/api/v1/subjects/{subjectId}", new
        {
            firstName = "Hacked",
            lastName = "Name",
            subjectType = "Athlete"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveSubject_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var subject = new Subject(orgId, "Test", "User", SubjectType.Athlete);
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/subjects/{subjectId}/archive", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/subjects/{subjectId}");
        var subject = await getResponse.Content.ReadFromJsonAsync<SubjectResponseDto>();
        Assert.Equal("Archived", subject!.Status);
    }

    [Fact]
    public async Task RestoreSubject_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var subjectId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var subject = new Subject(orgId, "Test", "User", SubjectType.Athlete);
            subject.Archive();
            db.Subjects.Add(subject);
            return subject.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/subjects/{subjectId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/subjects/{subjectId}");
        var subject = await getResponse.Content.ReadFromJsonAsync<SubjectResponseDto>();
        Assert.Equal("Active", subject!.Status);
    }

    [Fact]
    public async Task ArchiveSubject_NonExistent_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/subjects/{Guid.NewGuid()}/archive", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record SubjectResponseDto(
        Guid Id,
        Guid OrganizationId,
        string FirstName,
        string LastName,
        string SubjectType,
        string? ExternalId,
        string? DisplayName,
        DateTimeOffset? DateOfBirth,
        string? Sex,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);
}