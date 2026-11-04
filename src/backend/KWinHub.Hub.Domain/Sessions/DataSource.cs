using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Sessions;

public class DataSource : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string SourceApp { get; private set; } = string.Empty;

    public string? Version { get; private set; }

    public string? Description { get; private set; }

    public string Status { get; private set; } = "Active";

    private DataSource()
    {
    }

    public DataSource(
        Guid organizationId,
        string name,
        string code,
        string sourceApp,
        string? version = null,
        string? description = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name;
        Code = code;
        SourceApp = sourceApp;
        Version = version;
        Description = description;
    }

    public void Update(string name, string sourceApp, string? version = null, string? description = null)
    {
        Name = name;
        SourceApp = sourceApp;
        Version = version;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        Status = "Inactive";
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}