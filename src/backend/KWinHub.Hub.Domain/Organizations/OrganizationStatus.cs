using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Organizations;

public class OrganizationStatus : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string Status { get; private set; } = "Active";

    public long Version { get; private set; }

    public DateTimeOffset? EffectiveFrom { get; private set; }

    public DateTimeOffset LastUpdated { get; private set; } = DateTimeOffset.UtcNow;

    private OrganizationStatus()
    {
    }

    public OrganizationStatus(Guid organizationId, string status, long version)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Status = status;
        Version = version;
        EffectiveFrom = DateTimeOffset.UtcNow;
        LastUpdated = DateTimeOffset.UtcNow;
    }

    public bool IsActive => string.Equals(Status, "Active", StringComparison.OrdinalIgnoreCase);

    public bool IsSuspended => string.Equals(Status, "Suspended", StringComparison.OrdinalIgnoreCase);

    public bool ApplyLifecycleEvent(string newStatus, long eventVersion)
    {
        if (eventVersion <= Version)
        {
            return false;
        }

        Status = newStatus;
        Version = eventVersion;
        EffectiveFrom = DateTimeOffset.UtcNow;
        LastUpdated = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
        return true;
    }
}