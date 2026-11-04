using KWinHub.Hub.Contracts.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public interface ITrialService
{
    Task<TrialResponse> CreateAsync(Guid organizationId, CreateTrialRequest request, CancellationToken cancellationToken = default);
    Task<TrialResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrialResponse>> ListBySessionAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default);
    Task<TrialResponse?> CompleteAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<TrialResponse?> FailAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
}