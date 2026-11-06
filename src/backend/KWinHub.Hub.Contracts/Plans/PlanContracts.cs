namespace KWinHub.Hub.Contracts.Plans;

public record AssessmentPlanItemResponse(
    Guid Id,
    Guid PlanId,
    string MetricCode,
    string MetricName,
    string Unit,
    string ValueType,
    bool Required,
    double? MinValue,
    double? MaxValue,
    int SortOrder);

public record AssessmentPlanResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Code,
    string? Description,
    int Version,
    string Status,
    Guid? ProtocolId,
    string? ProtocolCode,
    Guid CreatedByUserId,
    IReadOnlyList<AssessmentPlanItemResponse> Items,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public record CreateAssessmentPlanRequest(
    string Name,
    string Code,
    string? Description,
    Guid? ProtocolId,
    string? ProtocolCode,
    IReadOnlyList<CreateAssessmentPlanItemRequest> Items);

public record CreateAssessmentPlanItemRequest(
    string MetricCode,
    string MetricName,
    string Unit,
    string ValueType,
    bool Required,
    double? MinValue,
    double? MaxValue);

public record UpdateAssessmentPlanRequest(
    string Name,
    string? Description,
    Guid? ProtocolId,
    string? ProtocolCode,
    IReadOnlyList<CreateAssessmentPlanItemRequest> Items);

public record CandidateSessionResponse(
    Guid SessionId,
    Guid SubjectId,
    Guid ProtocolId,
    string ProtocolVersion,
    string Status,
    DateTimeOffset? StartedAt,
    bool HasResult,
    bool AllRequiredMetricsPresent,
    IReadOnlyList<string> MissingRequiredMetrics,
    IReadOnlyList<string> IncompatibleMetrics);

public record PlanCompletenessResponse(
    Guid PlanId,
    int PlanVersion,
    Guid SubjectId,
    IReadOnlyList<Guid> SelectedSessionIds,
    bool IsComplete,
    IReadOnlyList<string> BlockingReasons,
    IReadOnlyList<CandidateSessionResponse> CandidateSessions);

public record CheckPlanCompletenessRequest(
    Guid SubjectId,
    IReadOnlyList<Guid> SessionIds);