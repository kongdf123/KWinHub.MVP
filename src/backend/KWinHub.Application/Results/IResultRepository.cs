using KWinHub.Domain.Results;

namespace KWinHub.Application.Results;

public interface IResultRepository
{
    Task<Result?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<Result?> GetBySessionAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> ListBySessionAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Result>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task AddAsync(Result result, CancellationToken cancellationToken = default);
    Task UpdateAsync(Result result, CancellationToken cancellationToken = default);
}
