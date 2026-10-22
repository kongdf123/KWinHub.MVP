using KWinHub.Application.Platform;
using KWinHub.Contracts.Platform;
using KWinHub.Domain.Catalogs;

namespace KWinHub.Application.Catalogs;

public class CatalogService : ICatalogService
{
    private const string ProtocolEntityType = "GlobalProtocol";
    private const string MetricEntityType = "GlobalMetric";
    private const string DeviceEntityType = "DeviceModel";
    private const string AlgorithmEntityType = "AlgorithmVersion";

    private readonly ICatalogRepository _repository;
    private readonly IPlatformService _platformService;

    public CatalogService(ICatalogRepository repository, IPlatformService platformService)
    {
        _repository = repository;
        _platformService = platformService;
    }

    public async Task<IReadOnlyList<GlobalProtocolResponse>> ListProtocolsAsync(
        CancellationToken cancellationToken = default)
    {
        var protocols = await _repository.ListProtocolsAsync(cancellationToken);
        return protocols.Select(ToResponse).ToList();
    }

    public async Task<GlobalProtocolResponse> CreateProtocolAsync(
        UpsertGlobalProtocolRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var protocol = new GlobalProtocol(
            request.Code.Trim(),
            request.Name.Trim(),
            request.Version.Trim(),
            request.Category,
            request.Sport,
            request.Description);

        await _repository.AddProtocolAsync(protocol, cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.protocol.created",
            ProtocolEntityType,
            protocol.Id.ToString(),
            $"Protocol '{protocol.Code}' v{protocol.Version} created.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return ToResponse(protocol);
    }

    public async Task<GlobalProtocolResponse?> UpdateProtocolAsync(
        Guid id,
        UpsertGlobalProtocolRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var protocol = await _repository.GetProtocolAsync(id, cancellationToken);
        if (protocol is null)
        {
            return null;
        }

        protocol.Update(
            request.Name.Trim(),
            request.Version.Trim(),
            request.Category,
            request.Sport,
            request.Description);

        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.protocol.updated",
            ProtocolEntityType,
            protocol.Id.ToString(),
            $"Protocol '{protocol.Code}' v{protocol.Version} updated.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return ToResponse(protocol);
    }

    public async Task<bool> ChangeProtocolStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedAction = NormalizeCatalogAction(action);

        var protocol = await _repository.GetProtocolAsync(id, cancellationToken);
        if (protocol is null)
        {
            return false;
        }

        if (normalizedAction == "publish")
        {
            protocol.Publish();
        }
        else
        {
            protocol.Archive();
        }

        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            normalizedAction == "publish" ? "catalog.protocol.published" : "catalog.protocol.archived",
            ProtocolEntityType,
            protocol.Id.ToString(),
            $"Protocol '{protocol.Code}' v{protocol.Version} {(normalizedAction == "publish" ? "published" : "archived")}.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteProtocolAsync(
        Guid id,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var protocol = await _repository.GetProtocolAsync(id, cancellationToken);
        if (protocol is null)
        {
            return false;
        }

        _repository.RemoveProtocol(protocol);
        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.protocol.deleted",
            ProtocolEntityType,
            protocol.Id.ToString(),
            $"Protocol '{protocol.Code}' v{protocol.Version} deleted.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<GlobalMetricResponse>> ListMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        var metrics = await _repository.ListMetricsAsync(cancellationToken);
        return metrics.Select(ToResponse).ToList();
    }

