using KWinHub.Contracts.Protocols;

namespace KWinHub.Application.Protocols;

public interface IProtocolService
{
    Task<IReadOnlyList<ProtocolResponse>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<ProtocolResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<ProtocolResponse?> GetByCodeAndVersionAsync(Guid organizationId, string code, string version, CancellationToken cancellationToken = default);
    Task<ProtocolResponse> CreateAsync(Guid organizationId, CreateProtocolRequest request, CancellationToken cancellationToken = default);
}
