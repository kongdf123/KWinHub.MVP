using KWinHub.Domain.Sessions;

namespace KWinHub.Application.Sessions;

public interface ISessionRepository
{
    Task<MeasurementSession?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MeasurementSession>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MeasurementSession>> ListBySubjectAsync(Guid organizationId, Guid subjectId, CancellationToken cancellationToken = default);
    Task AddAsync(MeasurementSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(MeasurementSession session, CancellationToken cancellationToken = default);
    Task<MeasurementSession?> GetByExternalIdAsync(Guid organizationId, string externalId, CancellationToken cancellationToken = default);
}
