using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Contracts.Plans;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Results;
using KWinHub.Hub.Sessions;
using KWinHub.Hub.Subjects;
using KWinHub.IntegrationTests.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Plans;

public class AssessmentPlanIntegrationTests : IClassFixture<HubApiSqliteFactory>
{
    private readonly HubApiSqliteFactory _factory;

    public AssessmentPlanIntegrationTests(HubApiSqliteFactory factory)
    {
        _factory = factory;
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

    private async Task SeedAsync(Action<HubDbContext> seed)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        seed(db);
        await db.SaveChangesAsync();
    }

    private static void SeedActiveMembership(HubDbContext db, Guid userId, Guid orgId, string role)
    {
        var membership = new OrganizationMembership(userId, orgId, role);
        membership.AcceptInvite();
        db.OrganizationMemberships.Add(membership);
        db.OrganizationStatuses.Add(new OrganizationStatus(orgId, "Active", 1));
    }

    private static (Guid SubjectId, Guid ProtocolId) SeedSubjectAndProtocol(HubDbContext db, Guid orgId)
    {
        var subject = new Subject(orgId, "Test", "Athlete", SubjectType.Athlete);
        var protocol = new Protocol(orgId, "Jump Test", "JUMP", "1.0");
        db.Subjects.Add(subject);
        db.Protocols.Add(protocol);
        return (subject.Id, protocol.Id);
    }

    private static Guid SeedSession(HubDbContext db, Guid orgId, Guid subjectId, Guid protocolId, DateTimeOffset startedAt)
    {
        var session = new MeasurementSession(
            organizationId: orgId,
            subjectId: subjectId,
            protocolId: protocolId,
            protocolVersion: "1.0",
            externalId: $"sess-{Guid.NewGuid():N}");
        session.SetStartedAt(startedAt);
        session.TransitionTo(SessionStatus.Published);
        db.MeasurementSessions.Add(session);
        return session.Id;
    }

    private static void SeedResultWithMetrics(
        HubDbContext db,
        Guid orgId,
        Guid sessionId,
        Guid protocolId,
        params (string Code, string Name, double Value, string Unit)[] metrics)
    {
        var result = new Result(
            organizationId: orgId,
            sessionId: sessionId,
            protocolId: protocolId,
            protocolVersion: "1.0",
            algorithmVersion: "ingested");
        result.MarkPublished("Good");

        foreach (var (code, name, value, unit) in metrics)
        {
            result.AddMetric(code, name, value, unit);
        }

        db.Results.Add(result);
    }

