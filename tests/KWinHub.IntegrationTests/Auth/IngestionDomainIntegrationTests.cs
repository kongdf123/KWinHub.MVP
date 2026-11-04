using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Sessions;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Auth;

public class IngestionDomainIntegrationTests : IClassFixture<HubApiFactory>
{
    private readonly HubApiFactory _factory;

    public IngestionDomainIntegrationTests(HubApiFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedAsync(Action<HubDbContext> seed)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        seed(db);
        await db.SaveChangesAsync();
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

    [Fact]
    public async Task CreateTrial_ValidSession_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var protocolId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            sessionId = SeedSession(db, orgId, subjectId, protocolId);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/trials", new
        {
            sessionId = sessionId,
            trialIndex = 1,
            externalId = "trial-001",
            notes = "First attempt"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var trial = await response.Content.ReadFromJsonAsync<TrialResponseDto>();
        Assert.NotNull(trial);
        Assert.Equal(sessionId, trial.SessionId);
        Assert.Equal(1, trial.TrialIndex);
        Assert.Equal("Pending", trial.Status);
        Assert.Equal("trial-001", trial.ExternalId);
    }

    [Fact]
    public async Task CreateTrial_NonExistentSession_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/trials", new
        {
            sessionId = Guid.NewGuid(),
            trialIndex = 1
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListTrialsBySession_ReturnsOrderedTrials()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var protocolId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            sessionId = SeedSession(db, orgId, subjectId, protocolId);
            db.Trials.Add(new Trial(orgId, sessionId, 2));
            db.Trials.Add(new Trial(orgId, sessionId, 1));
            db.Trials.Add(new Trial(orgId, sessionId, 3));
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync($"/api/v1/trials/by-session/{sessionId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var trials = await response.Content.ReadFromJsonAsync<List<TrialResponseDto>>();
        Assert.NotNull(trials);
        Assert.Equal(3, trials.Count);
        Assert.Equal(1, trials[0].TrialIndex);
        Assert.Equal(2, trials[1].TrialIndex);
        Assert.Equal(3, trials[2].TrialIndex);
    }

    [Fact]
    public async Task CompleteTrial_UpdatesStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var protocolId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid trialId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            sessionId = SeedSession(db, orgId, subjectId, protocolId);
            var trial = new Trial(orgId, sessionId, 1);
            db.Trials.Add(trial);
            trialId = trial.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/trials/{trialId}/complete", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var trial = await response.Content.ReadFromJsonAsync<TrialResponseDto>();
        Assert.NotNull(trial);
        Assert.Equal("Completed", trial.Status);
        Assert.NotNull(trial.CompletedAt);
    }

    [Fact]
    public async Task FailTrial_UpdatesStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var protocolId = Guid.NewGuid();
        Guid sessionId = Guid.Empty;
        Guid trialId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            sessionId = SeedSession(db, orgId, subjectId, protocolId);
            var trial = new Trial(orgId, sessionId, 1);
            db.Trials.Add(trial);
            trialId = trial.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/trials/{trialId}/fail", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var trial = await response.Content.ReadFromJsonAsync<TrialResponseDto>();
        Assert.NotNull(trial);
        Assert.Equal("Failed", trial.Status);
    }

