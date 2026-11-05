namespace KWinHub.Hub.Contracts.History;

public record HistoryMetricResponse(
    Guid MetricId,
    string MetricCode,
    string MetricName,
    string ValueType,
    double? ScalarValue,
    double? LeftValue,
    double? RightValue,
    string? CategoricalValue,
    string? StructuredValue,
    string Unit,
    string? Side);

public record HistorySessionResponse(
    Guid SessionId,
    Guid SubjectId,
    Guid ProtocolId,
    string ProtocolCode,
    string ProtocolVersion,
    string Status,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? ExternalId,
    Guid? ResultId,
    string? ResultStatus,
    string? QualityStatus,
    IReadOnlyList<HistoryMetricResponse> Metrics);

public record HistoryListResponse(
    Guid SubjectId,
    int TotalCount,
    IReadOnlyList<HistorySessionResponse> Sessions);

public record HistoryFilters(
    Guid? ProtocolId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? Status = null);