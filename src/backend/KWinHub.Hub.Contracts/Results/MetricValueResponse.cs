namespace KWinHub.Hub.Contracts.Results;

public record MetricValueResponse(
    Guid Id,
    Guid? MetricDefinitionId,
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