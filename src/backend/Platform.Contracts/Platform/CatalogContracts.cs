namespace KWinHub.Contracts.Platform;

public record GlobalProtocolResponse(
    Guid Id,
    string Code,
    string Name,
    string Version,
    string? Category,
    string? Sport,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt);

public record UpsertGlobalProtocolRequest(
    string Code,
    string Name,
    string Version,
    string? Category,
    string? Sport,
    string? Description);

public record GlobalMetricResponse(
    Guid Id,
    string Code,
    string Name,
    string Unit,
    string? Category,
    string? Description,
    string Status,
    DateTimeOffset CreatedAt);

public record UpsertGlobalMetricRequest(
    string Code,
    string Name,
    string Unit,
    string? Category,
    string? Description);

public record DeviceModelResponse(
    Guid Id,
    string Manufacturer,
    string Model,
    string DeviceType,
    string? Notes,
    string Status,
    DateTimeOffset CreatedAt);

public record UpsertDeviceModelRequest(
    string Manufacturer,
    string Model,
    string DeviceType,
    string? Notes);

public record AlgorithmVersionResponse(
    Guid Id,
    string Name,
    string Version,
    string? SupportedMetricCodes,
    string? Notes,
    string Status,
    DateTimeOffset CreatedAt);

public record UpsertAlgorithmVersionRequest(
    string Name,
    string Version,
    string? SupportedMetricCodes,
    string? Notes);

public record UpdateCatalogStatusRequest(string Action);
