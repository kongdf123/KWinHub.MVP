namespace KWinHub.Hub.Contracts.Sessions;

public record DeviceResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string? Model,
    string? Manufacturer,
    string? SerialNumber,
    string? FirmwareVersion,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);