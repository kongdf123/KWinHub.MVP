using KWinHub.Hub.Subjects;

namespace KWinHub.UnitTests;

public class SubjectTests
{
    [Fact]
    public void Subject_FullName_ReturnsTrimmedCombination()
    {
        var subject = new Subject(
            organizationId: Guid.NewGuid(),
            firstName: "Jane",
            lastName: "Doe",
            subjectType: SubjectType.Athlete);

        Assert.Equal("Jane Doe", subject.FullName);
    }

    [Fact]
    public void Subject_Status_DefaultsToActive()
    {
        var subject = new Subject(
            organizationId: Guid.NewGuid(),
            firstName: "John",
            lastName: "Smith",
            subjectType: SubjectType.Patient);

        Assert.Equal(Subject.StatusActive, subject.Status);
    }

    [Fact]
    public void Subject_Id_IsGeneratedOnCreation()
    {
        var subject = new Subject(
            organizationId: Guid.NewGuid(),
            firstName: "Alice",
            lastName: "Brown",
            subjectType: SubjectType.ResearchSubject);

        Assert.NotEqual(Guid.Empty, subject.Id);
    }

    [Fact]
    public void Subject_Archive_SetsStatusToArchived()
    {
        var subject = new Subject(
            organizationId: Guid.NewGuid(),
            firstName: "Test",
            lastName: "User",
            subjectType: SubjectType.Athlete);

        subject.Archive();

        Assert.Equal(Subject.StatusArchived, subject.Status);
    }

    [Fact]
    public void Subject_Restore_SetsStatusToActive()
    {
        var subject = new Subject(
            organizationId: Guid.NewGuid(),
            firstName: "Test",
            lastName: "User",
            subjectType: SubjectType.Athlete);

        subject.Archive();
        subject.Restore();

        Assert.Equal(Subject.StatusActive, subject.Status);
    }

    [Fact]
    public void Subject_Update_UpdatesAllFields()
    {
        var subject = new Subject(
            organizationId: Guid.NewGuid(),
            firstName: "Old",
            lastName: "Name",
            subjectType: SubjectType.Athlete,
            externalId: "ext-1",
            displayName: "Old Display",
            dateOfBirth: new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
            sex: "M");

        var newDob = new DateTimeOffset(1995, 6, 15, 0, 0, 0, TimeSpan.Zero);
        subject.Update(
            firstName: "New",
            lastName: "Name",
            subjectType: SubjectType.Patient,
            externalId: "ext-2",
            displayName: "New Display",
            dateOfBirth: newDob,
            sex: "F");

        Assert.Equal("New", subject.FirstName);
        Assert.Equal("Name", subject.LastName);
        Assert.Equal(SubjectType.Patient, subject.SubjectType);
        Assert.Equal("ext-2", subject.ExternalId);
        Assert.Equal("New Display", subject.DisplayName);
        Assert.Equal(newDob, subject.DateOfBirth);
        Assert.Equal("F", subject.Sex);
    }
}