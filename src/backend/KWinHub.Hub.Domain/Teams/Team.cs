using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Teams;

public class Team : BaseEntity<Guid>
{
    public const string StatusActive = "Active";
    public const string StatusArchived = "Archived";

    public Guid OrganizationId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? SportId { get; private set; }

    public string? Season { get; private set; }

    public string Status { get; private set; } = StatusActive;

    private readonly List<TeamMembership> _memberships = new();
    public IReadOnlyList<TeamMembership> Memberships => _memberships.AsReadOnly();

    private Team()
    {
    }

    public Team(
        Guid organizationId,
        string name,
        string? description = null,
        string? sportId = null,
        string? season = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name;
        Description = description;
        SportId = sportId;
        Season = season;
    }

    public void Update(string name, string? description, string? sportId, string? season)
    {
        Name = name;
        Description = description;
        SportId = sportId;
        Season = season;
    }

    public void Archive()
    {
        Status = StatusArchived;
    }

    public void Restore()
    {
        Status = StatusActive;
    }

    public TeamMembership AddMember(Guid subjectId, string? role = null)
    {
        var existing = _memberships.FirstOrDefault(m => m.SubjectId == subjectId);
        if (existing is not null)
        {
            return existing;
        }

        var membership = new TeamMembership(Id, subjectId, role);
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