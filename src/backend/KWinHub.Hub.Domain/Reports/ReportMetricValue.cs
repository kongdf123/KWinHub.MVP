using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Reports;

public class ReportMetricValue : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public Guid ReportId { get; private set; }

    public Guid SessionId { get; private set; }

    public Guid? TrialId { get; private set; }

    public Guid? ResultId { get; private set; }

    public DateTimeOffset? SessionStartedAt { get; private set; }

    public string MetricCode { get; private set; } = string.Empty;

    public string MetricName { get; private set; } = string.Empty;

    public string Unit { get; private set; } = string.Empty;

    public double? Value { get; private set; }

    public bool Required { get; private set; }

    public int PlanItemSortOrder { get; private set; }

    private ReportMetricValue()
    {
    }

    public ReportMetricValue(
        Guid organizationId,
        Guid reportId,
        Guid sessionId,
        Guid? trialId,
        Guid? resultId,
        DateTimeOffset? sessionStartedAt,
        string metricCode,
        string metricName,
        string unit,
        double? value,
        bool required,
        int planItemSortOrder)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        ReportId = reportId;
        SessionId = sessionId;
        TrialId = trialId;
        ResultId = resultId;
        SessionStartedAt = sessionStartedAt;
        MetricCode = metricCode;
        MetricName = metricName;
        Unit = unit;
        Value = value;
        Required = required;
        PlanItemSortOrder = planItemSortOrder;
    }
}