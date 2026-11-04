using KWinHub.Hub.Subjects;

namespace KWinHub.Hub.Application.Subjects;

public interface ISourceIdentityMappingRepository
{
    Task<SourceIdentityMapping?> GetBySourceIdentityAsync(
        Guid organizationId,
        string sourceApp,
        string installationScope,
        string externalId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SourceIdentityMapping>> ListByOrganizationAsync(
        Guid organizationId,
        string? status = null,
        string? sourceApp = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SourceIdentityMapping>> ListBySubjectAsync(
        Guid organizationId,
        Guid subjectId,
        CancellationToken cancellationToken = default);

    Task AddAsync(SourceIdentityMapping mapping, CancellationToken cancellationToken = default);

    Task UpdateAsync(SourceIdentityMapping mapping, CancellationToken cancellationToken = default);
}