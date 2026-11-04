namespace KWinHub.Hub.Contracts.Ingestion;

public record IngestMetricRequest(
    string MetricDefinitionCode,
    string ValueType,
    double? ScalarValue = null,
    double? LeftValue = null,
    double? RightValue = null,
    string? CategoricalValue = null,
    string? StructuredValue = null,
    string? Unit = null,
    string? Side = null);