using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Device>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task AddAsync(Device device, CancellationToken cancellationToken = default);
    Task UpdateAsync(Device device, CancellationToken cancellationToken = default);
}