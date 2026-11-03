using KWinHub.Domain.Common;

namespace KWinHub.Domain.Catalogs;

public class GlobalProtocol : BaseEntity<Guid>
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Version { get; private set; } = "1.0";

    public string? Category { get; private set; }

    public string? Sport { get; private set; }

    public string? Description { get; private set; }

    public CatalogStatus Status { get; private set; } = CatalogStatus.Draft;

    private GlobalProtocol()
    {
    }

    public GlobalProtocol(
        string code,
        string name,
        string version = "1.0",
        string? category = null,
        string? sport = null,
        string? description = null)
    {
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Version = version;
        Category = category;
        Sport = sport;
        Description = description;
    }

    public void Update(
        string name,
        string version,
        string? category,
        string? sport,
        string? description)
    {
        Name = name;
        Version = version;
        Category = category;
        Sport = sport;
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
