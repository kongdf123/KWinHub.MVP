using KWinHub.Hub.Application.Sessions;
using KWinHub.Hub.Infrastructure.Persistence;
using KWinHub.Hub.Sessions;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class IngestionOperationRepository : IIngestionOperationRepository
{
    private readonly HubDbContext _dbContext;

    public IngestionOperationRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(IngestionOperation operation, CancellationToken cancellationToken = default)
    {
        await _dbContext.IngestionOperations.AddAsync(operation, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IngestionOperation?> GetByClientOperationIdAsync(
        Guid organizationId,
        string clientOperationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IngestionOperations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                o => o.OrganizationId == organizationId && o.ClientOperationId == clientOperationId,
                cancellationToken);
    }

    public async Task<IngestionOperation?> GetBySessionIdAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.IngestionOperations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                o => o.OrganizationId == organizationId && o.SessionId == sessionId,
                cancellationToken);
    }

    public async Task UpdateAsync(IngestionOperation operation, CancellationToken cancellationToken = default)
    {
        _dbContext.IngestionOperations.Update(operation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}