using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Sessions;

public class Device : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Model { get; private set; }

    public string? Manufacturer { get; private set; }

    public string? SerialNumber { get; private set; }

    public string? FirmwareVersion { get; private set; }

    public string Status { get; private set; } = "Active";

    private Device()
    {
    }

    public Device(
        Guid organizationId,
        string name,
        string? model = null,
        string? manufacturer = null,
        string? serialNumber = null,
        string? firmwareVersion = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name;
        Model = model;
        Manufacturer = manufacturer;
        SerialNumber = serialNumber;
        FirmwareVersion = firmwareVersion;
    }

    public void Update(
        string name,
        string? model = null,
        string? manufacturer = null,
        string? serialNumber = null,
        string? firmwareVersion = null)
    {
        Name = name;
        Model = model;
        Manufacturer = manufacturer;
        SerialNumber = serialNumber;
        FirmwareVersion = firmwareVersion;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        Status = "Inactive";
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}