namespace KWinHub.Contracts.Subjects;

public record SubjectResponse(
    Guid Id,
    Guid OrganizationId,
    string FirstName,
    string LastName,
    string SubjectType,
    string? ExternalId,
    string? DisplayName,
    DateTimeOffset? DateOfBirth,
    string? Sex,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);