    public async Task<GlobalMetricResponse> CreateMetricAsync(
        UpsertGlobalMetricRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var metric = new GlobalMetric(
            request.Code.Trim(),
            request.Name.Trim(),
            request.Unit.Trim(),
            request.Category,
            request.Description);

        await _repository.AddMetricAsync(metric, cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.metric.created",
            MetricEntityType,
            metric.Id.ToString(),
            $"Metric '{metric.Code}' created.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return ToResponse(metric);
    }

    public async Task<GlobalMetricResponse?> UpdateMetricAsync(
        Guid id,
        UpsertGlobalMetricRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var metric = await _repository.GetMetricAsync(id, cancellationToken);
        if (metric is null)
        {
            return null;
        }

        metric.Update(request.Name.Trim(), request.Unit.Trim(), request.Category, request.Description);
        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.metric.updated",
            MetricEntityType,
            metric.Id.ToString(),
            $"Metric '{metric.Code}' updated.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return ToResponse(metric);
    }

    public async Task<bool> ChangeMetricStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedAction = NormalizeCatalogAction(action);

        var metric = await _repository.GetMetricAsync(id, cancellationToken);
        if (metric is null)
        {
            return false;
        }

        if (normalizedAction == "publish")
        {
            metric.Publish();
        }
        else
        {
            metric.Archive();
        }

        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            normalizedAction == "publish" ? "catalog.metric.published" : "catalog.metric.archived",
            MetricEntityType,
            metric.Id.ToString(),
            $"Metric '{metric.Code}' {(normalizedAction == "publish" ? "published" : "archived")}.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteMetricAsync(
        Guid id,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var metric = await _repository.GetMetricAsync(id, cancellationToken);
        if (metric is null)
        {
            return false;
        }

        _repository.RemoveMetric(metric);
        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.metric.deleted",
            MetricEntityType,
            metric.Id.ToString(),
            $"Metric '{metric.Code}' deleted.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<DeviceModelResponse>> ListDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        var devices = await _repository.ListDevicesAsync(cancellationToken);
        return devices.Select(ToResponse).ToList();
    }

    public async Task<DeviceModelResponse> CreateDeviceAsync(
        UpsertDeviceModelRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var device = new DeviceModel(
            request.Manufacturer.Trim(),
            request.Model.Trim(),
            request.DeviceType.Trim(),
            request.Notes);

        await _repository.AddDeviceAsync(device, cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.device.created",
            DeviceEntityType,
            device.Id.ToString(),
            $"Device model '{device.Manufacturer} {device.Model}' created.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return ToResponse(device);
    }

    public async Task<DeviceModelResponse?> UpdateDeviceAsync(
        Guid id,
        UpsertDeviceModelRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var device = await _repository.GetDeviceAsync(id, cancellationToken);
        if (device is null)
        {
            return null;
        }

        device.Update(
            request.Manufacturer.Trim(),
            request.Model.Trim(),
            request.DeviceType.Trim(),
            request.Notes);

        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.device.updated",
            DeviceEntityType,
            device.Id.ToString(),
            $"Device model '{device.Manufacturer} {device.Model}' updated.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return ToResponse(device);
    }

    public async Task<bool> ChangeDeviceStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedAction = NormalizeCatalogAction(action);

        var device = await _repository.GetDeviceAsync(id, cancellationToken);
        if (device is null)
        {
            return false;
        }

        if (normalizedAction == "publish")
        {
            device.Publish();
        }
        else
        {
            device.Archive();
        }

        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            normalizedAction == "publish" ? "catalog.device.published" : "catalog.device.archived",
            DeviceEntityType,
            device.Id.ToString(),
            $"Device model '{device.Manufacturer} {device.Model}' {(normalizedAction == "publish" ? "published" : "archived")}.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteDeviceAsync(
        Guid id,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var device = await _repository.GetDeviceAsync(id, cancellationToken);
        if (device is null)
        {
            return false;
        }

        _repository.RemoveDevice(device);
        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.device.deleted",
            DeviceEntityType,
            device.Id.ToString(),
            $"Device model '{device.Manufacturer} {device.Model}' deleted.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<AlgorithmVersionResponse>> ListAlgorithmsAsync(
        CancellationToken cancellationToken = default)
    {
        var algorithms = await _repository.ListAlgorithmsAsync(cancellationToken);
        return algorithms.Select(ToResponse).ToList();
    }