    [Fact]
    public async Task CreatePlan_WithItems_PersistsAndReturnsDraft()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        await SeedAsync(db => { SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin); });

        var client = CreateAuthorizedClient(userId, orgId);
        var request = new CreateAssessmentPlanRequest(
            Name: "Jump Assessment",
            Code: "JUMP-ASM",
            Description: "Countermovement jump battery",
            ProtocolId: null,
            ProtocolCode: null,
            Items: new[]
            {
                new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, 0, 100),
                new CreateAssessmentPlanItemRequest("rsi", "Reactive Strength Index", "m/s", "Scalar", false, null, null)
            });

        var response = await client.PostAsJsonAsync("/api/v1/assessmentplans", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var plan = await response.Content.ReadFromJsonAsync<AssessmentPlanResponse>();
        Assert.NotNull(plan);
        Assert.Equal("Draft", plan.Status);
        Assert.Equal(1, plan.Version);
        Assert.Equal(2, plan.Items.Count);
        Assert.Equal("jump_height", plan.Items[0].MetricCode);
        Assert.True(plan.Items[0].Required);
    }

    [Fact]
    public async Task PublishPlan_SetsStatusToPublished()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        await SeedAsync(db => { SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin); });

        var client = CreateAuthorizedClient(userId, orgId);
        var createRequest = new CreateAssessmentPlanRequest(
            "Jump Assessment", "JUMP-ASM", null, null, null,
            new[] { new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, null, null) });

        var created = await (await client.PostAsJsonAsync("/api/v1/assessmentplans", createRequest))
            .Content.ReadFromJsonAsync<AssessmentPlanResponse>();

        var publishResponse = await client.PostAsync($"/api/v1/assessmentplans/{created!.Id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);

        var published = await publishResponse.Content.ReadFromJsonAsync<AssessmentPlanResponse>();
        Assert.Equal("Published", published!.Status);
    }

    [Fact]
    public async Task UpdatePublishedPlan_ReturnsBadRequest_CannotModifyPublished()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        await SeedAsync(db => { SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin); });

        var client = CreateAuthorizedClient(userId, orgId);
        var createRequest = new CreateAssessmentPlanRequest(
            "Jump Assessment", "JUMP-ASM", null, null, null,
            new[] { new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, null, null) });

        var created = await (await client.PostAsJsonAsync("/api/v1/assessmentplans", createRequest))
            .Content.ReadFromJsonAsync<AssessmentPlanResponse>();

        await client.PostAsync($"/api/v1/assessmentplans/{created!.Id}/publish", null);

        var updateRequest = new UpdateAssessmentPlanRequest(
            "Modified Name", null, null, null,
            new[] { new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, null, null) });

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/assessmentplans/{created.Id}", updateRequest);
        Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
    }

    [Fact]
    public async Task CreateNewVersion_FromPublished_CreatesDraftWithIncrementedVersion()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        await SeedAsync(db => { SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin); });

        var client = CreateAuthorizedClient(userId, orgId);
        var createRequest = new CreateAssessmentPlanRequest(
            "Jump Assessment", "JUMP-ASM", null, null, null,
            new[] { new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, null, null) });

        var created = await (await client.PostAsJsonAsync("/api/v1/assessmentplans", createRequest))
            .Content.ReadFromJsonAsync<AssessmentPlanResponse>();

        await client.PostAsync($"/api/v1/assessmentplans/{created!.Id}/publish", null);

        var newVersionResponse = await client.PostAsync($"/api/v1/assessmentplans/{created.Id}/new-version", null);
        Assert.Equal(HttpStatusCode.Created, newVersionResponse.StatusCode);

        var newVersion = await newVersionResponse.Content.ReadFromJsonAsync<AssessmentPlanResponse>();
        Assert.Equal("Draft", newVersion!.Status);
        Assert.Equal(2, newVersion.Version);
        Assert.Equal("JUMP-ASM", newVersion.Code);
        Assert.Single(newVersion.Items);
        Assert.Equal("jump_height", newVersion.Items[0].MetricCode);

        var list = await client.GetFromJsonAsync<IReadOnlyList<AssessmentPlanResponse>>("/api/v1/assessmentplans");
        Assert.Equal(2, list!.Count);
        Assert.Contains(list, p => p.Version == 1 && p.Status == "Published");
        Assert.Contains(list, p => p.Version == 2 && p.Status == "Draft");
    }

    [Fact]
    public async Task CheckCompleteness_WithAllRequiredMetricsPresent_ReturnsComplete()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty, protocolId = Guid.Empty, sessionId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, subjectId, protocolId, DateTimeOffset.UtcNow.AddDays(-1));
            SeedResultWithMetrics(db, orgId, sessionId, protocolId,
                ("jump_height", "Jump Height", 40.5, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var createRequest = new CreateAssessmentPlanRequest(
            "Jump Assessment", "JUMP-ASM", null, protocolId, "JUMP",
            new[] { new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, null, null) });

        var created = await (await client.PostAsJsonAsync("/api/v1/assessmentplans", createRequest))
            .Content.ReadFromJsonAsync<AssessmentPlanResponse>();

        var checkRequest = new CheckPlanCompletenessRequest(subjectId, new[] { sessionId });
        var checkResponse = await client.PostAsJsonAsync($"/api/v1/assessmentplans/{created!.Id}/completeness", checkRequest);
        Assert.Equal(HttpStatusCode.OK, checkResponse.StatusCode);

        var completeness = await checkResponse.Content.ReadFromJsonAsync<PlanCompletenessResponse>();
        Assert.True(completeness!.IsComplete);
        Assert.Empty(completeness.BlockingReasons);
        Assert.Single(completeness.SelectedSessionIds);
        Assert.Single(completeness.CandidateSessions);
        Assert.True(completeness.CandidateSessions[0].AllRequiredMetricsPresent);
    }

    [Fact]
    public async Task CheckCompleteness_MissingRequiredMetric_BlocksReport_NotZeroFilled()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty, protocolId = Guid.Empty, sessionId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, subjectId, protocolId, DateTimeOffset.UtcNow.AddDays(-1));
            SeedResultWithMetrics(db, orgId, sessionId, protocolId,
                ("rsi", "RSI", 2.1, "m/s"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var createRequest = new CreateAssessmentPlanRequest(
            "Jump Assessment", "JUMP-ASM", null, protocolId, "JUMP",
            new[] { new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, null, null) });

        var created = await (await client.PostAsJsonAsync("/api/v1/assessmentplans", createRequest))
            .Content.ReadFromJsonAsync<AssessmentPlanResponse>();

        var checkRequest = new CheckPlanCompletenessRequest(subjectId, new[] { sessionId });
        var checkResponse = await client.PostAsJsonAsync($"/api/v1/assessmentplans/{created!.Id}/completeness", checkRequest);
        Assert.Equal(HttpStatusCode.OK, checkResponse.StatusCode);

        var completeness = await checkResponse.Content.ReadFromJsonAsync<PlanCompletenessResponse>();
        Assert.False(completeness!.IsComplete);
        Assert.NotEmpty(completeness.BlockingReasons);
        Assert.Contains(completeness.BlockingReasons, r => r.Contains("jump_height") && r.Contains("missing"));
        var candidate = Assert.Single(completeness.CandidateSessions);
        Assert.Contains("jump_height", candidate.MissingRequiredMetrics);
    }

    [Fact]
    public async Task CheckCompleteness_UnitMismatch_BlocksReport()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty, protocolId = Guid.Empty, sessionId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, subjectId, protocolId, DateTimeOffset.UtcNow.AddDays(-1));
            SeedResultWithMetrics(db, orgId, sessionId, protocolId,
                ("jump_height", "Jump Height", 1.5, "m"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var createRequest = new CreateAssessmentPlanRequest(
            "Jump Assessment", "JUMP-ASM", null, protocolId, "JUMP",
            new[] { new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, null, null) });

        var created = await (await client.PostAsJsonAsync("/api/v1/assessmentplans", createRequest))
            .Content.ReadFromJsonAsync<AssessmentPlanResponse>();

        var checkRequest = new CheckPlanCompletenessRequest(subjectId, new[] { sessionId });
        var checkResponse = await client.PostAsJsonAsync($"/api/v1/assessmentplans/{created!.Id}/completeness", checkRequest);

        var completeness = await checkResponse.Content.ReadFromJsonAsync<PlanCompletenessResponse>();
        Assert.False(completeness!.IsComplete);
        Assert.Contains(completeness.BlockingReasons, r => r.Contains("unit mismatch"));
    }

    [Fact]
    public async Task ListPlans_CrossTenantIsolation_DoesNotReturnOtherOrgPlans()
    {
        var userA = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var orgB = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userA, orgA, OrganizationRoles.Admin);
            SeedActiveMembership(db, userB, orgB, OrganizationRoles.Admin);
        });

        var clientA = CreateAuthorizedClient(userA, orgA);
        var clientB = CreateAuthorizedClient(userB, orgB);

        await clientA.PostAsJsonAsync("/api/v1/assessmentplans", new CreateAssessmentPlanRequest(
            "Plan A", "PLAN-A", null, null, null,
            new[] { new CreateAssessmentPlanItemRequest("m1", "M1", "u", "Scalar", true, null, null) }));

        var listB = await clientB.GetFromJsonAsync<IReadOnlyList<AssessmentPlanResponse>>("/api/v1/assessmentplans");
        Assert.Empty(listB!);
    }

    [Fact]
    public async Task RetirePublishedPlan_SetsStatusToRetired()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        await SeedAsync(db => { SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin); });

        var client = CreateAuthorizedClient(userId, orgId);
        var createRequest = new CreateAssessmentPlanRequest(
            "Jump Assessment", "JUMP-ASM", null, null, null,
            new[] { new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, null, null) });

        var created = await (await client.PostAsJsonAsync("/api/v1/assessmentplans", createRequest))
            .Content.ReadFromJsonAsync<AssessmentPlanResponse>();

        await client.PostAsync($"/api/v1/assessmentplans/{created!.Id}/publish", null);

        var retireResponse = await client.PostAsync($"/api/v1/assessmentplans/{created.Id}/retire", null);
        Assert.Equal(HttpStatusCode.OK, retireResponse.StatusCode);

        var retired = await retireResponse.Content.ReadFromJsonAsync<AssessmentPlanResponse>();
        Assert.Equal("Retired", retired!.Status);
    }
}