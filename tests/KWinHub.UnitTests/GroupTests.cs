using KWinHub.Hub.Groups;

namespace KWinHub.UnitTests;

public class GroupTests
{
    [Fact]
    public void Group_Status_DefaultsToActive()
    {
        var group = new Group(Guid.NewGuid(), "Test Group");

        Assert.Equal(Group.StatusActive, group.Status);
    }

    [Fact]
    public void Group_Id_IsGeneratedOnCreation()
    {
        var group = new Group(Guid.NewGuid(), "Test Group");

        Assert.NotEqual(Guid.Empty, group.Id);
    }

    [Fact]
    public void Group_Archive_SetsStatusToArchived()
    {
        var group = new Group(Guid.NewGuid(), "Test Group");

        group.Archive();

        Assert.Equal(Group.StatusArchived, group.Status);
    }

    [Fact]
    public void Group_Restore_SetsStatusToActive()
    {
        var group = new Group(Guid.NewGuid(), "Test Group");

        group.Archive();
        group.Restore();

        Assert.Equal(Group.StatusActive, group.Status);
    }

    [Fact]
    public void Group_Update_UpdatesAllFields()
    {
        var group = new Group(Guid.NewGuid(), "Old Name", "Old Desc", "type-1");

        group.Update("New Name", "New Desc", "type-2");

        Assert.Equal("New Name", group.Name);
        Assert.Equal("New Desc", group.Description);
        Assert.Equal("type-2", group.GroupType);
    }

    [Fact]
    public void Group_AddMember_AddsMembership()
    {
        var group = new Group(Guid.NewGuid(), "Test Group");
        var subjectId = Guid.NewGuid();

        var membership = group.AddMember(subjectId, "Leader");

        Assert.Single(group.Memberships);
        Assert.Equal(subjectId, membership.SubjectId);
        Assert.Equal("Leader", membership.Role);
    }

    [Fact]
    public void Group_AddMember_DuplicateSubject_ReturnsExisting()
    {
        var group = new Group(Guid.NewGuid(), "Test Group");
        var subjectId = Guid.NewGuid();

        var first = group.AddMember(subjectId, "Leader");
        var second = group.AddMember(subjectId, "Member");

        Assert.Single(group.Memberships);
        Assert.Equal(first.Id, second.Id);
        Assert.Equal("Leader", first.Role);
    }

    [Fact]
    public void Group_RemoveMember_RemovesMembership()
    {
        var group = new Group(Guid.NewGuid(), "Test Group");
        var subjectId = Guid.NewGuid();
        group.AddMember(subjectId);

        var removed = group.RemoveMember(subjectId);

        Assert.True(removed);
        Assert.Empty(group.Memberships);
    }

    [Fact]
    public void Group_RemoveMember_NotMember_ReturnsFalse()
    {
        var group = new Group(Guid.NewGuid(), "Test Group");

        var removed = group.RemoveMember(Guid.NewGuid());

        Assert.False(removed);
        Assert.Empty(group.Memberships);
    }
}