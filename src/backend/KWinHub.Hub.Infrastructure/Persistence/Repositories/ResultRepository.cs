using KWinHub.Application.Results;
using KWinHub.Domain.Results;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class ResultRepository : IResultRepository
{
    private readonly KWinHubDbContext _dbContext;

    public ResultRepository(KWinHubDbContext dbContext)
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
        return await _dbContext.Results
            .AsNoTracking()
            .Include(r => r.Metrics)
            .Where(r => r.SessionId == sessionId && r.OrganizationId == organizationId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Result>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Results
            .AsNoTracking()
            .Include(r => r.Metrics)
            .Where(r => r.OrganizationId == organizationId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
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
