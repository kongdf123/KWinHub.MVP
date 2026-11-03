using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Groups;

public class Group : BaseEntity<Guid>
{
    public const string StatusActive = "Active";
    public const string StatusArchived = "Archived";

    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? GroupType { get; private set; }

    public string Status { get; private set; } = StatusActive;

    private readonly List<GroupMembership> _memberships = new();
    public IReadOnlyList<GroupMembership> Memberships => _memberships.AsReadOnly();

    private Group()
    {
    }

    public Group(
        Guid organizationId,
        string name,
        string? description = null,
        string? groupType = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name;
        Description = description;
        GroupType = groupType;
    }

    public void Update(string name, string? description, string? groupType)
    {
        Name = name;
        Description = description;
        GroupType = groupType;
    }

    public void Archive()
    {
        Status = StatusArchived;
    }

    public void Restore()
    {
        Status = StatusActive;
    }

    public GroupMembership AddMember(Guid subjectId, string? role = null)
    {
        var existing = _memberships.FirstOrDefault(m => m.SubjectId == subjectId);
        if (existing is not null)
        {
            return existing;
        }

        var membership = new GroupMembership(Id, subjectId, role);
        _memberships.Add(membership);
        return membership;
    }

    public bool RemoveMember(Guid subjectId)
    {
        var membership = _memberships.FirstOrDefault(m => m.SubjectId == subjectId);
        if (membership is null)
        {
            return false;
        }

        _memberships.Remove(membership);
        return true;
    }
}