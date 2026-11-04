using KWinHub.Hub.Contracts.Sessions;
using KWinHub.Hub.Sessions;

namespace KWinHub.Hub.Application.Sessions;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository;

    public DeviceService(IDeviceRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeviceResponse> CreateAsync(
        Guid organizationId,
        CreateDeviceRequest request,
        CancellationToken cancellationToken = default)
    {
        var device = new Device(
            organizationId: organizationId,
            name: request.Name,
            model: request.Model,
            manufacturer: request.Manufacturer,
            serialNumber: request.SerialNumber,
            firmwareVersion: request.FirmwareVersion);

        await _repository.AddAsync(device, cancellationToken);
        return ToResponse(device);
    }

    public async Task<DeviceResponse?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var device = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        return device is null ? null : ToResponse(device);
    }

    public async Task<IReadOnlyList<DeviceResponse>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var devices = await _repository.ListByOrganizationAsync(organizationId, cancellationToken);
        return devices.Select(ToResponse).ToList();
    }

    public async Task<DeviceResponse?> DeactivateAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var device = await _repository.GetByIdAsync(organizationId, id, cancellationToken);
        if (device is null) return null;

        device.Deactivate();
        await _repository.UpdateAsync(device, cancellationToken);
        return ToResponse(device);
    }

    private static DeviceResponse ToResponse(Device device) =>
        new(
            Id: device.Id,
            OrganizationId: device.OrganizationId,
            Name: device.Name,
            Model: device.Model,
            Manufacturer: device.Manufacturer,
            SerialNumber: device.SerialNumber,
            FirmwareVersion: device.FirmwareVersion,
            Status: device.Status,
            CreatedAt: device.CreatedAt,
            UpdatedAt: device.UpdatedAt);
}