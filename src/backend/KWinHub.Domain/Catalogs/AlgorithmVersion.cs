using KWinHub.Domain.Common;

namespace KWinHub.Domain.Catalogs;

public class AlgorithmVersion : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;

    public string Version { get; private set; } = string.Empty;

    public string? SupportedMetricCodes { get; private set; }

    public string? Notes { get; private set; }

    public CatalogStatus Status { get; private set; } = CatalogStatus.Draft;

    private AlgorithmVersion()
    {
    }

    public AlgorithmVersion(string name, string version, string? supportedMetricCodes = null, string? notes = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Version = version;
        SupportedMetricCodes = supportedMetricCodes;
        Notes = notes;
    }

    public void Update(string name, string version, string? supportedMetricCodes, string? notes)
    {
        Name = name;
        Version = version;
        SupportedMetricCodes = supportedMetricCodes;
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
