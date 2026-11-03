using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Groups;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Subjects;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Auth;

public class GroupIntegrationTests : IClassFixture<HubApiFactory>
{
    private readonly HubApiFactory _factory;

    public GroupIntegrationTests(HubApiFactory factory)
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
    public async Task CreateGroup_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/groups", new
        {
            name = "First Group",
            description = "A test group",
            groupType = "cohort"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var group = await response.Content.ReadFromJsonAsync<GroupResponseDto>();
        Assert.NotNull(group);
        Assert.Equal("First Group", group.Name);
        Assert.Equal("cohort", group.GroupType);
        Assert.Equal("Active", group.Status);
        Assert.Equal(0, group.MemberCount);
    }

    [Fact]
    public async Task GetGroup_NonExistent_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync($"/api/v1/groups/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetGroup_CrossTenant_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();

        var otherGroupId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgA, OrganizationRoles.Admin);
            var other = new Group(orgB, "Other Org Group");
            db.Groups.Add(other);
            return other.Id;
        });

        var client = CreateAuthorizedClient(userId, orgA);

        var response = await client.GetAsync($"/api/v1/groups/{otherGroupId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ListGroups_FiltersByStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var active = new Group(orgId, "Active Group");
            var archived = new Group(orgId, "Archived Group");
            archived.Archive();
            db.Groups.AddRange(active, archived);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var allResponse = await client.GetAsync("/api/v1/groups");
        Assert.Equal(HttpStatusCode.OK, allResponse.StatusCode);
        var all = await allResponse.Content.ReadFromJsonAsync<List<GroupResponseDto>>();
        Assert.Equal(2, all!.Count);

        var activeResponse = await client.GetAsync("/api/v1/groups?status=Active");
        var activeOnly = await activeResponse.Content.ReadFromJsonAsync<List<GroupResponseDto>>();
        Assert.Single(activeOnly);
        Assert.Equal("Active Group", activeOnly![0].Name);
    }

    [Fact]
    public async Task ListGroups_FiltersByType()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.Groups.AddRange(
                new Group(orgId, "Cohort A", groupType: "cohort"),
                new Group(orgId, "Team B", groupType: "team"));
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync("/api/v1/groups?groupType=cohort");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var results = await response.Content.ReadFromJsonAsync<List<GroupResponseDto>>();
        Assert.Single(results);
        Assert.Equal("Cohort A", results![0].Name);
    }

    [Fact]
    public async Task UpdateGroup_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var groupId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var group = new Group(orgId, "Old Name", "Old Desc", "type-1");
            db.Groups.Add(group);
            return group.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PutAsJsonAsync($"/api/v1/groups/{groupId}", new
        {
            name = "New Name",
            description = "New Desc",
            groupType = "type-2"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<GroupResponseDto>();
        Assert.Equal("New Name", updated!.Name);
        Assert.Equal("type-2", updated.GroupType);
    }

    [Fact]
    public async Task ArchiveGroup_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var groupId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var group = new Group(orgId, "Test Group");
            db.Groups.Add(group);
            return group.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/groups/{groupId}/archive", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/groups/{groupId}");
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponseDto>();
        Assert.Equal("Archived", group!.Status);
    }

    [Fact]
    public async Task RestoreGroup_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var groupId = await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var group = new Group(orgId, "Test Group");
            group.Archive();
            db.Groups.Add(group);
            return group.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/groups/{groupId}/restore", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/groups/{groupId}");
        var group = await getResponse.Content.ReadFromJsonAsync<GroupResponseDto>();
        Assert.Equal("Active", group!.Status);
    }

    [Fact]
    public async Task AddMember_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var (groupId, subjectId) = await SeedTupleAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var group = new Group(orgId, "Test Group");
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            db.Groups.Add(group);
            db.Subjects.Add(subject);
            return (group.Id, subject.Id);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync($"/api/v1/groups/{groupId}/members", new
        {
            subjectId = subjectId,
            role = "Leader"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var member = await response.Content.ReadFromJsonAsync<GroupMemberResponseDto>();
        Assert.Equal(subjectId, member!.SubjectId);
        Assert.Equal("Leader", member.Role);

        var groupResponse = await client.GetAsync($"/api/v1/groups/{groupId}");
        var group = await groupResponse.Content.ReadFromJsonAsync<GroupResponseDto>();
        Assert.Equal(1, group!.MemberCount);
    }

    [Fact]
    public async Task AddMember_CrossTenantSubject_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();

        var (groupId, otherSubjectId) = await SeedTupleAsync(db =>
        {
            SeedActiveMembership(db, userId, orgA, OrganizationRoles.Admin);
            var group = new Group(orgA, "Test Group");
            var otherSubject = new Subject(orgB, "Other", "Subject", SubjectType.Athlete);
            db.Groups.Add(group);
            db.Subjects.Add(otherSubject);
            return (group.Id, otherSubject.Id);
        });

        var client = CreateAuthorizedClient(userId, orgA);

        var response = await client.PostAsJsonAsync($"/api/v1/groups/{groupId}/members", new
        {
            subjectId = otherSubjectId,
            role = "Member"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveMember_Admin_Succeeds()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        var (groupId, subjectId) = await SeedTupleAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var group = new Group(orgId, "Test Group");
            var subject = new Subject(orgId, "Jane", "Athlete", SubjectType.Athlete);
            group.AddMember(subject.Id);
            db.Groups.Add(group);
            db.Subjects.Add(subject);
            return (group.Id, subject.Id);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.DeleteAsync($"/api/v1/groups/{groupId}/members/{subjectId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var groupResponse = await client.GetAsync($"/api/v1/groups/{groupId}");
        var group = await groupResponse.Content.ReadFromJsonAsync<GroupResponseDto>();
        Assert.Equal(0, group!.MemberCount);
    }

    [Fact]
    public async Task CreateGroup_Viewer_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Viewer);
            return Guid.Empty;
        });

        var client = CreateAuthorizedClient(userId, orgId, OrganizationRoles.Viewer);

        var response = await client.PostAsJsonAsync("/api/v1/groups", new
        {
            name = "Should Fail"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private record GroupResponseDto(
        Guid Id,
        Guid OrganizationId,
        string Name,
        string? Description,
        string? GroupType,
        string Status,
        int MemberCount,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    private record GroupMemberResponseDto(
        Guid MembershipId,
        Guid GroupId,
        Guid SubjectId,
        string? Role);
}