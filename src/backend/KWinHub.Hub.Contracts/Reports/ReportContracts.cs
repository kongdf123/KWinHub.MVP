namespace KWinHub.Hub.Contracts.Reports;

public record GenerateReportRequest(
    Guid PlanId,
    Guid SubjectId,
    IReadOnlyList<Guid> SessionIds,
    string? Title,
    string? Notes);

public record ReportMetricValueResponse(
    Guid Id,
    Guid SessionId,
    Guid? TrialId,
    Guid? ResultId,
    DateTimeOffset? SessionStartedAt,
    string MetricCode,
    string MetricName,
    string Unit,
    double? Value,
    bool Required,
    int PlanItemSortOrder);

public record ReportAssetStateResponse(
    Guid Id,
    Guid AssetId,
    Guid SessionId,
    Guid? TrialId,
    string AssetType,
    string Status,
    string? MediaProviderId,
    string? PlaybackUrl,
    string? FailureReason,
    DateTimeOffset? ProcessedAt,
    bool PlaybackAuthorized);

public record ReportResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? PlanId,
    string PlanCode,
    int PlanVersion,
    string PlanName,
    Guid SubjectId,
    Guid GeneratedByUserId,
    string Title,
    string Status,
    string? Notes,
    DateTimeOffset GeneratedAt,
    int MetricValueCount,
    int AssetStateCount);

public record ReportDetailResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? PlanId,
    string PlanCode,
    int PlanVersion,
    string PlanName,
    Guid SubjectId,
    Guid GeneratedByUserId,
    string Title,
    string Status,
    string? Notes,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<ReportMetricValueResponse> MetricValues,
    IReadOnlyList<ReportAssetStateResponse> AssetStates);

public record PlaybackAuthorizationResponse(
    Guid AssetId,
    bool Authorized,
    string? PlaybackUrl,
    string? ExpiresAt,
    string? Reason);