using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Contracts.History;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Results;
using KWinHub.Hub.Sessions;
using KWinHub.Hub.Subjects;
using KWinHub.IntegrationTests.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.History;

public class HistoryIntegrationTests : IClassFixture<HubApiSqliteFactory>
{
    private readonly HubApiSqliteFactory _factory;

    public HistoryIntegrationTests(HubApiSqliteFactory factory)
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
    public async Task ListSubjectHistory_ReturnsSessionsWithMetrics()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;
        Guid sessionId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            subjectId = sId;
            protocolId = pId;
            sessionId = SeedSession(db, orgId, subjectId, protocolId, new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(
                db, orgId, sessionId, protocolId,
                ("jump_height", "Jump Height", 45.2, "cm"),
                ("takeoff_velocity", "Takeoff Velocity", 3.1, "m/s"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.GetAsync($"/api/v1/history/subjects/{subjectId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var history = await response.Content.ReadFromJsonAsync<HistoryListResponse>();
        Assert.NotNull(history);
        Assert.Equal(subjectId, history.SubjectId);
        Assert.Equal(1, history.TotalCount);
        Assert.Single(history.Sessions);

        var session = history.Sessions[0];
        Assert.Equal(sessionId, session.SessionId);
        Assert.Equal("JUMP", session.ProtocolCode);
        Assert.Equal("Published", session.Status);
        Assert.Equal(2, session.Metrics.Count);
        Assert.Contains(session.Metrics, m => m.MetricCode == "jump_height" && m.ScalarValue == 45.2);
        Assert.Contains(session.Metrics, m => m.MetricCode == "takeoff_velocity" && m.ScalarValue == 3.1);
    }

    [Fact]
    public async Task ListSubjectHistory_ProtocolFilter_ReturnsOnlyMatchingSessions()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolAId = Guid.Empty;
        Guid protocolBId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var subject = new Subject(orgId, "Test", "Athlete", SubjectType.Athlete);
            var protocolA = new Protocol(orgId, "Jump Test", "JUMP", "1.0");
            var protocolB = new Protocol(orgId, "Sprint Test", "SPRINT", "1.0");
            db.Subjects.Add(subject);
            db.Protocols.Add(protocolA);
            db.Protocols.Add(protocolB);
            subjectId = subject.Id;
            protocolAId = protocolA.Id;
            protocolBId = protocolB.Id;

            var sessionA = SeedSession(db, orgId, subjectId, protocolAId, new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, sessionA, protocolAId, ("jump_height", "Jump Height", 45.2, "cm"));

            var sessionB = SeedSession(db, orgId, subjectId, protocolBId, new DateTimeOffset(2025, 1, 16, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, sessionB, protocolBId, ("sprint_time", "Sprint Time", 6.5, "s"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.GetAsync($"/api/v1/history/subjects/{subjectId}?protocolId={protocolAId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var history = await response.Content.ReadFromJsonAsync<HistoryListResponse>();
        Assert.NotNull(history);
        Assert.Equal(1, history.TotalCount);
        Assert.Single(history.Sessions);
        Assert.Equal("JUMP", history.Sessions[0].ProtocolCode);
    }

    [Fact]
    public async Task CompareSessions_CompatibleMetrics_ReturnsMeanAndMax()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;
        var sessionIds = new List<Guid>();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            subjectId = sId;
            protocolId = pId;

            var s1 = SeedSession(db, orgId, subjectId, protocolId, new DateTimeOffset(2025, 1, 10, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, s1, protocolId, ("jump_height", "Jump Height", 40.0, "cm"));
            sessionIds.Add(s1);

            var s2 = SeedSession(db, orgId, subjectId, protocolId, new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, s2, protocolId, ("jump_height", "Jump Height", 50.0, "cm"));
            sessionIds.Add(s2);

            var s3 = SeedSession(db, orgId, subjectId, protocolId, new DateTimeOffset(2025, 1, 20, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, s3, protocolId, ("jump_height", "Jump Height", 60.0, "cm"));
            sessionIds.Add(s3);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var sessionIdsParam = string.Join("&", sessionIds.Select(id => $"sessionIds={id}"));
        var response = await client.GetAsync($"/api/v1/history/subjects/{subjectId}/comparison?{sessionIdsParam}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comparison = await response.Content.ReadFromJsonAsync<ComparisonResponse>();
        Assert.NotNull(comparison);
        Assert.Equal(3, comparison.SessionIds.Count);
        Assert.Single(comparison.CompatibleMetrics);

        var group = comparison.CompatibleMetrics[0];
        Assert.Equal("jump_height", group.MetricCode);
        Assert.Equal("cm", group.Unit);
        Assert.Equal(3, group.Points.Count);
        Assert.Equal(3, group.Statistics.SampleCount);
        Assert.Equal(50.0, group.Statistics.Mean);
        Assert.Equal(60.0, group.Statistics.Max);
        Assert.Empty(group.Exclusions);
    }

    [Fact]
    public async Task CompareSessions_IncompatibleUnit_ExcludesWithReason()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;
        var sessionIds = new List<Guid>();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            subjectId = sId;
            protocolId = pId;

            var s1 = SeedSession(db, orgId, subjectId, protocolId, new DateTimeOffset(2025, 1, 10, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, s1, protocolId, ("jump_height", "Jump Height", 40.0, "cm"));
            sessionIds.Add(s1);

            var s2 = SeedSession(db, orgId, subjectId, protocolId, new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, s2, protocolId, ("jump_height", "Jump Height", 1.5, "m"));
            sessionIds.Add(s2);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var sessionIdsParam = string.Join("&", sessionIds.Select(id => $"sessionIds={id}"));
        var response = await client.GetAsync($"/api/v1/history/subjects/{subjectId}/comparison?{sessionIdsParam}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comparison = await response.Content.ReadFromJsonAsync<ComparisonResponse>();
        Assert.NotNull(comparison);

        var group = comparison.CompatibleMetrics[0];
        Assert.Equal("jump_height", group.MetricCode);
        Assert.Single(group.Points);
        Assert.Equal(1, group.Statistics.SampleCount);
        Assert.Contains(new[] { 40.0, 1.5 }, v => v == group.Statistics.Mean);
        Assert.Equal(group.Statistics.Mean, group.Statistics.Max);
        Assert.Single(group.Exclusions);
        Assert.Contains("Unit mismatch", group.Exclusions[0].Reason);
    }

    [Fact]
    public async Task ListSubjectHistory_CrossTenant_ReturnsOnlyOwnOrgData()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var otherOrgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);

            var subject = new Subject(orgId, "Test", "Athlete", SubjectType.Athlete);
            var protocol = new Protocol(orgId, "Jump Test", "JUMP", "1.0");
            db.Subjects.Add(subject);
            db.Protocols.Add(protocol);
            subjectId = subject.Id;
            protocolId = protocol.Id;

            var sessionId = SeedSession(db, orgId, subjectId, protocolId, new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, sessionId, protocolId, ("jump_height", "Jump Height", 45.2, "cm"));

            var otherSubject = new Subject(otherOrgId, "Other", "Athlete", SubjectType.Athlete);
            var otherProtocol = new Protocol(otherOrgId, "Jump Test", "JUMP", "1.0");
            db.Subjects.Add(otherSubject);
            db.Protocols.Add(otherProtocol);
            var otherSession = SeedSession(db, otherOrgId, otherSubject.Id, otherProtocol.Id, new DateTimeOffset(2025, 1, 16, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, otherOrgId, otherSession, otherProtocol.Id, ("jump_height", "Jump Height", 99.9, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.GetAsync($"/api/v1/history/subjects/{subjectId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var history = await response.Content.ReadFromJsonAsync<HistoryListResponse>();
        Assert.NotNull(history);
        Assert.Equal(1, history.TotalCount);
        Assert.DoesNotContain(history.Sessions, s => s.Metrics.Any(m => m.ScalarValue == 99.9));
    }

    [Fact]
    public async Task CompareSessions_MissingMetricInOneSession_ExcludedWithReason()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;
        var sessionIds = new List<Guid>();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            subjectId = sId;
            protocolId = pId;

            var s1 = SeedSession(db, orgId, subjectId, protocolId, new DateTimeOffset(2025, 1, 10, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, s1, protocolId,
                ("jump_height", "Jump Height", 40.0, "cm"),
                ("takeoff_velocity", "Takeoff Velocity", 3.0, "m/s"));
            sessionIds.Add(s1);

            var s2 = SeedSession(db, orgId, subjectId, protocolId, new DateTimeOffset(2025, 1, 15, 10, 0, 0, TimeSpan.Zero));
            SeedResultWithMetrics(db, orgId, s2, protocolId,
                ("jump_height", "Jump Height", 50.0, "cm"));
            sessionIds.Add(s2);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var sessionIdsParam = string.Join("&", sessionIds.Select(id => $"sessionIds={id}"));
        var response = await client.GetAsync($"/api/v1/history/subjects/{subjectId}/comparison?{sessionIdsParam}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comparison = await response.Content.ReadFromJsonAsync<ComparisonResponse>();
        Assert.NotNull(comparison);

        var velocityGroup = comparison.CompatibleMetrics.FirstOrDefault(g => g.MetricCode == "takeoff_velocity");
        Assert.NotNull(velocityGroup);
        Assert.Single(velocityGroup.Points);
        Assert.Equal(1, velocityGroup.Statistics.SampleCount);
        Assert.Single(velocityGroup.Exclusions);
        Assert.Equal("Metric not present in result", velocityGroup.Exclusions[0].Reason);

        var heightGroup = comparison.CompatibleMetrics.First(g => g.MetricCode == "jump_height");
        Assert.Equal(2, heightGroup.Statistics.SampleCount);
        Assert.Empty(heightGroup.Exclusions);
    }
}