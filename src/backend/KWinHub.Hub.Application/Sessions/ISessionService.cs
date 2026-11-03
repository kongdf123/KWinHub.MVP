using KWinHub.Contracts.Sessions;

namespace KWinHub.Application.Sessions;

public interface ISessionService
{
    Task<IReadOnlyList<SessionResponse>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SessionResponse>> ListBySubjectAsync(Guid organizationId, Guid subjectId, CancellationToken cancellationToken = default);
    Task<SessionResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<SessionStatusResponse?> GetStatusAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<SessionResponse> CreateAsync(Guid organizationId, CreateSessionRequest request, CancellationToken cancellationToken = default);
    Task<SessionResponse> GetOrCreateByExternalIdAsync(Guid organizationId, CreateSessionRequest request, CancellationToken cancellationToken = default);
}
