using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Subjects;

public class Subject : BaseEntity<Guid>
{
    public const string StatusActive = "Active";
    public const string StatusArchived = "Archived";

    public Guid OrganizationId { get; private set; }

    public string? ExternalId { get; private set; }

    public SubjectType SubjectType { get; private set; }

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string? DisplayName { get; private set; }

    public DateTimeOffset? DateOfBirth { get; private set; }

    public string? Sex { get; private set; }

    public string Status { get; private set; } = StatusActive;

    private Subject()
    {
    }

    public Subject(
        Guid organizationId,
        string firstName,
        string lastName,
        SubjectType subjectType,
        string? externalId = null,
        string? displayName = null,
        DateTimeOffset? dateOfBirth = null,
        string? sex = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        FirstName = firstName;
        LastName = lastName;
        SubjectType = subjectType;
        ExternalId = externalId;
        DisplayName = displayName;
        DateOfBirth = dateOfBirth;
        Sex = sex;
    }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public void Update(
        string firstName,
        string lastName,
        SubjectType subjectType,
        string? externalId,
        string? displayName,
        DateTimeOffset? dateOfBirth,
        string? sex)
    {
        FirstName = firstName;
        LastName = lastName;
        SubjectType = subjectType;
        ExternalId = externalId;
        DisplayName = displayName;
        DateOfBirth = dateOfBirth;
        Sex = sex;
    }

    public void Archive()
    {
        Status = StatusArchived;
    }

    public void Restore()
    {
        Status = StatusActive;
    }
}