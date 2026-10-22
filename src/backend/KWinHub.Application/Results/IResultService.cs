using KWinHub.Contracts.Results;

namespace KWinHub.Application.Results;

public interface IResultService
{
    Task<ResultResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<ResultResponse?> GetBySessionAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResultResponse>> ListBySessionAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResultResponse>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
}
