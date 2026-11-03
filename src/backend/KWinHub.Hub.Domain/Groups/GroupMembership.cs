using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Groups;

public class GroupMembership : BaseEntity<Guid>
{
    public Guid GroupId { get; private set; }

    public Guid SubjectId { get; private set; }

    public string? Role { get; private set; }

    private GroupMembership()
    {
    }

    public GroupMembership(Guid groupId, Guid subjectId, string? role = null)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        SubjectId = subjectId;
        Role = role;
    }

    public void SetRole(string? role)
    {
        Role = role;
    }
}