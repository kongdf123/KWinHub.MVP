using KWinHub.Hub.Teams;

namespace KWinHub.UnitTests;

public class TeamTests
{
    [Fact]
    public void Team_Status_DefaultsToActive()
    {
        var team = new Team(Guid.NewGuid(), "Test Team");

        Assert.Equal(Team.StatusActive, team.Status);
    }

    [Fact]
    public void Team_Id_IsGeneratedOnCreation()
    {
        var team = new Team(Guid.NewGuid(), "Test Team");

        Assert.NotEqual(Guid.Empty, team.Id);
    }

    [Fact]
    public void Team_Archive_SetsStatusToArchived()
    {
        var team = new Team(Guid.NewGuid(), "Test Team");

        team.Archive();

        Assert.Equal(Team.StatusArchived, team.Status);
    }

    [Fact]
    public void Team_Restore_SetsStatusToActive()
    {
        var team = new Team(Guid.NewGuid(), "Test Team");

        team.Archive();
        team.Restore();

        Assert.Equal(Team.StatusActive, team.Status);
    }

    [Fact]
    public void Team_Update_UpdatesAllFields()
    {
        var team = new Team(Guid.NewGuid(), "Old Name", "Old Desc", "sport-1", "2024");

        team.Update("New Name", "New Desc", "sport-2", "2025");

        Assert.Equal("New Name", team.Name);
        Assert.Equal("New Desc", team.Description);
        Assert.Equal("sport-2", team.SportId);
        Assert.Equal("2025", team.Season);
    }

    [Fact]
    public void Team_AddMember_AddsMembership()
    {
        var team = new Team(Guid.NewGuid(), "Test Team");
        var subjectId = Guid.NewGuid();

        var membership = team.AddMember(subjectId, "Captain");

        Assert.Single(team.Memberships);
        Assert.Equal(subjectId, membership.SubjectId);
        Assert.Equal("Captain", membership.Role);
    }

    [Fact]
    public void Team_AddMember_DuplicateSubject_ReturnsExisting()
    {
        var team = new Team(Guid.NewGuid(), "Test Team");
        var subjectId = Guid.NewGuid();

        var first = team.AddMember(subjectId, "Captain");
        var second = team.AddMember(subjectId, "Player");

        Assert.Single(team.Memberships);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal("Captain", first.Role);
    }

    [Fact]
    public void Team_RemoveMember_RemovesMembership()
    {
        var team = new Team(Guid.NewGuid(), "Test Team");
        var subjectId = Guid.NewGuid();
        team.AddMember(subjectId);

        var removed = team.RemoveMember(subjectId);

        Assert.True(removed);
        Assert.Empty(team.Memberships);
    }

    [Fact]
    public void Team_RemoveMember_NotMember_ReturnsFalse()
    {
        var team = new Team(Guid.NewGuid(), "Test Team");

        var removed = team.RemoveMember(Guid.NewGuid());

        Assert.False(removed);
        Assert.Empty(team.Memberships);
    }
}