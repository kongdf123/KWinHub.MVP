using KWinHub.Hub.Application.Results;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Results;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class ResultRepository : IResultRepository
{
    private readonly HubDbContext _dbContext;

    public ResultRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Results
            .AsNoTracking()
            .Include(r => r.Metrics)
            .SingleOrDefaultAsync(
                r => r.Id == id && r.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<Result?> GetBySessionAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Results
            .AsNoTracking()
            .Include(r => r.Metrics)
            .SingleOrDefaultAsync(
                r => r.SessionId == sessionId && r.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Result>> ListBySessionAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var results = await _dbContext.Results
            .AsNoTracking()
            .Include(r => r.Metrics)
            .Where(r => r.SessionId == sessionId && r.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        return results.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task<IReadOnlyList<Result>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var results = await _dbContext.Results
            .AsNoTracking()
            .Include(r => r.Metrics)
            .Where(r => r.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        return results.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task<IReadOnlyList<Result>> ListBySessionsAsync(
        Guid organizationId,
        IEnumerable<Guid> sessionIds,
        CancellationToken cancellationToken = default)
    {
        var sessionIdList = sessionIds.ToList();
        if (sessionIdList.Count == 0)
        {
            return Array.Empty<Result>();
        }

        var results = await _dbContext.Results
            .AsNoTracking()
            .Include(r => r.Metrics)
            .Where(r => r.OrganizationId == organizationId && sessionIdList.Contains(r.SessionId))
            .ToListAsync(cancellationToken);

        return results.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task AddAsync(Result result, CancellationToken cancellationToken = default)
    {
        await _dbContext.Results.AddAsync(result, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Result result, CancellationToken cancellationToken = default)
    {
        _dbContext.Results.Update(result);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}