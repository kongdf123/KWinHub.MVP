namespace KWinHub.Hub.Contracts.History;

public record MetricExclusionResponse(
    Guid SessionId,
    string Reason);

public record MetricComparisonPointResponse(
    Guid SessionId,
    DateTimeOffset? SessionDate,
    double? Value,
    bool Included,
    string? ExclusionReason);

public record MetricStatisticsResponse(
    double? Mean,
    double? Max,
    int SampleCount,
    string InclusionRule);

public record CompatibleMetricGroupResponse(
    string MetricCode,
    string MetricName,
    string Unit,
    string ValueType,
    MetricStatisticsResponse Statistics,
    IReadOnlyList<MetricComparisonPointResponse> Points,
    IReadOnlyList<MetricExclusionResponse> Exclusions);

public record ComparisonResponse(
    Guid SubjectId,
    IReadOnlyList<Guid> SessionIds,
    IReadOnlyList<CompatibleMetricGroupResponse> CompatibleMetrics,
    IReadOnlyList<string> ExcludedMetricCodes);