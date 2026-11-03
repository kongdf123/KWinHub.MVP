using KWinHub.Hub.Contracts.Subjects;

namespace KWinHub.Hub.Application.Subjects;

public interface ISubjectService
{
    Task<SubjectResponse> CreateAsync(
        Guid organizationId,
        CreateSubjectRequest request,
        CancellationToken cancellationToken = default);

    Task<SubjectResponse?> UpdateAsync(
        Guid organizationId,
        Guid id,
        UpdateSubjectRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubjectResponse>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        string? subjectType = null,
        CancellationToken cancellationToken = default);

    Task<SubjectResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ArchiveAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> RestoreAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default);
}