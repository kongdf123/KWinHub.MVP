using KWinHub.BuildingBlocks.Common;
using KWinHub.Hub.Assets;
using KWinHub.Hub.Plans;
using KWinHub.Hub.Results;
using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Reports;

public class Report : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public Guid? PlanId { get; private set; }

    public string PlanCode { get; private set; } = string.Empty;

    public int PlanVersion { get; private set; }

    public string PlanName { get; private set; } = string.Empty;

    public Guid SubjectId { get; private set; }

    public Guid GeneratedByUserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public ReportStatus Status { get; private set; } = ReportStatus.Draft;

    public string? Notes { get; private set; }

    public DateTimeOffset GeneratedAt { get; private set; }

    private readonly List<ReportMetricValue> _metricValues = new();
    public IReadOnlyList<ReportMetricValue> MetricValues => _metricValues.AsReadOnly();

    private readonly List<ReportAssetState> _assetStates = new();
    public IReadOnlyList<ReportAssetState> AssetStates => _assetStates.AsReadOnly();

    private Report()
    {
    }

    public Report(
        Guid organizationId,
        Guid? planId,
        string planCode,
        int planVersion,
        string planName,
        Guid subjectId,
        Guid generatedByUserId,
        string title,
        string? notes = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        PlanId = planId;
        PlanCode = planCode;
        PlanVersion = planVersion;
        PlanName = planName;
        SubjectId = subjectId;
        GeneratedByUserId = generatedByUserId;
        Title = title;
        Notes = notes;
        GeneratedAt = DateTimeOffset.UtcNow;
    }

    public void AddMetricValue(ReportMetricValue value)
    {
        _metricValues.Add(value);
    }

    public void AddAssetState(ReportAssetState state)
    {
        _assetStates.Add(state);
    }

    public void MarkComplete()
    {
        Status = ReportStatus.Complete;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        Status = ReportStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}