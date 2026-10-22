using KWinHub.Domain.Common;

namespace KWinHub.Domain.Catalogs;

public class GlobalMetric : BaseEntity<Guid>
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Unit { get; private set; } = string.Empty;

    public string? Category { get; private set; }

    public string? Description { get; private set; }

    public CatalogStatus Status { get; private set; } = CatalogStatus.Draft;

    private GlobalMetric()
    {
    }

    public GlobalMetric(
        string code,
        string name,
        string unit,
        string? category = null,
        string? description = null)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Unit = unit;
        Category = category;
        Description = description;
    }

    public void Update(string name, string unit, string? category, string? description)
    {
        Name = name;
        Unit = unit;
        Category = category;
        Description = description;
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
