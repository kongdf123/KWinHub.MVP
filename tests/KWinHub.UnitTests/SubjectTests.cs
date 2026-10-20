using KWinHub.Domain.Subjects;

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

        Assert.Equal("Active", subject.Status);
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
}