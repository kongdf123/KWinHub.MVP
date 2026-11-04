namespace KWinHub.Hub.Assets;

public interface IAssetRepository
{
    Task<DataAsset?> GetByIdAsync(Guid organizationId, Guid assetId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataAsset>> GetBySessionAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default);

    Task<DataAsset?> GetByExternalIdAsync(Guid organizationId, Guid sessionId, string externalId, CancellationToken cancellationToken = default);

    Task AddAsync(DataAsset asset, CancellationToken cancellationToken = default);

    Task UpdateAsync(DataAsset asset, CancellationToken cancellationToken = default);
}