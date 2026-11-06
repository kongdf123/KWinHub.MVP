using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Application.Workers;
using KWinHub.Hub.Assets;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Infrastructure.Workers;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Sessions;
using KWinHub.Hub.Subjects;
using KWinHub.Hub.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Assets;

public class MediaWorkerIntegrationTests : IClassFixture<AssetTestFactory>
{
    private readonly AssetTestFactory _factory;

    public MediaWorkerIntegrationTests(AssetTestFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateAuthorizedClient(Guid userId, Guid orgId, string role = OrganizationRoles.Admin)
    {
        var client = _factory.CreateClient();
        var token = Auth.TestTokenFactory.CreateToken(
            new[] { Auth.TestTokenFactory.HubAudience },
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

    private static Guid SeedSession(HubDbContext db, Guid orgId, Guid subjectId, Guid protocolId)
    {
        var session = new MeasurementSession(
            organizationId: orgId,
            subjectId: subjectId,
            protocolId: protocolId,
            protocolVersion: "1.0",
            externalId: $"sess-{Guid.NewGuid():N}");
        db.MeasurementSessions.Add(session);
        return session.Id;
    }

    private static Guid SeedAsset(HubDbContext db, Guid orgId, Guid sessionId, string mimeType, long size = 1024)
    {
        var asset = new DataAsset(orgId, sessionId, null, mimeType.StartsWith("video/") ? "video" : "data", size, $"ext-{Guid.NewGuid():N}", mimeType, null);
        db.DataAssets.Add(asset);
        return asset.Id;
    }

    [Fact]
    public async Task CompleteUpload_VideoAsset_EnqueuesWorkerJob()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid assetId = Guid.Empty;
        const long size = 2048;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            assetId = SeedAsset(db, orgId, sessionId, "video/mp4", size);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        var storageKey = $"orgs/{orgId}/sessions/{sessionId}/assets/{assetId}/r1";
        _factory.ObjectStorage.SetObjectSize("kwinhub-assets", storageKey, size);

        var response = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var jobs = db.WorkerJobs.Where(j => j.AssetId == assetId).ToList();
        Assert.Single(jobs);
        Assert.Equal(WorkerJobStatus.Pending, jobs[0].Status);
        Assert.Equal(WorkerJobType.TranscodeVideo, jobs[0].JobType);
    }

    [Fact]
    public async Task CompleteUpload_NonVideoAsset_DoesNotEnqueueWorkerJob()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid assetId = Guid.Empty;
        const long size = 1024;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            assetId = SeedAsset(db, orgId, sessionId, "text/csv", size);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        var storageKey = $"orgs/{orgId}/sessions/{sessionId}/assets/{assetId}/r1";
        _factory.ObjectStorage.SetObjectSize("kwinhub-assets", storageKey, size);

        var response = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var jobs = db.WorkerJobs.Where(j => j.AssetId == assetId).ToList();
        Assert.Empty(jobs);
    }

    [Fact]
    public async Task MediaWorkerService_ProcessesVideoJob_MarksAssetReady()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid assetId = Guid.Empty;
        const long size = 2048;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            assetId = SeedAsset(db, orgId, sessionId, "video/mp4", size);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        var storageKey = $"orgs/{orgId}/sessions/{sessionId}/assets/{assetId}/r1";
        _factory.ObjectStorage.SetObjectSize("kwinhub-assets", storageKey, size);
        await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        using var scope = _factory.Services.CreateScope();
        var hostedServices = scope.ServiceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
        var workerService = hostedServices.OfType<MediaWorkerService>().First();

        await workerService.ProcessBatchAsync(CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var asset = await db.DataAssets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(DataAssetStatus.Ready, asset.Status);
        Assert.NotNull(asset.MediaProviderId);
        Assert.NotNull(asset.ProcessedAt);

        var job = db.WorkerJobs.First(j => j.AssetId == assetId);
        Assert.Equal(WorkerJobStatus.Completed, job.Status);
    }

    [Fact]
    public async Task MediaWorkerService_JobForMissingAsset_MarksSkipped()
    {
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            db.WorkerJobs.Add(new WorkerJob(assetId, orgId, WorkerJobType.TranscodeVideo));
        });

        using var scope = _factory.Services.CreateScope();
        var hostedServices = scope.ServiceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
        var workerService = hostedServices.OfType<MediaWorkerService>().First();

        await workerService.ProcessBatchAsync(CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var job = db.WorkerJobs.First(j => j.AssetId == assetId);
        Assert.Equal(WorkerJobStatus.Skipped, job.Status);
    }

