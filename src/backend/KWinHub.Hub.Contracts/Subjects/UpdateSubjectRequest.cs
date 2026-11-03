using System.ComponentModel.DataAnnotations;

namespace KWinHub.Hub.Contracts.Subjects;

public record UpdateSubjectRequest(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] string SubjectType,
    string? ExternalId = null,
    string? DisplayName = null,
    DateTimeOffset? DateOfBirth = null,
    string? Sex = null);