    public async Task<AlgorithmVersionResponse> CreateAlgorithmAsync(
        UpsertAlgorithmVersionRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var algorithm = new AlgorithmVersion(
            request.Name.Trim(),
            request.Version.Trim(),
            request.SupportedMetricCodes,
            request.Notes);

        await _repository.AddAlgorithmAsync(algorithm, cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.algorithm.created",
            AlgorithmEntityType,
            algorithm.Id.ToString(),
            $"Algorithm '{algorithm.Name}' v{algorithm.Version} created.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return ToResponse(algorithm);
    }

    public async Task<AlgorithmVersionResponse?> UpdateAlgorithmAsync(
        Guid id,
        UpsertAlgorithmVersionRequest request,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var algorithm = await _repository.GetAlgorithmAsync(id, cancellationToken);
        if (algorithm is null)
        {
            return null;
        }

        algorithm.Update(
            request.Name.Trim(),
            request.Version.Trim(),
            request.SupportedMetricCodes,
            request.Notes);

        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.algorithm.updated",
            AlgorithmEntityType,
            algorithm.Id.ToString(),
            $"Algorithm '{algorithm.Name}' v{algorithm.Version} updated.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return ToResponse(algorithm);
    }

    public async Task<bool> ChangeAlgorithmStatusAsync(
        Guid id,
        string action,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var normalizedAction = NormalizeCatalogAction(action);

        var algorithm = await _repository.GetAlgorithmAsync(id, cancellationToken);
        if (algorithm is null)
        {
            return false;
        }

        if (normalizedAction == "publish")
        {
            algorithm.Publish();
        }
        else
        {
            algorithm.Archive();
        }

        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            normalizedAction == "publish" ? "catalog.algorithm.published" : "catalog.algorithm.archived",
            AlgorithmEntityType,
            algorithm.Id.ToString(),
            $"Algorithm '{algorithm.Name}' v{algorithm.Version} {(normalizedAction == "publish" ? "published" : "archived")}.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAlgorithmAsync(
        Guid id,
        Guid actorUserId,
        string? actorEmail,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var algorithm = await _repository.GetAlgorithmAsync(id, cancellationToken);
        if (algorithm is null)
        {
            return false;
        }

        _repository.RemoveAlgorithm(algorithm);
        await _repository.SaveChangesAsync(cancellationToken);

        await _platformService.RecordAuditAsync(
            "catalog.algorithm.deleted",
            AlgorithmEntityType,
            algorithm.Id.ToString(),
            $"Algorithm '{algorithm.Name}' v{algorithm.Version} deleted.",
            actorUserId,
            actorEmail,
            ipAddress,
            cancellationToken);

        return true;
    }

    private static string NormalizeCatalogAction(string action)
    {
        var normalized = (action ?? string.Empty).Trim().ToLowerInvariant();
        if (normalized is not ("publish" or "archive"))
        {
            throw new ArgumentException("Action must be 'publish' or 'archive'.", nameof(action));
        }

        return normalized;
    }

    private static GlobalProtocolResponse ToResponse(GlobalProtocol protocol) =>
        new(
            Id: protocol.Id,
            Code: protocol.Code,
            Name: protocol.Name,
            Version: protocol.Version,
            Category: protocol.Category,
            Sport: protocol.Sport,
            Description: protocol.Description,
            Status: protocol.Status.ToString(),
            CreatedAt: protocol.CreatedAt);

    private static GlobalMetricResponse ToResponse(GlobalMetric metric) =>
        new(
            Id: metric.Id,
            Code: metric.Code,
            Name: metric.Name,
            Unit: metric.Unit,
            Category: metric.Category,
            Description: metric.Description,
            Status: metric.Status.ToString(),
            CreatedAt: metric.CreatedAt);

    private static DeviceModelResponse ToResponse(DeviceModel device) =>
        new(
            Id: device.Id,
            Manufacturer: device.Manufacturer,
            Model: device.Model,
            DeviceType: device.DeviceType,
            Notes: device.Notes,
            Status: device.Status.ToString(),
            CreatedAt: device.CreatedAt);

    private static AlgorithmVersionResponse ToResponse(AlgorithmVersion algorithm) =>
        new(
            Id: algorithm.Id,
            Name: algorithm.Name,
            Version: algorithm.Version,
            SupportedMetricCodes: algorithm.SupportedMetricCodes,
            Notes: algorithm.Notes,
            Status: algorithm.Status.ToString(),
            CreatedAt: algorithm.CreatedAt);
}
