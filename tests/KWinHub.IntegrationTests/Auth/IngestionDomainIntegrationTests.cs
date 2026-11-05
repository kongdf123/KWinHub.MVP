using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Sessions;
using KWinHub.Hub.Subjects;
using Microsoft.Extensions.DependencyInjection;

namespace KWinHub.IntegrationTests.Auth;

public class IngestionDomainIntegrationTests : IClassFixture<HubApiSqliteFactory>
{
    private readonly HubApiSqliteFactory _factory;

    public IngestionDomainIntegrationTests(HubApiSqliteFactory factory)
    {
        _factory = factory;
    }

    private async Task SeedAsync(Action<HubDbContext> seed)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        await db.Database.EnsureCreatedAsync();
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

    [Fact]
    public async Task IngestSession_ValidRequest_ReturnsAcceptedWithSessionAndTrials()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            db.MetricDefinitions.Add(new MetricDefinition(orgId, "jump_height", "Jump Height", MetricValueType.Scalar, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "1.0",
            clientOperationId = "op-valid-001",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            externalId = "sess-valid-001",
            trials = new[]
            {
                new
                {
                    trialIndex = 1,
                    externalId = "trial-001",
                    metrics = new[]
                    {
                        new
                        {
                            metricDefinitionCode = "jump_height",
                            valueType = "Scalar",
                            scalarValue = 45.5,
                            unit = "cm"
                        }
                    }
                }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Accepted", result.Status);
        Assert.NotEqual(Guid.Empty, result.SessionId);
        Assert.Equal("sess-valid-001", result.SessionExternalId);
        Assert.Single(result.Trials!);
        Assert.Equal(1, result.Trials![0].TrialIndex);
        Assert.Equal("Accepted", result.Trials![0].Status);
    }

    [Fact]
    public async Task IngestSession_UnknownSubject_ReturnsUnprocessableEntity()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var protocolId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            db.Protocols.Add(new Protocol(orgId, "Jump Test", "JUMP", "1.0"));
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "1.0",
            clientOperationId = "op-unknown-subject",
            subjectId = Guid.NewGuid(),
            protocolId = protocolId,
            protocolVersion = "1.0"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Failed", result.Status);
        Assert.Contains(result.Errors!, e => e.Code == "unknown_subject");
    }

    [Fact]
    public async Task IngestSession_InvalidProtocolVersion_ReturnsUnprocessableEntity()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "1.0",
            clientOperationId = "op-bad-proto-ver",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "2.0"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Errors!, e => e.Code == "invalid_protocol_version");
    }

    [Fact]
    public async Task IngestSession_UnsupportedMetric_ReturnsUnprocessableEntity()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "1.0",
            clientOperationId = "op-unknown-metric",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            trials = new[]
            {
                new
                {
                    trialIndex = 1,
                    metrics = new[]
                    {
                        new { metricDefinitionCode = "nonexistent", valueType = "Scalar", scalarValue = 1.0 }
                    }
                }
            }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Errors!, e => e.Code == "unsupported_metric");
    }

    [Fact]
    public async Task IngestSession_UnitMismatch_ReturnsUnprocessableEntity()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            db.MetricDefinitions.Add(new MetricDefinition(orgId, "jump_height", "Jump Height", MetricValueType.Scalar, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "1.0",
            clientOperationId = "op-unit-mismatch",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            trials = new[]
            {
                new
                {
                    trialIndex = 1,
                    metrics = new[]
                    {
                        new { metricDefinitionCode = "jump_height", valueType = "Scalar", scalarValue = 45.5, unit = "m" }
                    }
                }
            }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Errors!, e => e.Code == "unit_mismatch");
    }

    [Fact]
    public async Task IngestSession_DuplicateClientOperationId_SameContent_ReturnsAccepted()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            db.MetricDefinitions.Add(new MetricDefinition(orgId, "jump_height", "Jump Height", MetricValueType.Scalar, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var payload = new
        {
            version = "1.0",
            clientOperationId = "op-idempotent-001",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            externalId = "sess-idempotent-001",
            trials = new[]
            {
                new
                {
                    trialIndex = 1,
                    metrics = new[]
                    {
                        new { metricDefinitionCode = "jump_height", valueType = "Scalar", scalarValue = 45.5, unit = "cm" }
                    }
                }
            }
        };

        var first = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", payload);
        var second = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", payload);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var firstResult = await first.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        var secondResult = await second.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.Equal(firstResult!.SessionId, secondResult!.SessionId);
    }

    [Fact]
    public async Task IngestSession_DuplicateClientOperationId_DifferentContent_ReturnsConflict()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            db.MetricDefinitions.Add(new MetricDefinition(orgId, "jump_height", "Jump Height", MetricValueType.Scalar, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var firstPayload = new
        {
            version = "1.0",
            clientOperationId = "op-conflict-001",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            externalId = "sess-conflict-001",
            trials = new[]
            {
                new
                {
                    trialIndex = 1,
                    metrics = new[]
                    {
                        new { metricDefinitionCode = "jump_height", valueType = "Scalar", scalarValue = 45.5, unit = "cm" }
                    }
                }
            }
        };

        var secondPayload = new
        {
            version = "1.0",
            clientOperationId = "op-conflict-001",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            externalId = "sess-conflict-001",
            trials = new[]
            {
                new
                {
                    trialIndex = 1,
                    metrics = new[]
                    {
                        new { metricDefinitionCode = "jump_height", valueType = "Scalar", scalarValue = 99.9, unit = "cm" }
                    }
                }
            }
        };

        var first = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", firstPayload);
        var second = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", secondPayload);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);

        var result = await second.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Errors!, e => e.Code == "conflict");
    }

    [Fact]
    public async Task IngestSession_DuplicateExternalId_ReturnsDuplicateSessionError()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var firstPayload = new
        {
            version = "1.0",
            clientOperationId = "op-dup-ext-1",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            externalId = "sess-dup-ext"
        };

        var secondPayload = new
        {
            version = "1.0",
            clientOperationId = "op-dup-ext-2",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            externalId = "sess-dup-ext"
        };

        await client.PostAsJsonAsync("/api/v1/ingestion/sessions", firstPayload);
        var second = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", secondPayload);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, second.StatusCode);
        var result = await second.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Errors!, e => e.Code == "duplicate_session");
    }

    [Fact]
    public async Task IngestSession_Correction_ExistingExternalId_ReturnsAccepted()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
            db.MetricDefinitions.Add(new MetricDefinition(orgId, "jump_height", "Jump Height", MetricValueType.Scalar, "cm"));
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var firstPayload = new
        {
            version = "1.0",
            clientOperationId = "op-correction-1",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            externalId = "sess-correction"
        };

        var correctionPayload = new
        {
            version = "1.0",
            clientOperationId = "op-correction-2",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            externalId = "sess-correction",
            isCorrection = true,
            trials = new[]
            {
                new
                {
                    trialIndex = 1,
                    metrics = new[]
                    {
                        new { metricDefinitionCode = "jump_height", valueType = "Scalar", scalarValue = 50.0, unit = "cm" }
                    }
                }
            }
        };

        var first = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", firstPayload);
        var second = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", correctionPayload);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);

        var firstResult = await first.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        var secondResult = await second.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.Equal(firstResult!.SessionId, secondResult!.SessionId);
    }

    [Fact]
    public async Task IngestSession_WithAssetPlaceholders_ReturnsPendingUpload()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "1.0",
            clientOperationId = "op-assets-001",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            assetPlaceholders = new[]
            {
                new { assetType = "Video", externalId = "vid-001", size = 1024000L, mimeType = "video/mp4" },
                new { assetType = "Image", externalId = "img-001", size = 512000L, mimeType = "image/png" }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.NotNull(result);
        Assert.Equal("Accepted", result.Status);
        Assert.Equal(2, result.AssetPlaceholders!.Count);
        Assert.All(result.AssetPlaceholders!, a => Assert.Equal("PendingUpload", a.Status));
    }

    [Fact]
    public async Task IngestSession_InvalidContractVersion_ReturnsValidationFailed()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "9.9",
            clientOperationId = "op-bad-version",
            subjectId = Guid.NewGuid(),
            protocolId = Guid.NewGuid(),
            protocolVersion = "1.0"
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        Assert.NotNull(result);
        Assert.Contains(result.Errors!, e => e.Code == "validation_failed");
    }

    [Fact]
    public async Task GetOperationStatus_ExistingOperation_ReturnsStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;
        const string clientOpId = "op-status-001";

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "1.0",
            clientOperationId = clientOpId,
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0"
        });

        var response = await client.GetAsync($"/api/v1/ingestion/operations/{clientOpId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var status = await response.Content.ReadFromJsonAsync<OperationStatusResponseDto>();
        Assert.NotNull(status);
        Assert.Equal(clientOpId, status.ClientOperationId);
        Assert.Equal("Accepted", status.Status);
    }

    [Fact]
    public async Task GetSessionStatus_ExistingSession_ReturnsStatus()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var ingest = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "1.0",
            clientOperationId = "op-session-status-001",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0"
        });

        var ingestResult = await ingest.Content.ReadFromJsonAsync<IngestSessionResponseDto>();
        var sessionId = ingestResult!.SessionId;

        var response = await client.GetAsync($"/api/v1/ingestion/sessions/{sessionId}/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var status = await response.Content.ReadFromJsonAsync<OperationStatusResponseDto>();
        Assert.NotNull(status);
        Assert.Equal(sessionId, status.SessionId);
        Assert.Equal("Accepted", status.Status);
    }

    [Fact]
    public async Task IngestSession_ValidationFailure_RollsBackTransaction()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        Guid subjectId = Guid.Empty;
        Guid protocolId = Guid.Empty;

        await SeedAsync(db =>
        {
            SeedActiveMembership(db, userId, orgId, OrganizationRoles.Admin);
            (subjectId, protocolId) = SeedSubjectAndProtocol(db, orgId);
        });

        var client = CreateAuthorizedClient(userId, orgId);

        var response = await client.PostAsJsonAsync("/api/v1/ingestion/sessions", new
        {
            version = "1.0",
            clientOperationId = "op-rollback-001",
            subjectId = subjectId,
            protocolId = protocolId,
            protocolVersion = "1.0",
            externalId = "sess-rollback",
            trials = new[]
            {
                new
                {
                    trialIndex = 1,
                    metrics = new[]
                    {
                        new { metricDefinitionCode = "nonexistent", valueType = "Scalar", scalarValue = 1.0 }
                    }
                }
            }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HubDbContext>();
        var sessionCount = db.MeasurementSessions.Count(s => s.ExternalId == "sess-rollback");
        Assert.Equal(0, sessionCount);
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

    private record IngestTrialResultDto(
        int TrialIndex,
        Guid TrialId,
        string Status);

    private record IngestAssetPlaceholderResultDto(
        string AssetType,
        string ExternalId,
        long Size,
        string Status,
        Guid? TrialId = null);

    private record IngestionErrorDto(
        string Code,
        string Message,
        string? Field = null);

    private record IngestSessionResponseDto(
        Guid SessionId,
        string? SessionExternalId,
        string Status,
        IReadOnlyList<IngestTrialResultDto>? Trials = null,
        IReadOnlyList<IngestAssetPlaceholderResultDto>? AssetPlaceholders = null,
        IReadOnlyList<IngestionErrorDto>? Errors = null);

    private record OperationStatusResponseDto(
        Guid OperationId,
        string ClientOperationId,
        string Status,
        Guid? SessionId = null,
        string? SessionExternalId = null,
        string? ErrorCode = null,
        string? ErrorDetail = null,
        DateTimeOffset CompletedAt = default);
}