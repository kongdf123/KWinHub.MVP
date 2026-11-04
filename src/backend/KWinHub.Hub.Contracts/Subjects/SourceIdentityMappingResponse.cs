namespace KWinHub.Hub.Contracts.Subjects;

public record SourceIdentityMappingResponse(
    Guid Id,
    Guid OrganizationId,
    string SourceApp,
    string InstallationScope,
    string ExternalId,
    Guid SubjectId,
    string Status,
    string? ConflictReason,
    Guid? ResolvedBy,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);