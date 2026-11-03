using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Organizations;

public class OrganizationMembership : BaseEntity<Guid>
{
    public const string StatusActive = "Active";
    public const string StatusSuspended = "Suspended";
    public const string StatusInvited = "Invited";

    public Guid UserId { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Role { get; private set; } = OrganizationRoles.Member;

    public string Status { get; private set; } = StatusInvited;

    public DateTimeOffset? InvitedAt { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    private OrganizationMembership()
    {
    }

    public OrganizationMembership(
        Guid userId,
        Guid organizationId,
        string role = OrganizationRoles.Member)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        OrganizationId = organizationId;
        Role = OrganizationRoles.Normalize(role);
        Status = StatusInvited;
        InvitedAt = DateTimeOffset.UtcNow;
    }

    public bool IsActive => string.Equals(Status, StatusActive, StringComparison.OrdinalIgnoreCase);

    public void AcceptInvite()
    {
        Status = StatusActive;
        AcceptedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend()
    {
        Status = StatusSuspended;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        Status = StatusActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeRole(string newRole)
    {
        Role = OrganizationRoles.Normalize(newRole);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool HasPermission(string permission)
    {
        return IsActive && OrganizationPermissionMatrix.HasPermission(Role, permission);
    }
}