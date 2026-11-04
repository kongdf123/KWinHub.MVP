namespace KWinHub.Hub.Contracts.Protocols;

public record MetricDefinitionResponse(
    Guid Id,
    Guid OrganizationId,
    string Code,
    string Name,
    string ValueType,
    string Unit,
    string Version,
    string? Method,
    string? AlgorithmVersion,
    string? Category,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);