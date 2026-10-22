using KWinHub.Domain.Protocols;

namespace KWinHub.Application.Protocols;

public interface IProtocolRepository
{
    Task<Protocol?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Protocol>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task AddAsync(Protocol protocol, CancellationToken cancellationToken = default);
    Task UpdateAsync(Protocol protocol, CancellationToken cancellationToken = default);
    Task<Protocol?> GetByCodeAndVersionAsync(Guid organizationId, string code, string version, CancellationToken cancellationToken = default);
}
