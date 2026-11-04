using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Application.Messaging;
using KWinHub.Hub.Assets;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Messaging;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Sessions;
using KWinHub.Hub.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Messaging;

public class OutboxIntegrationTests : IClassFixture<OutboxTestFactory>
{
    private readonly OutboxTestFactory _factory;

    public OutboxIntegrationTests(OutboxTestFactory factory)
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

    private static Guid SeedAsset(HubDbContext db, Guid orgId, Guid sessionId, long size = 1024)
    {
        var asset = new DataAsset(orgId, sessionId, null, "video", size, $"ext-{Guid.NewGuid():N}", "video/mp4", null);
        db.DataAssets.Add(asset);
        return asset.Id;
    }

    [Fact]
    public async Task CompleteUpload_Atomicity_AssetAndOutboxRowCommitTogether()
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
            assetId = SeedAsset(db, orgId, sessionId, size);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        var storageKey = $"orgs/{orgId}/sessions/{sessionId}/assets/{assetId}/r1";
        _factory.ObjectStorage.SetObjectSize("kwinhub-assets", storageKey, size);

        var response = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();

        var asset = await db.DataAssets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(DataAssetStatus.UploadedVerified, asset.Status);

        var outboxMessage = await db.OutboxMessages
            .SingleOrDefaultAsync(m => m.AggregateId == assetId && m.EventType == "asset.verified");
        Assert.NotNull(outboxMessage);
        Assert.Equal(orgId, outboxMessage.OrganizationId);
        Assert.Equal("DataAsset", outboxMessage.AggregateType);
        Assert.Equal(asset.Revision, outboxMessage.EventVersion);
        Assert.Equal(OutboxMessageStatus.Pending, outboxMessage.Status);
    }

    [Fact]
    public async Task CompleteUpload_VerificationFailed_DoesNotCreateOutboxRow()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid assetId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            assetId = SeedAsset(db, orgId, sessionId, 1024);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        var storageKey = $"orgs/{orgId}/sessions/{sessionId}/assets/{assetId}/r1";
        _factory.ObjectStorage.SetObjectSize("kwinhub-assets", storageKey, 999);

        var response = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();

        var outboxCount = await db.OutboxMessages
            .CountAsync(m => m.AggregateId == assetId);
        Assert.Equal(0, outboxCount);
    }

    [Fact]
    public async Task Relay_ProcessesPendingMessage_PublishesToBusAndMarksSent()
    {
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            db.OutboxMessages.Add(new OutboxMessage(
                eventId,
                "asset.verified",
                orgId,
                "DataAsset",
                assetId,
                1,
                "{\"assetId\":\"" + assetId + "\"}"));
        });

        var relay = _factory.GetRelay();
        await relay.ProcessBatchAsync(CancellationToken.None);

        var bus = _factory.GetMessageBus();
        var published = bus.DrainAll();
        Assert.Single(published);
        Assert.Equal(eventId, published[0].EventId);
        Assert.Equal("asset.verified", published[0].EventType);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var message = await db.OutboxMessages.SingleAsync(m => m.EventId == eventId);
        Assert.Equal(OutboxMessageStatus.Sent, message.Status);
        Assert.NotNull(message.ProcessedAt);
    }

    [Fact]
    public async Task Relay_KillResume_ExpiredLeaseMessageIsReprocessed()
    {
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var message = new OutboxMessage(
                eventId,
                "asset.verified",
                orgId,
                "DataAsset",
                assetId,
                1,
                "{\"assetId\":\"" + assetId + "\"}");
            message.Claim(TimeSpan.FromSeconds(30));
            message.GetType().GetProperty("LeaseExpiresAt")!
                .SetValue(message, DateTimeOffset.UtcNow.AddMinutes(-1));
            db.OutboxMessages.Add(message);
        });

        var relay = _factory.GetRelay();
        await relay.ProcessBatchAsync(CancellationToken.None);

        var bus = _factory.GetMessageBus();
        var published = bus.DrainAll();
        Assert.Single(published);
        Assert.Equal(eventId, published[0].EventId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var message = await db.OutboxMessages.SingleAsync(m => m.EventId == eventId);
        Assert.Equal(OutboxMessageStatus.Sent, message.Status);
    }

    [Fact]
    public async Task Relay_AlreadySentMessage_IsNotReprocessed()
    {
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var message = new OutboxMessage(
                eventId,
                "asset.verified",
                orgId,
                "DataAsset",
                assetId,
                1,
                "{\"assetId\":\"" + assetId + "\"}");
            message.MarkSent();
            db.OutboxMessages.Add(message);
        });

        var relay = _factory.GetRelay();
        await relay.ProcessBatchAsync(CancellationToken.None);

        var bus = _factory.GetMessageBus();
        var published = bus.DrainAll();
        Assert.Empty(published);
    }

    [Fact]
    public async Task DuplicateEventId_ThrowsOnSecondInsert()
    {
        var orgId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            db.OutboxMessages.Add(new OutboxMessage(
                eventId,
                "asset.verified",
                orgId,
                "DataAsset",
                Guid.NewGuid(),
                1,
                "{}"));
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();

        var duplicate = new OutboxMessage(
            eventId,
            "asset.verified",
            orgId,
            "DataAsset",
            Guid.NewGuid(),
            1,
            "{}");

        db.OutboxMessages.Add(duplicate);
        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public void PayloadContract_RejectsUrls()
    {
        var payload = "{\"url\":\"https://example.com/resource\"}";
        Assert.Throws<InvalidPayloadException>(() => PayloadContractLinter.Validate(payload));
    }

    [Fact]
    public void PayloadContract_RejectsSecrets()
    {
        var payload = "{\"password\":\"secret123\"}";
        Assert.Throws<InvalidPayloadException>(() => PayloadContractLinter.Validate(payload));
    }

    [Fact]
    public void PayloadContract_AcceptsIdsAndVersions()
    {
        var payload = "{\"assetId\":\"" + Guid.NewGuid() + "\",\"sessionId\":\"" + Guid.NewGuid() + "\",\"revision\":1}";
        PayloadContractLinter.Validate(payload);
    }

    [Fact]
    public async Task AssetVerifiedPayload_DoesNotContainUrlsOrSecrets()
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
            assetId = SeedAsset(db, orgId, sessionId, size);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        var storageKey = $"orgs/{orgId}/sessions/{sessionId}/assets/{assetId}/r1";
        _factory.ObjectStorage.SetObjectSize("kwinhub-assets", storageKey, size);
        await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var outboxMessage = await db.OutboxMessages
            .SingleAsync(m => m.AggregateId == assetId && m.EventType == "asset.verified");

        var exception = Record.Exception(() => PayloadContractLinter.Validate(outboxMessage.PayloadJson));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Relay_FailedMessage_IsResetAndRetried()
    {
        var orgId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            var message = new OutboxMessage(
                eventId,
                "asset.verified",
                orgId,
                "DataAsset",
                assetId,
                1,
                "{\"assetId\":\"" + assetId + "\"}");
            message.MarkFailed("transient error");
            db.OutboxMessages.Add(message);
        });

        var relay = _factory.GetRelay();
        await relay.ProcessBatchAsync(CancellationToken.None);

        var bus = _factory.GetMessageBus();
        var published = bus.DrainAll();
        Assert.Single(published);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var message = await db.OutboxMessages.SingleAsync(m => m.EventId == eventId);
        Assert.Equal(OutboxMessageStatus.Sent, message.Status);
    }
}