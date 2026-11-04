namespace KWinHub.Hub.Contracts.Protocols;

public record UpdateMetricDefinitionRequest(
    string Name,
    string ValueType,
    string Unit,
    string Version,
    string? Method = null,
    string? AlgorithmVersion = null,
    string? Category = null,
    string? Description = null);