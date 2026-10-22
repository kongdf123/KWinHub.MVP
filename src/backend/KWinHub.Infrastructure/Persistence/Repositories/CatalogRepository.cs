using KWinHub.Application.Catalogs;
using KWinHub.Domain.Catalogs;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class CatalogRepository : ICatalogRepository
{
    private readonly KWinHubDbContext _dbContext;

    public CatalogRepository(KWinHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<GlobalProtocol>> ListProtocolsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GlobalProtocols
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<GlobalProtocol?> GetProtocolAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.GlobalProtocols
            .SingleOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddProtocolAsync(
        GlobalProtocol protocol,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.GlobalProtocols.AddAsync(protocol, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void RemoveProtocol(GlobalProtocol protocol)
    {
        _dbContext.GlobalProtocols.Remove(protocol);
    }

    public async Task<IReadOnlyList<GlobalMetric>> ListMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.GlobalMetrics
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<GlobalMetric?> GetMetricAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.GlobalMetrics
            .SingleOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task AddMetricAsync(GlobalMetric metric, CancellationToken cancellationToken = default)
    {
        await _dbContext.GlobalMetrics.AddAsync(metric, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void RemoveMetric(GlobalMetric metric)
    {
        _dbContext.GlobalMetrics.Remove(metric);
    }

    public async Task<IReadOnlyList<DeviceModel>> ListDevicesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeviceModels
            .AsNoTracking()
            .OrderBy(d => d.Manufacturer)
            .ThenBy(d => d.Model)
            .ToListAsync(cancellationToken);
    }

    public async Task<DeviceModel?> GetDeviceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DeviceModels
            .SingleOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task AddDeviceAsync(DeviceModel device, CancellationToken cancellationToken = default)
    {
        await _dbContext.DeviceModels.AddAsync(device, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void RemoveDevice(DeviceModel device)
    {
        _dbContext.DeviceModels.Remove(device);
    }

    public async Task<IReadOnlyList<AlgorithmVersion>> ListAlgorithmsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AlgorithmVersions
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<AlgorithmVersion?> GetAlgorithmAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.AlgorithmVersions
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task AddAlgorithmAsync(
        AlgorithmVersion algorithm,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.AlgorithmVersions.AddAsync(algorithm, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public void RemoveAlgorithm(AlgorithmVersion algorithm)
    {
        _dbContext.AlgorithmVersions.Remove(algorithm);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
