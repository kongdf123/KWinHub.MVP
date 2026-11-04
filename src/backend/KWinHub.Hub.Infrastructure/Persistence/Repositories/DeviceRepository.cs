using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Sessions;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly HubDbContext _dbContext;

    public DeviceRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Device?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Devices
            .AsNoTracking()
            .SingleOrDefaultAsync(
                d => d.Id == id && d.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Device>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Devices
            .AsNoTracking()
            .Where(d => d.OrganizationId == organizationId)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Device device, CancellationToken cancellationToken = default)
    {
        await _dbContext.Devices.AddAsync(device, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Device device, CancellationToken cancellationToken = default)
    {
        _dbContext.Devices.Update(device);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}