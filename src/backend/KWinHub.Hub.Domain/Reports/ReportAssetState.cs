using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Reports;

public class ReportAssetState : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public Guid ReportId { get; private set; }

    public Guid AssetId { get; private set; }

    public Guid SessionId { get; private set; }

    public Guid? TrialId { get; private set; }

    public string AssetType { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public string? MediaProviderId { get; private set; }

    public string? PlaybackUrl { get; private set; }

    public string? FailureReason { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    private ReportAssetState()
    {
    }

    public ReportAssetState(
        Guid organizationId,
        Guid reportId,
        Guid assetId,
        Guid sessionId,
        Guid? trialId,
        string assetType,
        string status,
        string? mediaProviderId,
        string? playbackUrl,
        string? failureReason,
        DateTimeOffset? processedAt)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        ReportId = reportId;
        AssetId = assetId;
        SessionId = sessionId;
        TrialId = trialId;
        AssetType = assetType;
        Status = status;
        MediaProviderId = mediaProviderId;
        PlaybackUrl = playbackUrl;
        FailureReason = failureReason;
        ProcessedAt = processedAt;
    }
}