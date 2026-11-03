namespace KWinHub.Hub.Contracts.Results;

public record MetricValueResponse(
    Guid Id,
    string MetricCode,
    string MetricName,
    double Value,
    string Unit,
    string? Side);
