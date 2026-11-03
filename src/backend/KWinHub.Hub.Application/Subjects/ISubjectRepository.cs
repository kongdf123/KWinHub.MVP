using KWinHub.Hub.Subjects;

namespace KWinHub.Hub.Application.Subjects;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subject>> ListByOrganizationAsync(
        Guid organizationId,
        string? search = null,
        string? status = null,
        string? subjectType = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Subject subject, CancellationToken cancellationToken = default);

    Task UpdateAsync(Subject subject, CancellationToken cancellationToken = default);
}