using KWinHub.Domain.Common;

namespace KWinHub.Domain.Protocols;

public class Protocol : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Category { get; private set; }

    public string? Sport { get; private set; }

    public string Version { get; private set; } = "1.0";

    public ProtocolStatus Status { get; private set; } = ProtocolStatus.Draft;

    public string? Description { get; private set; }

    private Protocol()
    {
    }

    public Protocol(
        Guid organizationId,
        string name,
        string code,
        string version = "1.0",
        string? category = null,
        string? sport = null,
        string? description = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name;
        Code = code;
        Version = version;
        Category = category;
        Sport = sport;
        Description = description;
    }

    public void Publish()
    {
        Status = ProtocolStatus.Published;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deprecate()
    {
        Status = ProtocolStatus.Deprecated;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}