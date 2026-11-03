namespace KWinHub.Contracts.Results;

public record ResultResponse(
    Guid Id,
    Guid OrganizationId,
    Guid SessionId,
    Guid ProtocolId,
    string ProtocolVersion,
    string AlgorithmVersion,
    string Status,
    string? QualityStatus,
    Guid? TrialId,
    IReadOnlyList<MetricValueResponse> Metrics,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
