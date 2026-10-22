using KWinHub.Domain.Common;

namespace KWinHub.Domain.Catalogs;

public class DeviceModel : BaseEntity<Guid>
{
    public string Manufacturer { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public string DeviceType { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public CatalogStatus Status { get; private set; } = CatalogStatus.Draft;

    private DeviceModel()
    {
    }

    public DeviceModel(string manufacturer, string model, string deviceType, string? notes = null)
    {
        Id = Guid.NewGuid();
        Manufacturer = manufacturer;
        Model = model;
        DeviceType = deviceType;
        Notes = notes;
    }

    public void Update(string manufacturer, string model, string deviceType, string? notes)
    {
        Manufacturer = manufacturer;
        Model = model;
        DeviceType = deviceType;
        Notes = notes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Publish()
    {
        Status = CatalogStatus.Published;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        Status = CatalogStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
