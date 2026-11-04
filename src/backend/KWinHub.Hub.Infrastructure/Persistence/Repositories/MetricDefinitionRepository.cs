using KWinHub.Hub.Application.Protocols;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Protocols;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class MetricDefinitionRepository : IMetricDefinitionRepository
{
    private readonly HubDbContext _dbContext;

    public MetricDefinitionRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MetricDefinition?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MetricDefinitions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                m => m.Id == id && m.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<MetricDefinition?> GetByCodeAndVersionAsync(
        Guid organizationId,
        string code,
        string version,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MetricDefinitions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                m => m.OrganizationId == organizationId && m.Code == code && m.Version == version,
                cancellationToken);
    }

    public async Task<MetricDefinition?> GetByCodeAsync(
        Guid organizationId,
        string code,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MetricDefinitions
            .AsNoTracking()
            .Where(m => m.OrganizationId == organizationId && m.Code == code && m.Status == "Active")
            .OrderByDescending(m => m.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MetricDefinition>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.MetricDefinitions
            .AsNoTracking()
            .Where(m => m.OrganizationId == organizationId)
            .OrderBy(m => m.Code)
            .ThenBy(m => m.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MetricDefinition definition, CancellationToken cancellationToken = default)
    {
        await _dbContext.MetricDefinitions.AddAsync(definition, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(MetricDefinition definition, CancellationToken cancellationToken = default)
    {
        _dbContext.MetricDefinitions.Update(definition);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}