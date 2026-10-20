using System.ComponentModel.DataAnnotations;

namespace KWinHub.Contracts.Subjects;

public record CreateSubjectRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string SubjectType,
    string? ExternalId = null,
    string? DisplayName = null,
    DateTimeOffset? DateOfBirth = null,
    string? Sex = null);