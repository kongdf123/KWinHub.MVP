using KWinHub.Contracts.Platform;

namespace KWinHub.Application.Catalogs;

public interface ICatalogService
{
    Task<IReadOnlyList<GlobalProtocolResponse>> ListProtocolsAsync(CancellationToken cancellationToken = default);

    Task<GlobalProtocolResponse> CreateProtocolAsync(
        UpsertGlobalProtocolRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<GlobalProtocolResponse?> UpdateProtocolAsync(
        Guid id,
        UpsertGlobalProtocolRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> ChangeProtocolStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteProtocolAsync(
        Guid id,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GlobalMetricResponse>> ListMetricsAsync(CancellationToken cancellationToken = default);

    Task<GlobalMetricResponse> CreateMetricAsync(
        UpsertGlobalMetricRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<GlobalMetricResponse?> UpdateMetricAsync(
        Guid id,
        UpsertGlobalMetricRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> ChangeMetricStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteMetricAsync(
        Guid id,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeviceModelResponse>> ListDevicesAsync(CancellationToken cancellationToken = default);

    Task<DeviceModelResponse> CreateDeviceAsync(
        UpsertDeviceModelRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<DeviceModelResponse?> UpdateDeviceAsync(
        Guid id,
        UpsertDeviceModelRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> ChangeDeviceStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteDeviceAsync(
        Guid id,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AlgorithmVersionResponse>> ListAlgorithmsAsync(CancellationToken cancellationToken = default);

    Task<AlgorithmVersionResponse> CreateAlgorithmAsync(
        UpsertAlgorithmVersionRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AlgorithmVersionResponse?> UpdateAlgorithmAsync(
        Guid id,
        UpsertAlgorithmVersionRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> ChangeAlgorithmStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAlgorithmAsync(
        Guid id,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
