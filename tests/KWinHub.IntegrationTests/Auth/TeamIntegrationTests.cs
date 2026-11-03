using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Subjects;
using KWinHub.Hub.Teams;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Auth;

public class TeamIntegrationTests : IClassFixture<HubApiFactory>
{
    private readonly HubApiFactory _factory;

    public TeamIntegrationTests(HubApiFactory factory)
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

    private async Task<(Guid, Guid)> SeedTupleAsync(Func<HubDbContext, (Guid, Guid)> seed)
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
    public async Task CreateTeam_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/teams", new
        {
            name = "First Team",
            description = "A test team",
            sportId = "basketball",
            season = "2025"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var team = await response.Content.ReadFromJsonAsync<TeamResponseDto>();
        Assert.NotNull(team);
        Assert.Equal("First Team", team.Name);
        Assert.Equal("basketball", team.SportId);
        Assert.Equal("2025", team.Season);
        Assert.Equal("Active", team.Status);
        Assert.Equal(0, team.MemberCount);
    }

    [Fact]
    public async Task GetTeam_NonExistent_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync($"/api/v1/teams/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTeam_CrossTenant_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();

        var otherTeamId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgA, OrganizationRoles.Admin);
            var other = new Team(orgB, "Other Org Team");
            db.Teams.Add(other);
            return other.Id;
        });

        var client = CreateAuthorizedClient(userId, orgA);

        var response = await client.GetAsync($"/api/v1/teams/{otherTeamId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListTeams_FiltersByStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var active = new Team(orgId, "Active Team");
            var archived = new Team(orgId, "Archived Team");
            archived.Archive();
            db.Teams.AddRange(active, archived);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var allResponse = await client.GetAsync("/api/v1/teams");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        var all = await allResponse.Content.ReadFromJsonAsync<List<TeamResponseDto>>();
        Assert.Equal(2, all!.Count);

        var activeResponse = await client.GetAsync("/api/v1/teams?status=Active");
        var activeOnly = await activeResponse.Content.ReadFromJsonAsync<List<TeamResponseDto>>();
        Assert.Single(activeOnly);
        Assert.Equal("Active Team", activeOnly![0].Name);
    }

    [Fact]
    public async Task ListTeams_SearchByName()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.Teams.AddRange(
                new Team(orgId, "Alpha Squad"),
                new Team(orgId, "Beta Squad"));
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync("/api/v1/teams?search=Alpha");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<TeamResponseDto>>();
        Assert.Single(results);
        Assert.Equal("Alpha Squad", results![0].Name);
    }

    [Fact]
    public async Task UpdateTeam_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var teamId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var team = new Team(orgId, "Old Name", "Old Desc", "sport-1", "2024");
            db.Teams.Add(team);
            return team.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PutAsJsonAsync($"/api/v1/teams/{teamId}", new
        {
            name = "New Name",
            description = "New Desc",
            sportId = "sport-2",
            season = "2025"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<TeamResponseDto>();
        Assert.Equal("New Name", updated!.Name);
        Assert.Equal("New Desc", updated.Description);
        Assert.Equal("sport-2", updated.SportId);
        Assert.Equal("2025", updated.Season);
    }

    [Fact]
    public async Task ArchiveTeam_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var teamId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var team = new Team(orgId, "Test Team");
            db.Teams.Add(team);
            return team.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/teams/{teamId}/archive", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/teams/{teamId}");
        var team = await getResponse.Content.ReadFromJsonAsync<TeamResponseDto>();
        Assert.Equal("Archived", team!.Status);
    }

    [Fact]
    public async Task RestoreTeam_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var teamId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var team = new Team(orgId, "Test Team");
            team.Archive();
            db.Teams.Add(team);
            return team.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/teams/{teamId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/teams/{teamId}");
        var team = await getResponse.Content.ReadFromJsonAsync<TeamResponseDto>();
        Assert.Equal("Active", team!.Status);
    }

    [Fact]
    public async Task AddMember_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var (teamId, subjectId) = await SeedTupleAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var team = new Team(orgId, "Test Team");
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            db.Teams.Add(team);
            db.Subjects.Add(subject);
            return (team.Id, subject.Id);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync($"/api/v1/teams/{teamId}/members", new
        {
            subjectId = subjectId,
            role = "Captain"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<TeamMemberResponseDto>();
        Assert.Equal(subjectId, member!.SubjectId);
        Assert.Equal("Captain", member.Role);

        var teamResponse = await client.GetAsync($"/api/v1/teams/{teamId}");
        var team = await teamResponse.Content.ReadFromJsonAsync<TeamResponseDto>();
        Assert.Equal(1, team!.MemberCount);
    }

    [Fact]
    public async Task AddMember_CrossTenantSubject_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();

        var (teamId, otherSubjectId) = await SeedTupleAsync(db =>
        {
            SeedActiveMembership(db, userId, orgA, OrganizationRoles.Admin);
            var team = new Team(orgA, "Test Team");
            var otherSubject = new Subject(orgB, "Other", "Subject", SubjectType.Athlete);
            db.Teams.Add(team);
            db.Subjects.Add(otherSubject);
            return (team.Id, otherSubject.Id);
        });

        var client = CreateAuthorizedClient(userId, orgA);

        var response = await client.PostAsJsonAsync($"/api/v1/teams/{teamId}/members", new
        {
            subjectId = otherSubjectId,
            role = "Player"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var (teamId, subjectId) = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var team = new Team(orgId, "Test Team");
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            team.AddMember(subject.Id);
            db.Teams.Add(team);
            db.Subjects.Add(subject);
            return (team.Id, subject.Id);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.DeleteAsync($"/api/v1/teams/{teamId}/members/{subjectId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var teamResponse = await client.GetAsync($"/api/v1/teams/{teamId}");
        var team = await teamResponse.Content.ReadFromJsonAsync<TeamResponseDto>();
        Assert.Equal(0, team!.MemberCount);
    }

    [Fact]
    public async Task ListMembers_ReturnsMembers()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var (teamId, subjectId) = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var team = new Team(orgId, "Test Team");
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            team.AddMember(subject.Id, "Captain");
            db.Teams.Add(team);
            db.Subjects.Add(subject);
            return (team.Id, subject.Id);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync($"/api/v1/teams/{teamId}/members");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var members = await response.Content.ReadFromJsonAsync<List<TeamMemberResponseDto>>();
        Assert.Single(members);
        Assert.Equal(subjectId, members![0].SubjectId);
        Assert.Equal("Captain", members[0].Role);
    }

    [Fact]
    public async Task CreateTeam_Viewer_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Viewer);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId, OrganizationRoles.Viewer);

        var response = await client.PostAsJsonAsync("/api/v1/teams", new
        {
            name = "Should Fail"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private record TeamResponseDto(
        Guid Id,
        Guid OrganizationId,
        string Name,
        string? Description,
        string? SportId,
        string? Season,
        string Status,
        int MemberCount,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    private record TeamMemberResponseDto(
        Guid MembershipId,
        Guid TeamId,
        Guid SubjectId,
        string? Role);
}