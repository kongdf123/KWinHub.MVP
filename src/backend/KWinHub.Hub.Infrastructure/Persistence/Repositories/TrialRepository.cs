using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Sessions;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class TrialRepository : ITrialRepository
{
    private readonly HubDbContext _dbContext;

    public TrialRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Trial?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Trials
            .AsNoTracking()
            .SingleOrDefaultAsync(
                t => t.Id == id && t.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Trial>> ListBySessionAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Trials
            .AsNoTracking()
            .Where(t => t.OrganizationId == organizationId && t.SessionId == sessionId)
            .OrderBy(t => t.TrialIndex)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Trial trial, CancellationToken cancellationToken = default)
    {
        await _dbContext.Trials.AddAsync(trial, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Trial trial, CancellationToken cancellationToken = default)
    {
        _dbContext.Trials.Update(trial);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Trial?> GetByExternalIdAsync(
        Guid organizationId,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Trials
            .AsNoTracking()
            .SingleOrDefaultAsync(
                t => t.OrganizationId == organizationId && t.ExternalId == externalId,
                cancellationToken);
    }
}