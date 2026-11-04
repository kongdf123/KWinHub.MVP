namespace KWinHub.Hub.Contracts.Protocols;

public record CreateMetricDefinitionRequest(
    string Code,
    string Name,
    string ValueType,
    string Unit,
    string Version = "1.0",
    string? Method = null,
    string? AlgorithmVersion = null,
    string? Category = null,
    string? Description = null);