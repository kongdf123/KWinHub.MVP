using System.Reflection;
using System.Text.Json;
using KWinHub.Hub.Contracts.Ingestion;

namespace KWinHub.ContractTests;

public class IngestionContractTests
{
    private const string SupportedContractVersion = "1.0";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static string ReadFixture(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"KWinHub.ContractTests.Fixtures.{name}";
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Fixture not found: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static IngestSessionRequest LoadFixture(string name)
    {
        var json = ReadFixture(name);
        var request = JsonSerializer.Deserialize<IngestSessionRequest>(json, JsonOptions);
        Assert.NotNull(request);
        return request!;
    }

    [Fact]
    public void CompleteSession_DeserializesCorrectly()
    {
        var request = LoadFixture("complete-session.json");

        Assert.Equal(SupportedContractVersion, request.Version);
        Assert.Equal("op-cmj-complete-001", request.ClientOperationId);
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), request.SubjectId);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), request.ProtocolId);
        Assert.Equal("1.0", request.ProtocolVersion);
        Assert.Equal("ext-cmj-001", request.ExternalId);
        Assert.NotNull(request.SourceId);
        Assert.NotNull(request.DeviceId);
        Assert.NotNull(request.StartedAt);
        Assert.Equal("UTC", request.Timezone);
        Assert.Equal("KWinHub.iOS", request.ClientApplication);
        Assert.Equal("1.0.0", request.ClientVersion);
        Assert.False(request.IsCorrection);

        Assert.NotNull(request.Trials);
        Assert.Equal(2, request.Trials!.Count);

        var trial0 = request.Trials[0];
        Assert.Equal(0, trial0.TrialIndex);
        Assert.Equal("trial-0", trial0.ExternalId);
        Assert.Equal("Warm-up", trial0.Notes);
        Assert.NotNull(trial0.Metrics);
        Assert.Equal(2, trial0.Metrics!.Count);

        var metric0 = trial0.Metrics[0];
        Assert.Equal("cmj.jump_height", metric0.MetricDefinitionCode);
        Assert.Equal("scalar", metric0.ValueType);
        Assert.Equal(28.5, metric0.ScalarValue);
        Assert.Equal("cm", metric0.Unit);

        Assert.NotNull(request.AssetPlaceholders);
        Assert.Single(request.AssetPlaceholders!);
        var asset = request.AssetPlaceholders![0];
        Assert.Equal("force_plate_raw", asset.AssetType);
        Assert.Equal("asset-force-0", asset.ExternalId);
        Assert.Equal(102400, asset.Size);
        Assert.Equal("application/octet-stream", asset.MimeType);
        Assert.Equal("sha256:abc123", asset.Checksum);
    }

    [Fact]
    public void PartialSession_DeserializesWithoutTrials()
    {
        var request = LoadFixture("partial-session.json");

        Assert.Equal(SupportedContractVersion, request.Version);
        Assert.Equal("op-cmj-partial-001", request.ClientOperationId);
        Assert.Null(request.Trials);
        Assert.Null(request.AssetPlaceholders);
    }

    [Fact]
    public void VersionMismatch_HasUnsupportedVersion()
    {
        var request = LoadFixture("version-mismatch.json");

        Assert.NotEqual(SupportedContractVersion, request.Version);
        Assert.Equal("2.0", request.Version);
    }

    [Fact]
    public void RetrySession_HasSameOperationIdAndContent()
    {
        var complete = LoadFixture("complete-session.json");
        var retry = LoadFixture("retry-session.json");

        Assert.Equal(complete.ClientOperationId, retry.ClientOperationId);
        Assert.Equal(complete.Version, retry.Version);
        Assert.Equal(complete.SubjectId, retry.SubjectId);
        Assert.Equal(complete.ProtocolId, retry.ProtocolId);
        Assert.Equal(complete.ProtocolVersion, retry.ProtocolVersion);
        Assert.Equal(complete.ExternalId, retry.ExternalId);
        Assert.Equal(complete.SourceId, retry.SourceId);
        Assert.Equal(complete.DeviceId, retry.DeviceId);
        Assert.Equal(complete.StartedAt, retry.StartedAt);
        Assert.Equal(complete.Timezone, retry.Timezone);
        Assert.Equal(complete.ClientApplication, retry.ClientApplication);
        Assert.Equal(complete.ClientVersion, retry.ClientVersion);
        Assert.Equal(complete.IsCorrection, retry.IsCorrection);

        Assert.NotNull(complete.Trials);
        Assert.NotNull(retry.Trials);
        Assert.Equal(complete.Trials!.Count, retry.Trials!.Count);
        for (var i = 0; i < complete.Trials.Count; i++)
        {
            Assert.Equal(complete.Trials[i].TrialIndex, retry.Trials[i].TrialIndex);
            Assert.Equal(complete.Trials[i].ExternalId, retry.Trials[i].ExternalId);
            Assert.Equal(complete.Trials[i].Notes, retry.Trials[i].Notes);
            Assert.NotNull(complete.Trials[i].Metrics);
            Assert.NotNull(retry.Trials[i].Metrics);
            Assert.Equal(complete.Trials[i].Metrics!.Count, retry.Trials[i].Metrics!.Count);
            for (var j = 0; j < complete.Trials[i].Metrics.Count; j++)
            {
                Assert.Equal(complete.Trials[i].Metrics[j].MetricDefinitionCode, retry.Trials[i].Metrics[j].MetricDefinitionCode);
                Assert.Equal(complete.Trials[i].Metrics[j].ValueType, retry.Trials[i].Metrics[j].ValueType);
                Assert.Equal(complete.Trials[i].Metrics[j].ScalarValue, retry.Trials[i].Metrics[j].ScalarValue);
                Assert.Equal(complete.Trials[i].Metrics[j].Unit, retry.Trials[i].Metrics[j].Unit);
            }
        }
    }

    [Fact]
    public void DuplicateSession_HasSameOperationIdButDifferentContent()
    {
        var complete = LoadFixture("complete-session.json");
        var duplicate = LoadFixture("duplicate-session.json");

        Assert.Equal(complete.ClientOperationId, duplicate.ClientOperationId);

        Assert.NotNull(complete.Trials);
        Assert.NotNull(duplicate.Trials);
        Assert.NotEqual(complete.Trials!.Count, duplicate.Trials!.Count);

        var completeMetric = complete.Trials[0].Metrics?.FirstOrDefault();
        var duplicateMetric = duplicate.Trials[0].Metrics?.FirstOrDefault();
        Assert.NotNull(completeMetric);
        Assert.NotNull(duplicateMetric);
        Assert.NotEqual(completeMetric!.ScalarValue, duplicateMetric!.ScalarValue);
    }

    [Fact]
    public void AllFixtures_UseCamelCaseJsonNaming()
    {
        var fixtures = new[]
        {
            "complete-session.json",
            "partial-session.json",
            "retry-session.json",
            "duplicate-session.json",
            "version-mismatch.json"
        };

        foreach (var fixture in fixtures)
        {
            var json = ReadFixture(fixture);
            Assert.Contains("\"clientOperationId\"", json);
            Assert.Contains("\"protocolVersion\"", json);
            Assert.DoesNotContain("\"ClientOperationId\"", json);
            Assert.DoesNotContain("\"ProtocolVersion\"", json);
        }
    }
}