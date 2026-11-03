using KWinHub.Domain.Common;

namespace KWinHub.Domain.Organizations;

public class OrganizationMembership : BaseEntity<Guid>
{
    public Guid UserId { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string Role { get; private set; } = "Member";

    public string Status { get; private set; } = "Active";

    public DateTimeOffset? InvitedAt { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    private OrganizationMembership()
    {
    }

    public OrganizationMembership(
        Guid userId,
        Guid organizationId,
        string role = "Member")
    {
        Id = Guid.NewGuid();
        UserId = userId;
        OrganizationId = organizationId;
        Role = role;
        Status = "Active";
    }

    public void AcceptInvite()
    {
        Status = "Active";
        AcceptedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Suspend()
    {
        Status = "Suspended";
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeRole(string newRole)
    {
        Role = newRole;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}