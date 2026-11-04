using KWinHub.Hub.Contracts.Subjects;

namespace KWinHub.Hub.Application.Subjects;

public interface ISourceIdentityMappingService
{
    Task<SourceIdentityMappingResponse> RegisterAsync(
        Guid organizationId,
        CreateSourceIdentityMappingRequest request,
        CancellationToken cancellationToken = default);

    Task<SourceIdentityMappingResponse?> ResolveConflictAsync(
        Guid organizationId,
        Guid mappingId,
        ResolveSourceIdentityConflictRequest request,
        Guid resolvedBy,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SourceIdentityMappingResponse>> ListAsync(
        Guid organizationId,
        string? status = null,
        string? sourceApp = null,
        CancellationToken cancellationToken = default);

    Task<SourceIdentityMappingResponse?> GetBySourceIdentityAsync(
        Guid organizationId,
        string sourceApp,
        string installationScope,
        string externalId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SourceIdentityMappingResponse>> ListBySubjectAsync(
        Guid organizationId,
        Guid subjectId,
        CancellationToken cancellationToken = default);
}