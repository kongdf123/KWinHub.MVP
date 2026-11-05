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

public class AssetUploadIntegrationTests : IClassFixture<AssetTestFactory>
{
    private readonly AssetTestFactory _factory;

    public AssetUploadIntegrationTests(AssetTestFactory factory)
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

    private static Guid SeedAsset(HubDbContext db, Guid orgId, Guid sessionId, long size = 1024, string? checksum = null)
    {
        var asset = new DataAsset(orgId, sessionId, null, "video", size, $"ext-{Guid.NewGuid():N}", "video/mp4", checksum);
        db.DataAssets.Add(asset);
        return asset.Id;
    }

    [Fact]
    public async Task RequestUploadGrant_ValidAsset_ReturnsGrantAndSetsInProgress()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var protocolId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid assetId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            subjectId = sId;
            protocolId = pId;
            sessionId = SeedSession(db, orgId, subjectId, protocolId);
            assetId = SeedAsset(db, orgId, sessionId);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var grant = await response.Content.ReadFromJsonAsync<AssetUploadGrantResponse>();
        Assert.NotNull(grant);
        Assert.Equal(assetId, grant.AssetId);
        Assert.False(string.IsNullOrEmpty(grant.UploadUrl));
        Assert.Equal("kwinhub-assets", grant.Bucket);
        Assert.True(grant.ExpiresAt > DateTimeOffset.UtcNow);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var asset = await db.DataAssets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(DataAssetStatus.UploadInProgress, asset.Status);
    }

    [Fact]
    public async Task CompleteUpload_ValidObject_MarksVerified()
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
        var result = await response.Content.ReadFromJsonAsync<AssetResponse>();
        Assert.NotNull(result);
        Assert.Equal("UploadedVerified", result.Status);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var asset = await db.DataAssets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(DataAssetStatus.UploadedVerified, asset.Status);
        Assert.NotNull(asset.VerifiedAt);
    }

    [Fact]
    public async Task CompleteUpload_SizeMismatch_MarksVerificationFailed()
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
        var result = await response.Content.ReadFromJsonAsync<AssetResponse>();
        Assert.NotNull(result);
        Assert.Equal("VerificationFailed", result.Status);
        Assert.Contains("Size mismatch", result.FailureReason);
    }

    [Fact]
    public async Task CompleteUpload_ChecksumMismatch_MarksVerificationFailed()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid assetId = Guid.Empty;
        const long size = 1024;
        const string expectedChecksum = "abc123";

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            assetId = SeedAsset(db, orgId, sessionId, size, expectedChecksum);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        var storageKey = $"orgs/{orgId}/sessions/{sessionId}/assets/{assetId}/r1";
        _factory.ObjectStorage.SetObjectSize("kwinhub-assets", storageKey, size);
        _factory.ObjectStorage.SetObjectChecksum("kwinhub-assets", storageKey, "def456");

        var response = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AssetResponse>();
        Assert.NotNull(result);
        Assert.Equal("VerificationFailed", result.Status);
        Assert.Contains("Checksum mismatch", result.FailureReason);
    }

    [Fact]
    public async Task CompleteUpload_ObjectNotFound_MarksUploadFailed()
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
            assetId = SeedAsset(db, orgId, sessionId);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        var response = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AssetResponse>();
        Assert.NotNull(result);
        Assert.Equal("UploadFailed", result.Status);
        Assert.Contains("Object not found", result.FailureReason);
    }

    [Fact]
    public async Task CompleteUpload_DuplicateNotification_IsIdempotent()
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

        var first = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });
        var second = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var firstResult = await first.Content.ReadFromJsonAsync<AssetResponse>();
        var secondResult = await second.Content.ReadFromJsonAsync<AssetResponse>();
        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);
        Assert.Equal("UploadedVerified", firstResult.Status);
        Assert.Equal("UploadedVerified", secondResult.Status);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var asset = await db.DataAssets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(1, asset.Revision);
    }

    [Fact]
    public async Task CompleteUpload_CrossTenantAsset_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var otherOrgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid assetId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            assetId = SeedAsset(db, otherOrgId, sessionId);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RequestUploadGrant_AlreadyVerified_ReturnsConflict()
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

        var response = await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RequestUploadGrant_FailedThenRetry_ResetsAndIncrementsRevision()
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

        var badStorageKey = $"orgs/{orgId}/sessions/{sessionId}/assets/{assetId}/r1";
        _factory.ObjectStorage.SetObjectSize("kwinhub-assets", badStorageKey, 999);
        await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        var retryResponse = await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);
        Assert.Equal(HttpStatusCode.OK, retryResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var asset = await db.DataAssets.FindAsync(assetId);
        Assert.NotNull(asset);
        Assert.Equal(2, asset.Revision);
        Assert.Equal(DataAssetStatus.UploadInProgress, asset.Status);
    }

    [Fact]
    public async Task ListBySession_ReturnsAssetsForSession()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid otherSessionId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var (sId, pId) = SeedSubjectAndProtocol(db, orgId);
            sessionId = SeedSession(db, orgId, sId, pId);
            otherSessionId = SeedSession(db, orgId, sId, pId);
            SeedAsset(db, orgId, sessionId);
            SeedAsset(db, orgId, sessionId);
            SeedAsset(db, orgId, otherSessionId);
        });

        var client = CreateAuthorizedClient(userId, orgId);
        var response = await client.GetAsync($"/api/v1/assets/by-session/{sessionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var assets = await response.Content.ReadFromJsonAsync<List<AssetResponse>>();
        Assert.NotNull(assets);
        Assert.Equal(2, assets.Count);
    }

    [Fact]
    public async Task CompleteUpload_ContentTypeMismatch_MarksVerificationFailed()
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
        _factory.ObjectStorage.SetObjectContentType("kwinhub-assets", storageKey, "application/octet-stream");

        var response = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AssetResponse>();
        Assert.NotNull(result);
        Assert.Equal("VerificationFailed", result.Status);
        Assert.Contains("Content type mismatch", result.FailureReason);
    }

    [Fact]
    public async Task CompleteUpload_ContentTypeMatches_MarksVerified()
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
        _factory.ObjectStorage.SetObjectContentType("kwinhub-assets", storageKey, "video/mp4");

        var response = await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AssetResponse>();
        Assert.NotNull(result);
        Assert.Equal("UploadedVerified", result.Status);
    }

    [Fact]
    public async Task RequestUploadGrant_RetryDeletesOldStorageObject()
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

        var oldStorageKey = $"orgs/{orgId}/sessions/{sessionId}/assets/{assetId}/r1";
        _factory.ObjectStorage.SetObjectSize("kwinhub-assets", oldStorageKey, 999);
        await client.PostAsJsonAsync("/api/v1/assets/complete", new { AssetId = assetId });

        var oldExists = await _factory.ObjectStorage.ObjectExistsAsync("kwinhub-assets", oldStorageKey);
        Assert.True(oldExists);

        await client.PostAsync($"/api/v1/assets/{assetId}/upload-grant", null);

        var stillExists = await _factory.ObjectStorage.ObjectExistsAsync("kwinhub-assets", oldStorageKey);
        Assert.False(stillExists);
    }
}