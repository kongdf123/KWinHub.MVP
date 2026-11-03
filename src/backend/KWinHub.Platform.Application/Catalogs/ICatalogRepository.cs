using KWinHub.Domain.Catalogs;

namespace KWinHub.Application.Catalogs;

public interface ICatalogRepository
{
    Task<IReadOnlyList<GlobalProtocol>> ListProtocolsAsync(CancellationToken cancellationToken = default);

    Task<GlobalProtocol?> GetProtocolAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddProtocolAsync(GlobalProtocol protocol, CancellationToken cancellationToken = default);

    void RemoveProtocol(GlobalProtocol protocol);

    Task<IReadOnlyList<GlobalMetric>> ListMetricsAsync(CancellationToken cancellationToken = default);

    Task<GlobalMetric?> GetMetricAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddMetricAsync(GlobalMetric metric, CancellationToken cancellationToken = default);

    void RemoveMetric(GlobalMetric metric);

    Task<IReadOnlyList<DeviceModel>> ListDevicesAsync(CancellationToken cancellationToken = default);

    Task<DeviceModel?> GetDeviceAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddDeviceAsync(DeviceModel device, CancellationToken cancellationToken = default);

    void RemoveDevice(DeviceModel device);

    Task<IReadOnlyList<AlgorithmVersion>> ListAlgorithmsAsync(CancellationToken cancellationToken = default);

    Task<AlgorithmVersion?> GetAlgorithmAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAlgorithmAsync(AlgorithmVersion algorithm, CancellationToken cancellationToken = default);

    void RemoveAlgorithm(AlgorithmVersion algorithm);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