    [Fact]
    public async Task GetTrial_CrossTenant_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();
        var subjectId = Guid.NewGuid();
        var protocolId = Guid.NewGuid();
        Guid trialId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgA, OrganizationRoles.Admin);
            var session = new MeasurementSession(orgB, subjectId, protocolId, "1.0");
            db.MeasurementSessions.Add(session);
            var trial = new Trial(orgB, session.Id, 1);
            db.Trials.Add(trial);
            trialId = trial.Id;
        });

        var client = CreateAuthorizedClient(userId, orgA);

        var response = await client.GetAsync($"/api/v1/trials/{trialId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateMetricDefinition_Valid_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/MetricDefinitions", new
        {
            code = "jump_height",
            name = "Jump Height",
            valueType = "Scalar",
            unit = "cm",
            version = "1.0",
            method = "Flight time",
            algorithmVersion = "2.1.0",
            category = "Power"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var def = await response.Content.ReadFromJsonAsync<MetricDefinitionResponseDto>();
        Assert.NotNull(def);
        Assert.Equal("jump_height", def.Code);
        Assert.Equal("Scalar", def.ValueType);
        Assert.Equal("cm", def.Unit);
        Assert.Equal("Active", def.Status);
    }

    [Fact]
    public async Task CreateMetricDefinition_InvalidValueType_ReturnsBadRequest()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/MetricDefinitions", new
        {
            code = "bad",
            name = "Bad",
            valueType = "InvalidType",
            unit = "x"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ListMetricDefinitions_ReturnsDefinitions()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.MetricDefinitions.Add(new MetricDefinition(orgId, "jump_height", "Jump Height", MetricValueType.Scalar, "cm"));
            db.MetricDefinitions.Add(new MetricDefinition(orgId, "sprint_time", "Sprint Time", MetricValueType.Scalar, "s"));
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync("/api/v1/MetricDefinitions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var defs = await response.Content.ReadFromJsonAsync<List<MetricDefinitionResponseDto>>();
        Assert.NotNull(defs);
        Assert.Equal(2, defs.Count);
    }

    [Fact]
    public async Task DeprecateMetricDefinition_UpdatesStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid defId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var def = new MetricDefinition(orgId, "jump_height", "Jump Height", MetricValueType.Scalar, "cm");
            db.MetricDefinitions.Add(def);
            defId = def.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/MetricDefinitions/{defId}/deprecate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var def = await response.Content.ReadFromJsonAsync<MetricDefinitionResponseDto>();
        Assert.NotNull(def);
        Assert.Equal("Deprecated", def.Status);
    }

    [Fact]
    public async Task CreateDataSource_Valid_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/DataSources", new
        {
            name = "KWin iOS App",
            code = "kwin-ios",
            sourceApp = "com.kwin.ios",
            version = "2.0.0"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var source = await response.Content.ReadFromJsonAsync<DataSourceResponseDto>();
        Assert.NotNull(source);
        Assert.Equal("kwin-ios", source.Code);
        Assert.Equal("com.kwin.ios", source.SourceApp);
        Assert.Equal("Active", source.Status);
    }

    [Fact]
    public async Task CreateDevice_Valid_ReturnsCreated()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/devices", new
        {
            name = "Force Plate A",
            model = "FP-1000",
            manufacturer = "KWin",
            serialNumber = "SN-001",
            firmwareVersion = "1.5.2"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var device = await response.Content.ReadFromJsonAsync<DeviceResponseDto>();
        Assert.NotNull(device);
        Assert.Equal("Force Plate A", device.Name);
        Assert.Equal("FP-1000", device.Model);
        Assert.Equal("Active", device.Status);
    }

    [Fact]
    public async Task ListDevices_ReturnsDevices()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.Devices.Add(new Device(orgId, "Force Plate A"));
            db.Devices.Add(new Device(orgId, "Force Plate B"));
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.GetAsync("/api/v1/devices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var devices = await response.Content.ReadFromJsonAsync<List<DeviceResponseDto>>();
        Assert.NotNull(devices);
        Assert.Equal(2, devices.Count);
    }

    [Fact]
    public async Task DeactivateDevice_UpdatesStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid deviceId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            var device = new Device(orgId, "Force Plate A");
            db.Devices.Add(device);
            deviceId = device.Id;
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsync($"/api/v1/devices/{deviceId}/deactivate", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var device = await response.Content.ReadFromJsonAsync<DeviceResponseDto>();
        Assert.NotNull(device);
        Assert.Equal("Inactive", device.Status);
    }

    [Fact]
    public async Task GetMetricDefinition_CrossTenant_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();
        Guid defId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgA, OrganizationRoles.Admin);
            var def = new MetricDefinition(orgB, "jump_height", "Jump Height", MetricValueType.Scalar, "cm");
            db.MetricDefinitions.Add(def);
            defId = def.Id;
        });

        var client = CreateAuthorizedClient(userId, orgA);

        var response = await client.GetAsync($"/api/v1/MetricDefinitions/{defId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record TrialResponseDto(
        Guid Id,
        Guid OrganizationId,
        Guid SessionId,
        int TrialIndex,
        string Status,
        string? ExternalId,
        DateTimeOffset? StartedAt,
        DateTimeOffset? CompletedAt,
        string? Notes,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    private record MetricDefinitionResponseDto(
        Guid Id,
        Guid OrganizationId,
        string Code,
        string Name,
        string ValueType,
        string Unit,
        string Version,
        string? Method,
        string? AlgorithmVersion,
        string? Category,
        string? Description,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    private record DataSourceResponseDto(
        Guid Id,
        Guid OrganizationId,
        string Name,
        string Code,
        string SourceApp,
        string? Version,
        string? Description,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);

    private record DeviceResponseDto(
        Guid Id,
        Guid OrganizationId,
        string Name,
        string? Model,
        string? Manufacturer,
        string? SerialNumber,
        string? FirmwareVersion,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? UpdatedAt);
}