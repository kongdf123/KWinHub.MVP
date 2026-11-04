using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public interface IIngestionOperationRepository
{
    Task AddAsync(IngestionOperation operation, CancellationToken cancellationToken = default);

    Task<IngestionOperation?> GetByClientOperationIdAsync(
        Guid organizationId,
        string clientOperationId,
        CancellationToken cancellationToken = default);

    Task<IngestionOperation?> GetBySessionIdAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(IngestionOperation operation, CancellationToken cancellationToken = default);
}