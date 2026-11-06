using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Assets;
using KWinHub.Hub.Contracts.Plans;
using KWinHub.Hub.Contracts.Reports;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Results;
using KWinHub.Hub.Sessions;
using KWinHub.Hub.Subjects;
using KWinHub.IntegrationTests.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Reports;

public class ReportIntegrationTests : IClassFixture<HubApiSqliteFactory>
{
    private readonly HubApiSqliteFactory _factory;

    public ReportIntegrationTests(HubApiSqliteFactory factory)
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

    private static Guid SeedResultWithMetrics(
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
        return result.Id;
    }

    private static Guid SeedAsset(
        HubDbContext db,
        Guid orgId,
        Guid sessionId,
        Guid? trialId,
        string assetType,
        bool ready,
        string? failureReason = null)
    {
        var asset = new DataAsset(orgId, sessionId, trialId, assetType, 1024, $"ext-{Guid.NewGuid():N}", "video/mp4", "checksum");
        asset.MarkUploaded("key", "bucket");
        asset.MarkVerified();
        if (ready)
        {
            asset.MarkProcessing();
            asset.MarkReady("provider-" + Guid.NewGuid().ToString("N"), "https://play.example.com/" + Guid.NewGuid().ToString("N"));
        }
        else
        {
            asset.MarkProcessing();
            asset.MarkProcessingFailed(failureReason ?? "transcode error");
        }

        db.DataAssets.Add(asset);
        return asset.Id;
    }

    private static async Task<AssessmentPlanResponse> CreatePlanAsync(HttpClient client, Guid protocolId)
    {
        var request = new CreateAssessmentPlanRequest(
            "Jump Assessment",
            "JUMP-ASM",
            null,
            protocolId,
            "JUMP",
            new[]
            {
                new CreateAssessmentPlanItemRequest("jump_height", "Jump Height", "cm", "Scalar", true, 0, 100),
                new CreateAssessmentPlanItemRequest("rsi", "Reactive Strength Index", "m/s", "Scalar", false, null, null)
            });

        var created = await (await client.PostAsJsonAsync("/api/v1/assessmentplans", request))
            .Content.ReadFromJsonAsync<AssessmentPlanResponse>();

        await client.PostAsync($"/api/v1/assessmentplans/{created!.Id}/publish", null);
        return created;
    }

    [Fact]
    public async Task GenerateReport_WithAllRequiredMetrics_ReturnsCompleteWithSnapshotValues()
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
                ("jump_height", "Jump Height", 42.5, "cm"),
                ("rsi", "Reactive Strength Index", 2.1, "m/s"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var plan = await CreatePlanAsync(client, protocolId);

        var generateRequest = new GenerateReportRequest(plan.Id, subjectId, new[] { sessionId }, "My Report", null);
        var response = await client.PostAsJsonAsync("/api/v1/reports", generateRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var report = await response.Content.ReadFromJsonAsync<ReportDetailResponse>();
        Assert.NotNull(report);
        Assert.Equal("Complete", report.Status);
        Assert.Equal(plan.Version, report.PlanVersion);
        Assert.Equal("JUMP-ASM", report.PlanCode);

        var heightMetric = report.MetricValues.First(m => m.MetricCode == "jump_height");
        Assert.Equal(42.5, heightMetric.Value);
        Assert.True(heightMetric.Required);

        var rsiMetric = report.MetricValues.First(m => m.MetricCode == "rsi");
        Assert.Equal(2.1, rsiMetric.Value);
        Assert.False(rsiMetric.Required);
    }

    [Fact]
    public async Task GenerateReport_MissingRequiredMetric_ReturnsDraft_NeverZeroFilled()
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
                ("rsi", "Reactive Strength Index", 2.1, "m/s"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var plan = await CreatePlanAsync(client, protocolId);

        var generateRequest = new GenerateReportRequest(plan.Id, subjectId, new[] { sessionId }, null, null);
        var report = await (await client.PostAsJsonAsync("/api/v1/reports", generateRequest))
            .Content.ReadFromJsonAsync<ReportDetailResponse>();

        Assert.Equal("Draft", report!.Status);
        var heightMetric = report.MetricValues.First(m => m.MetricCode == "jump_height");
        Assert.Null(heightMetric.Value);
        Assert.True(heightMetric.Required);

        var rsiMetric = report.MetricValues.First(m => m.MetricCode == "rsi");
        Assert.Equal(2.1, rsiMetric.Value);
    }

    [Fact]
    public async Task GeneratedReport_IsImmutable_WhenSourceResultChanges()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty, protocolId = Guid.Empty, sessionId = Guid.Empty, resultId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, subjectId, protocolId, DateTimeOffset.UtcNow.AddDays(-1));
            resultId = SeedResultWithMetrics(db, orgId, sessionId, protocolId,
                ("jump_height", "Jump Height", 42.5, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var plan = await CreatePlanAsync(client, protocolId);

        var generateRequest = new GenerateReportRequest(plan.Id, subjectId, new[] { sessionId }, null, null);
        var report = await (await client.PostAsJsonAsync("/api/v1/reports", generateRequest))
            .Content.ReadFromJsonAsync<ReportDetailResponse>();

        Assert.Equal(42.5, report!.MetricValues.First(m => m.MetricCode == "jump_height").Value);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
            var result = db.Results.Single(r => r.Id == resultId);
            var metric = db.Metrics.Single(m => m.ResultId == resultId && m.MetricCode == "jump_height");
            db.Entry(metric).Property("ScalarValue").CurrentValue = 99.9;
            await db.SaveChangesAsync();
        }

        var reloaded = await client.GetFromJsonAsync<ReportDetailResponse>($"/api/v1/reports/{report.Id}");
        Assert.Equal(42.5, reloaded!.MetricValues.First(m => m.MetricCode == "jump_height").Value);
    }

