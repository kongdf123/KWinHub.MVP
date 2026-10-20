using KWinHub.Domain.Subjects;

namespace KWinHub.Application.Subjects;

public interface ISubjectRepository
{
    Task<Subject?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Subject>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Subject subject, CancellationToken cancellationToken = default);

    Task UpdateAsync(Subject subject, CancellationToken cancellationToken = default);
}