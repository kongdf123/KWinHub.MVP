using KWinHub.Hub.Contracts.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public interface IDeviceService
{
    Task<DeviceResponse> CreateAsync(Guid organizationId, CreateDeviceRequest request, CancellationToken cancellationToken = default);
    Task<DeviceResponse?> GetByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeviceResponse>> ListByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<DeviceResponse?> DeactivateAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
}