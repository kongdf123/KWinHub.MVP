using KWinHub.Application.Protocols;
using KWinHub.Domain.Protocols;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence.Repositories;

public class ProtocolRepository : IProtocolRepository
{
    private readonly KWinHubDbContext _dbContext;

    public ProtocolRepository(KWinHubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Protocol?> GetByIdAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Protocols
            .AsNoTracking()
            .SingleOrDefaultAsync(
                p => p.Id == id && p.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Protocol>> ListByOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Protocols
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Protocol protocol, CancellationToken cancellationToken = default)
    {
        await _dbContext.Protocols.AddAsync(protocol, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Protocol protocol, CancellationToken cancellationToken = default)
    {
        _dbContext.Protocols.Update(protocol);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Protocol?> GetByCodeAndVersionAsync(
        Guid organizationId,
        string code,
        string version,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Protocols
            .AsNoTracking()
            .SingleOrDefaultAsync(
                p => p.OrganizationId == organizationId
                     && p.Code == code
                     && p.Version == version,
                cancellationToken);
    }
}