    [Fact]
    public async Task ResetExpiredLeases_ProcessingJobWithExpiredLease_ResetsToPending()
    {
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var job = new WorkerJob(assetId, orgId, WorkerJobType.TranscodeVideo);
            job.Claim(TimeSpan.FromMinutes(5));
            job.GetType().GetProperty("LeaseExpiresAt")?
                .SetValue(job, DateTimeOffset.UtcNow.AddMinutes(-1));
            db.WorkerJobs.Add(job);
        });

        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<KWinHub.Hub.Application.Workers.IWorkerJobRepository>();
        await repo.ResetExpiredLeasesAsync();

        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var job = db.WorkerJobs.First(j => j.AssetId == assetId);
        Assert.Equal(WorkerJobStatus.Pending, job.Status);
        Assert.Null(job.LeaseExpiresAt);
    }

    [Fact]
    public void WorkerJob_MarkFailed_UnderMaxAttempts_ResetsToPending()
    {
        var job = new WorkerJob(Guid.NewGuid(), Guid.NewGuid(), WorkerJobType.TranscodeVideo, maxAttempts: 3);
        job.Claim(TimeSpan.FromMinutes(5));
        Assert.Equal(1, job.Attempt);

        job.MarkFailed("transcode error");

        Assert.Equal(WorkerJobStatus.Pending, job.Status);
        Assert.Equal("transcode error", job.LastError);
        Assert.Null(job.LeaseExpiresAt);
    }

    [Fact]
    public void WorkerJob_MarkFailed_AtMaxAttempts_MarksFailed()
    {
        var job = new WorkerJob(Guid.NewGuid(), Guid.NewGuid(), WorkerJobType.TranscodeVideo, maxAttempts: 1);
        job.Claim(TimeSpan.FromMinutes(5));
        Assert.Equal(1, job.Attempt);

        job.MarkFailed("transcode error");

        Assert.Equal(WorkerJobStatus.Failed, job.Status);
        Assert.Equal("transcode error", job.LastError);
    }

    [Fact]
    public void WorkerJob_ResetForRetry_FromFailed_ResetsToPending()
    {
        var job = new WorkerJob(Guid.NewGuid(), Guid.NewGuid(), WorkerJobType.TranscodeVideo, maxAttempts: 1);
        job.Claim(TimeSpan.FromMinutes(5));
        job.MarkFailed("transcode error");
        Assert.Equal(WorkerJobStatus.Failed, job.Status);

        job.ResetForRetry();

        Assert.Equal(WorkerJobStatus.Pending, job.Status);
        Assert.Equal(0, job.Attempt);
        Assert.Null(job.LastError);
        Assert.Null(job.LeaseExpiresAt);
    }

    [Fact]
    public async Task GetByOrganizationIdAsync_WithStatusFilter_ReturnsOnlyMatchingJobs()
    {
        var orgId = Guid.NewGuid();
        var assetId1 = Guid.NewGuid();
        var assetId2 = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var job1 = new WorkerJob(assetId1, orgId, WorkerJobType.TranscodeVideo, maxAttempts: 1);
            job1.Claim(TimeSpan.FromMinutes(5));
            job1.MarkFailed("error");
            db.WorkerJobs.Add(job1);

            var job2 = new WorkerJob(assetId2, orgId, WorkerJobType.TranscodeVideo);
            db.WorkerJobs.Add(job2);
        });

        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<KWinHub.Hub.Application.Workers.IWorkerJobRepository>();

        var failedJobs = await repo.GetByOrganizationIdAsync(orgId, WorkerJobStatus.Failed);
        Assert.Single(failedJobs);
        Assert.Equal(WorkerJobStatus.Failed, failedJobs[0].Status);

        var pendingJobs = await repo.GetByOrganizationIdAsync(orgId, WorkerJobStatus.Pending);
        Assert.Single(pendingJobs);
        Assert.Equal(WorkerJobStatus.Pending, pendingJobs[0].Status);
    }

    [Fact]
    public async Task ResetJobForRetryAsync_FailedJob_ResetsToPending()
    {
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        Guid jobId = Guid.Empty;

        await SeedAsync(db =>
        {
            var job = new WorkerJob(assetId, orgId, WorkerJobType.TranscodeVideo, maxAttempts: 1);
            job.Claim(TimeSpan.FromMinutes(5));
            job.MarkFailed("error");
            db.WorkerJobs.Add(job);
            jobId = job.Id;
        });

        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<KWinHub.Hub.Application.Workers.IWorkerJobRepository>();

        await repo.ResetJobForRetryAsync(jobId);

        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var job = await db.WorkerJobs.FindAsync(jobId);
        Assert.NotNull(job);
        Assert.Equal(WorkerJobStatus.Pending, job.Status);
        Assert.Equal(0, job.Attempt);
    }

    [Fact]
    public async Task ResetFailedForRetryAsync_ResetsAllFailedJobsForOrg()
    {
        var orgId = Guid.NewGuid();
        var otherOrgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var job1 = new WorkerJob(Guid.NewGuid(), orgId, WorkerJobType.TranscodeVideo, maxAttempts: 1);
            job1.Claim(TimeSpan.FromMinutes(5));
            job1.MarkFailed("error1");
            db.WorkerJobs.Add(job1);

            var job2 = new WorkerJob(Guid.NewGuid(), orgId, WorkerJobType.TranscodeVideo, maxAttempts: 1);
            job2.Claim(TimeSpan.FromMinutes(5));
            job2.MarkFailed("error2");
            db.WorkerJobs.Add(job2);

            var otherJob = new WorkerJob(Guid.NewGuid(), otherOrgId, WorkerJobType.TranscodeVideo, maxAttempts: 1);
            otherJob.Claim(TimeSpan.FromMinutes(5));
            otherJob.MarkFailed("error3");
            db.WorkerJobs.Add(otherJob);
        });

        using var scope = _factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<KWinHub.Hub.Application.Workers.IWorkerJobRepository>();

        var count = await repo.ResetFailedForRetryAsync(orgId);

        Assert.Equal(2, count);

        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var orgJobs = db.WorkerJobs.Where(j => j.OrganizationId == orgId).ToList();
        Assert.All(orgJobs, j => Assert.Equal(WorkerJobStatus.Pending, j.Status));

        var otherOrgJob = db.WorkerJobs.First(j => j.OrganizationId == otherOrgId);
        Assert.Equal(WorkerJobStatus.Failed, otherOrgJob.Status);
    }

    [Fact]
    public async Task ReconcileStuckJobs_ProcessingJobBeyondThreshold_ResetsToPending()
    {
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var job = new WorkerJob(assetId, orgId, WorkerJobType.TranscodeVideo);
            job.Claim(TimeSpan.FromMinutes(5));
            job.GetType().GetProperty("UpdatedAt")?
                .SetValue(job, DateTimeOffset.UtcNow.AddMinutes(-15));
            db.WorkerJobs.Add(job);
        });

        using (var scope = _factory.Services.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<KWinHub.Hub.Application.Workers.IWorkerJobRepository>();

            var stuckJobs = await repo.GetStuckJobsAsync(TimeSpan.FromMinutes(10));
            Assert.Single(stuckJobs);

            var hostedServices = scope.ServiceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
            var workerService = hostedServices.OfType<MediaWorkerService>().First();
            await workerService.ProcessBatchAsync(CancellationToken.None);
        }

        using (var verifyScope = _factory.Services.CreateScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<HubDbContext>();
            var job = db.WorkerJobs.First(j => j.AssetId == assetId);
            Assert.NotEqual(WorkerJobStatus.Processing, job.Status);
        }
    }

    [Fact]
    public async Task WorkerJobsController_List_ReturnsJobsForOrg()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.WorkerJobs.Add(new WorkerJob(assetId, orgId, WorkerJobType.TranscodeVideo));
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.GetAsync("/api/v1/worker-jobs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var jobs = await response.Content.ReadFromJsonAsync<List<WorkerJobResponse>>();
        Assert.NotNull(jobs);
        Assert.Single(jobs);
    }

    [Fact]
    public async Task WorkerJobsController_Replay_FailedJob_ReturnsNoContent()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        Guid jobId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var job = new WorkerJob(assetId, orgId, WorkerJobType.TranscodeVideo, maxAttempts: 1);
            job.Claim(TimeSpan.FromMinutes(5));
            job.MarkFailed("error");
            db.WorkerJobs.Add(job);
            jobId = job.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.PostAsync($"/api/v1/worker-jobs/{jobId}/replay", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var job = await db.WorkerJobs.FindAsync(jobId);
        Assert.NotNull(job);
        Assert.Equal(WorkerJobStatus.Pending, job.Status);
    }

    [Fact]
    public async Task MediaWorkerService_OnlyProcessesMediaQueueJobs()
    {
        var orgId = Guid.NewGuid();
        var mediaAssetId = Guid.NewGuid();
        var reportAssetId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var mediaJob = new WorkerJob(mediaAssetId, orgId, WorkerJobType.TranscodeVideo, queueName: WorkerQueueName.Media);
            db.WorkerJobs.Add(mediaJob);

            var reportJob = new WorkerJob(reportAssetId, orgId, "GenerateReport", queueName: WorkerQueueName.Report);
            db.WorkerJobs.Add(reportJob);
        });

        using var scope = _factory.Services.CreateScope();
        var hostedServices = scope.ServiceProvider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();
        var workerService = hostedServices.OfType<MediaWorkerService>().First();

        await workerService.ProcessBatchAsync(CancellationToken.None);

        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var mediaJob = db.WorkerJobs.First(j => j.AssetId == mediaAssetId);
        var reportJob = db.WorkerJobs.First(j => j.AssetId == reportAssetId);

        Assert.Equal(WorkerJobStatus.Skipped, mediaJob.Status);
        Assert.Equal(WorkerJobStatus.Pending, reportJob.Status);
    }

    [Fact]
    public async Task MediaCallback_CompletedStatus_MarksAssetReady()
    {
        var orgId = Guid.NewGuid();
        Guid assetId = Guid.Empty;

        await SeedAsync(db =>
        {
            var asset = new DataAsset(orgId, Guid.NewGuid(), null, "video", 1024, $"ext-{Guid.NewGuid():N}", "video/mp4", null);
            asset.MarkUploaded("key", "bucket");
            asset.MarkVerified();
            asset.MarkProcessing();
            db.DataAssets.Add(asset);
            assetId = asset.Id;

            var job = new WorkerJob(assetId, orgId, WorkerJobType.TranscodeVideo);
            job.Claim(TimeSpan.FromMinutes(5));
            db.WorkerJobs.Add(job);
        });

        var client = _factory.CreateClient();
        var payload = new { AssetId = assetId, Status = "completed", ProviderId = "vod-123", PlaybackUrl = "https://example.com/play.m3u8" };
        var response = await client.PostAsJsonAsync("/api/v1/media-callbacks", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var asset = await db.DataAssets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(DataAssetStatus.Ready, asset.Status);
        Assert.Equal("vod-123", asset.MediaProviderId);
        Assert.Equal("https://example.com/play.m3u8", asset.PlaybackUrl);

        var job = db.WorkerJobs.First(j => j.AssetId == assetId);
        Assert.Equal(WorkerJobStatus.Completed, job.Status);
    }

    [Fact]
    public async Task MediaCallback_FailedStatus_MarksAssetFailed()
    {
        var orgId = Guid.NewGuid();
        Guid assetId = Guid.Empty;

        await SeedAsync(db =>
        {
            var asset = new DataAsset(orgId, Guid.NewGuid(), null, "video", 1024, $"ext-{Guid.NewGuid():N}", "video/mp4", null);
            asset.MarkUploaded("key", "bucket");
            asset.MarkVerified();
            asset.MarkProcessing();
            db.DataAssets.Add(asset);
            assetId = asset.Id;

            var job = new WorkerJob(assetId, orgId, WorkerJobType.TranscodeVideo, maxAttempts: 1);
            job.Claim(TimeSpan.FromMinutes(5));
            db.WorkerJobs.Add(job);
        });

        var client = _factory.CreateClient();
        var payload = new { AssetId = assetId, Status = "failed", Error = "transcode timeout" };
        var response = await client.PostAsJsonAsync("/api/v1/media-callbacks", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var asset = await db.DataAssets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(DataAssetStatus.ProcessingFailed, asset.Status);
        Assert.Equal("transcode timeout", asset.FailureReason);

        var job = db.WorkerJobs.First(j => j.AssetId == assetId);
        Assert.Equal(WorkerJobStatus.Failed, job.Status);
    }

    [Fact]
    public async Task MediaCallback_AlreadyCompletedJob_IsIdempotent()
    {
        var orgId = Guid.NewGuid();
        Guid assetId = Guid.Empty;

        await SeedAsync(db =>
        {
            var asset = new DataAsset(orgId, Guid.NewGuid(), null, "video", 1024, $"ext-{Guid.NewGuid():N}", "video/mp4", null);
            asset.MarkUploaded("key", "bucket");
            asset.MarkVerified();
            asset.MarkProcessing();
            asset.MarkReady("vod-original", "https://example.com/original.m3u8");
            db.DataAssets.Add(asset);
            assetId = asset.Id;

            var job = new WorkerJob(assetId, orgId, WorkerJobType.TranscodeVideo);
            job.Claim(TimeSpan.FromMinutes(5));
            job.MarkCompleted();
            db.WorkerJobs.Add(job);
        });

        var client = _factory.CreateClient();
        var payload = new { AssetId = assetId, Status = "completed", ProviderId = "vod-new", PlaybackUrl = "https://example.com/new.m3u8" };
        var response = await client.PostAsJsonAsync("/api/v1/media-callbacks", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var asset = await db.DataAssets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal("vod-original", asset.MediaProviderId);
        Assert.Equal("https://example.com/original.m3u8", asset.PlaybackUrl);
    }

    [Fact]
    public async Task MediaCallback_NoMatchingJob_ReturnsNoContent()
    {
        var assetId = Guid.NewGuid();

        await SeedAsync(_ => { });

        var client = _factory.CreateClient();
        var payload = new { AssetId = assetId, Status = "completed" };
        var response = await client.PostAsJsonAsync("/api/v1/media-callbacks", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}