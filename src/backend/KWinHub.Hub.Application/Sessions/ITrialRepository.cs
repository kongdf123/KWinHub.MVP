using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public interface ITrialRepository
{
    Task<Trial?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Trial>> ListBySessionAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(Trial trial, CancellationToken cancellationToken = default);
    Task UpdateAsync(Trial trial, CancellationToken cancellationToken = default);
    Task<Trial?> GetByExternalIdAsync(Guid organizationId, string externalId, CancellationToken cancellationToken = default);
}