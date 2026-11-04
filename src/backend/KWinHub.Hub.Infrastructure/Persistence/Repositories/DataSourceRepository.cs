using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Sessions;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class DataSourceRepository : IDataSourceRepository
{
    private readonly HubDbContext _dbContext;

    public DataSourceRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataSource?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DataSources
            .AsNoTracking()
            .SingleOrDefaultAsync(
                d => d.Id == id && d.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<DataSource?> GetByCodeAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DataSources
            .AsNoTracking()
            .SingleOrDefaultAsync(
                d => d.OrganizationId == organizationId && d.Code == code,
                cancellationToken);
    }

    public async Task<IReadOnlyList<DataSource>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DataSources
            .AsNoTracking()
            .Where(d => d.OrganizationId == organizationId)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(DataSource source, CancellationToken cancellationToken = default)
    {
        await _dbContext.DataSources.AddAsync(source, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DataSource source, CancellationToken cancellationToken = default)
    {
        _dbContext.DataSources.Update(source);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}