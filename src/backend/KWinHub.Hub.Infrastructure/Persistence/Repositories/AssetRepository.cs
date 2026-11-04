using KWinHub.Hub.Assets;
using KWinHub.Hub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence.Repositories;

public class AssetRepository : IAssetRepository
{
    private readonly HubDbContext _dbContext;

    public AssetRepository(HubDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataAsset?> GetByIdAsync(
        Guid organizationId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DataAssets
            .AsNoTracking()
            .SingleOrDefaultAsync(
                a => a.Id == assetId && a.OrganizationId == organizationId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<DataAsset>> GetBySessionAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DataAssets
            .AsNoTracking()
            .Where(a => a.OrganizationId == organizationId && a.SessionId == sessionId)
            .OrderBy(a => a.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<DataAsset?> GetByExternalIdAsync(
        Guid organizationId,
        Guid sessionId,
        string externalId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DataAssets
            .AsNoTracking()
            .SingleOrDefaultAsync(
                a => a.OrganizationId == organizationId &&
                     a.SessionId == sessionId &&
                     a.ExternalId == externalId,
                cancellationToken);
    }

    public async Task AddAsync(DataAsset asset, CancellationToken cancellationToken = default)
    {
        await _dbContext.DataAssets.AddAsync(asset, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DataAsset asset, CancellationToken cancellationToken = default)
    {
        _dbContext.DataAssets.Update(asset);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}