    [Fact]
    public async Task GeneratedReport_ReferencesOriginalPlanVersion_AfterNewPlanVersion()
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
                ("jump_height", "Jump Height", 42.5, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var plan = await CreatePlanAsync(client, protocolId);

        var generateRequest = new GenerateReportRequest(plan.Id, subjectId, new[] { sessionId }, null, null);
        var report = await (await client.PostAsJsonAsync("/api/v1/reports", generateRequest))
            .Content.ReadFromJsonAsync<ReportDetailResponse>();

        Assert.Equal(1, report!.PlanVersion);

        await client.PostAsync($"/api/v1/assessmentplans/{plan.Id}/new-version", null);

        var reloaded = await client.GetFromJsonAsync<ReportDetailResponse>($"/api/v1/reports/{report.Id}");
        Assert.Equal(1, reloaded!.PlanVersion);
    }

    [Fact]
    public async Task GenerateReport_WithOneFailedAsset_DoesNotInvalidateReport()
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
                ("jump_height", "Jump Height", 42.5, "cm"));
            SeedAsset(db, orgId, sessionId, null, "video", ready: false, failureReason: "transcode failed");
            SeedAsset(db, orgId, sessionId, null, "video", ready: true);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var plan = await CreatePlanAsync(client, protocolId);

        var generateRequest = new GenerateReportRequest(plan.Id, subjectId, new[] { sessionId }, null, null);
        var report = await (await client.PostAsJsonAsync("/api/v1/reports", generateRequest))
            .Content.ReadFromJsonAsync<ReportDetailResponse>();

        Assert.Equal("Complete", report!.Status);
        Assert.Equal(2, report.AssetStates.Count);

        var failed = report.AssetStates.First(a => a.Status == "ProcessingFailed");
        Assert.Equal("transcode failed", failed.FailureReason);
        Assert.False(failed.PlaybackAuthorized);

        var ready = report.AssetStates.First(a => a.Status == "Ready");
        Assert.True(ready.PlaybackAuthorized);
        Assert.NotNull(ready.PlaybackUrl);
    }

    [Fact]
    public async Task RequestPlayback_ForReadyAsset_ReturnsAuthorizedUrl()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty, protocolId = Guid.Empty, sessionId = Guid.Empty;
        Guid readyAssetId = Guid.Empty, failedAssetId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, subjectId, protocolId, DateTimeOffset.UtcNow.AddDays(-1));
            SeedResultWithMetrics(db, orgId, sessionId, protocolId,
                ("jump_height", "Jump Height", 42.5, "cm"));
            failedAssetId = SeedAsset(db, orgId, sessionId, null, "video", ready: false, "broken");
            readyAssetId = SeedAsset(db, orgId, sessionId, null, "video", ready: true);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var plan = await CreatePlanAsync(client, protocolId);

        var generateRequest = new GenerateReportRequest(plan.Id, subjectId, new[] { sessionId }, null, null);
        var report = await (await client.PostAsJsonAsync("/api/v1/reports", generateRequest))
            .Content.ReadFromJsonAsync<ReportDetailResponse>();

        var readyAuth = await client.PostAsync($"/api/v1/reports/{report!.Id}/playback/{readyAssetId}", null);
        Assert.Equal(HttpStatusCode.OK, readyAuth.StatusCode);
        var readyResult = await readyAuth.Content.ReadFromJsonAsync<PlaybackAuthorizationResponse>();
        Assert.True(readyResult!.Authorized);
        Assert.NotNull(readyResult.PlaybackUrl);
        Assert.NotNull(readyResult.ExpiresAt);

        var failedAuth = await client.PostAsync($"/api/v1/reports/{report.Id}/playback/{failedAssetId}", null);
        var failedResult = await failedAuth.Content.ReadFromJsonAsync<PlaybackAuthorizationResponse>();
        Assert.False(failedResult!.Authorized);
        Assert.Null(failedResult.PlaybackUrl);
    }

    [Fact]
    public async Task ListReports_ReturnsGeneratedReportsForOrganization()
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
                ("jump_height", "Jump Height", 42.5, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var plan = await CreatePlanAsync(client, protocolId);

        var generateRequest = new GenerateReportRequest(plan.Id, subjectId, new[] { sessionId }, "Report 1", null);
        await client.PostAsJsonAsync("/api/v1/reports", generateRequest);

        var list = await client.GetFromJsonAsync<IReadOnlyList<ReportResponse>>("/api/v1/reports");
        Assert.Single(list!);
        Assert.Equal("Report 1", list[0].Title);
        Assert.Equal("Complete", list[0].Status);
    }
}