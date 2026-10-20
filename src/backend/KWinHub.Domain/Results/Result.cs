using KWinHub.Domain.Common;

namespace KWinHub.Domain.Results;

public class Result : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public Guid SessionId { get; private set; }

    public Guid? TrialId { get; private set; }

    public Guid ProtocolId { get; private set; }

    public string ProtocolVersion { get; private set; } = string.Empty;

    public string AlgorithmVersion { get; private set; } = string.Empty;

    public string Status { get; private set; } = "Pending";

    public string? QualityStatus { get; private set; }

    private readonly List<Metric> _metrics = new();

    public IReadOnlyCollection<Metric> Metrics => _metrics.AsReadOnly();

    private Result()
    {
    }

    public Result(
        Guid organizationId,
        Guid sessionId,
        Guid protocolId,
        string protocolVersion,
        string algorithmVersion,
        Guid? trialId = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        SessionId = sessionId;
        ProtocolId = protocolId;
        ProtocolVersion = protocolVersion;
        AlgorithmVersion = algorithmVersion;
        TrialId = trialId;
    }

    public void AddMetric(string metricCode, string metricName, double value, string unit, string? side = null)
    {
        _metrics.Add(new Metric(Id, metricCode, metricName, value, unit, side));
    }

    public void MarkPublished(string? qualityStatus = null)
    {
        Status = "Published";
        QualityStatus = qualityStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}