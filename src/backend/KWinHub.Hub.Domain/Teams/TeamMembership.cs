using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Teams;

public class TeamMembership : BaseEntity<Guid>
{
    public Guid TeamId { get; private set; }

    public Guid SubjectId { get; private set; }

    public string? Role { get; private set; }

    private TeamMembership()
    {
    }

    public TeamMembership(Guid teamId, Guid subjectId, string? role = null)
    {
        Id = Guid.NewGuid();
        TeamId = teamId;
        SubjectId = subjectId;
        Role = role;
    }

    public void SetRole(string? role)
    {
        Role = role;
    }
}