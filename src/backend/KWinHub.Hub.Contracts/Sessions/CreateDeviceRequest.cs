namespace KWinHub.Hub.Contracts.Sessions;

public record CreateDeviceRequest(
    string Name,
    string? Model = null,
    string? Manufacturer = null,
    string? SerialNumber = null,
    string? FirmwareVersion = null);