namespace KWinHub.Hub.Contracts.Subjects;

public record CreateSourceIdentityMappingRequest(
    string SourceApp,
    string InstallationScope,
    string ExternalId,
    Guid SubjectId);