using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Assets;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Sessions;
using KWinHub.Hub.Subjects;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Assets;

public class PlaybackAuthorizationIntegrationTests : IClassFixture<AssetTestFactory>
{
    private readonly AssetTestFactory _factory;

    public PlaybackAuthorizationIntegrationTests(AssetTestFactory factory)
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

    private static DataAsset SeedReadyVideoAsset(HubDbContext db, Guid orgId, Guid sessionId)
    {
        var asset = new DataAsset(
            orgId, sessionId, null, "video", 2048, $"ext-{Guid.NewGuid():N}", "video/mp4", null);
        asset.MarkUploadInProgress();
        asset.MarkUploaded($"orgs/{orgId}/sessions/{sessionId}/assets/{asset.Id}/r1", "kwinhub-assets");
        asset.MarkVerified();
        asset.MarkProcessing();
        asset.MarkReady("local-ffmpeg", $"https://media.example.com/{asset.Id}/playlist.m3u8");
        db.DataAssets.Add(asset);
        return asset;
    }

    private static DataAsset SeedUnverifiedAsset(HubDbContext db, Guid orgId, Guid sessionId)
    {
        var asset = new DataAsset(
            orgId, sessionId, null, "video", 2048, $"ext-{Guid.NewGuid():N}", "video/mp4", null);
        asset.MarkUploadInProgress();
        asset.MarkUploaded($"orgs/{orgId}/sessions/{sessionId}/assets/{asset.Id}/r1", "kwinhub-assets");
        db.DataAssets.Add(asset);
        return asset;
    }

    [Fact]
    public async Task RequestPlaybackCredential_ReadyAsset_ReturnsPresignedDownloadUrl()
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
            var asset = SeedReadyVideoAsset(db, orgId, sessionId);
            assetId = asset.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.GetAsync($"/api/v1/assets/{assetId}/playback-credential");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var credential = await response.Content.ReadFromJsonAsync<PlaybackCredentialResponse>();
        Assert.NotNull(credential);
        Assert.Equal(assetId, credential.AssetId);
        Assert.StartsWith("https://storage.example.com/kwinhub-assets/", credential.DownloadUrl);
        Assert.Contains("download-token=", credential.DownloadUrl);
        Assert.True(credential.ExpiresAt > DateTimeOffset.UtcNow);
        Assert.Equal("video/mp4", credential.MimeType);
    }

    [Fact]
    public async Task RequestPlaybackCredential_AssetNotReady_ReturnsConflict()
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
            var asset = SeedUnverifiedAsset(db, orgId, sessionId);
            assetId = asset.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.GetAsync($"/api/v1/assets/{assetId}/playback-credential");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RequestPlaybackCredential_AssetNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.GetAsync($"/api/v1/assets/{Guid.NewGuid()}/playback-credential");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RequestPlaybackCredential_ViewerRoleWithoutPermission_ReturnsForbidden()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid assetId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Viewer);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            var asset = SeedReadyVideoAsset(db, orgId, sessionId);
            assetId = asset.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId, OrganizationRoles.Viewer);
        var response = await client.GetAsync($"/api/v1/assets/{assetId}/playback-credential");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task RequestPlaybackCredential_MemberRoleWithPermission_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid assetId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Member);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            var asset = SeedReadyVideoAsset(db, orgId, sessionId);
            assetId = asset.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId, OrganizationRoles.Member);
        var response = await client.GetAsync($"/api/v1/assets/{assetId}/playback-credential");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ListBySession_IncludesAssetStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid readyAssetId = Guid.Empty;
        Guid unverifiedAssetId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            var readyAsset = SeedReadyVideoAsset(db, orgId, sessionId);
            readyAssetId = readyAsset.Id;
            var unverifiedAsset = SeedUnverifiedAsset(db, orgId, sessionId);
            unverifiedAssetId = unverifiedAsset.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.GetAsync($"/api/v1/assets/by-session/{sessionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var assets = await response.Content.ReadFromJsonAsync<List<AssetResponse>>();
        Assert.NotNull(assets);
        Assert.Contains(assets, a => a.Id == readyAssetId && a.Status == "Ready");
        Assert.Contains(assets, a => a.Id == unverifiedAssetId && a.Status == "UploadedUnverified");
    }
}