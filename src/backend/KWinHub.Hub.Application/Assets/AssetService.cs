using KWinHub.Hub.Application.Common;

namespace KWinHub.Hub.Assets;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;
    private readonly IObjectStorage _objectStorage;
    private readonly ITransactionManager _transactionManager;
    private readonly ObjectStorageOptions _storageOptions;

    public AssetService(
        IAssetRepository assetRepository,
        IObjectStorage objectStorage,
        ITransactionManager transactionManager,
        ObjectStorageOptions storageOptions)
    {
        _assetRepository = assetRepository;
        _objectStorage = objectStorage;
        _transactionManager = transactionManager;
        _storageOptions = storageOptions;
    }

    public async Task<IReadOnlyList<AssetResponse>> ListBySessionAsync(
        Guid organizationId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var assets = await _assetRepository.GetBySessionAsync(organizationId, sessionId, cancellationToken);
        return assets.Select(ToResponse).ToList();
    }

    public async Task<AssetUploadGrantResponse> RequestUploadGrantAsync(
        Guid organizationId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdAsync(organizationId, assetId, cancellationToken)
            ?? throw new AssetNotFoundException(assetId);

        if (asset.Status == DataAssetStatus.UploadedVerified)
        {
            throw new AssetAlreadyUploadedException(assetId);
        }

        if (asset.Status is DataAssetStatus.UploadFailed or DataAssetStatus.VerificationFailed)
        {
            asset.ResetForRetry();
        }

        var storageKey = BuildStorageKey(asset);
        var grant = await _objectStorage.CreateUploadGrantAsync(
            _storageOptions.Bucket,
            storageKey,
            asset.MimeType ?? "application/octet-stream",
            asset.Size,
            _storageOptions.UploadGrantExpiry,
            cancellationToken);

        asset.MarkUploadInProgress();
        await _assetRepository.UpdateAsync(asset, cancellationToken);

        return new AssetUploadGrantResponse(
            asset.Id,
            grant.UploadUrl,
            grant.StorageKey,
            grant.Bucket,
            grant.ExpiresAt,
            grant.Headers);
    }

    public async Task<AssetResponse> CompleteUploadAsync(
        Guid organizationId,
        AssetCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdAsync(organizationId, request.AssetId, cancellationToken)
            ?? throw new AssetNotFoundException(request.AssetId);

        if (asset.Status == DataAssetStatus.UploadedVerified)
        {
            return ToResponse(asset);
        }

        await using var transaction = await _transactionManager.BeginTransactionAsync(cancellationToken);
        try
        {
            var storageKey = BuildStorageKey(asset);
            var objectInfo = await _objectStorage.GetObjectInfoAsync(
                _storageOptions.Bucket, storageKey, cancellationToken);

            if (objectInfo is null)
            {
                asset.MarkUploadFailed("Object not found in storage");
                await _assetRepository.UpdateAsync(asset, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ToResponse(asset);
            }

            if (objectInfo.Size != asset.Size)
            {
                asset.MarkVerificationFailed(
                    $"Size mismatch: expected {asset.Size}, got {objectInfo.Size}");
                await _assetRepository.UpdateAsync(asset, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return ToResponse(asset);
            }

            if (asset.Checksum is not null)
            {
                var actualChecksum = await _objectStorage.GetObjectChecksumAsync(
                    _storageOptions.Bucket, storageKey, cancellationToken);

                if (actualChecksum is null ||
                    !string.Equals(actualChecksum, asset.Checksum, StringComparison.OrdinalIgnoreCase))
                {
                    asset.MarkVerificationFailed(
                        $"Checksum mismatch: expected {asset.Checksum}, got {actualChecksum ?? "null"}");
                    await _assetRepository.UpdateAsync(asset, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return ToResponse(asset);
                }
            }

            asset.MarkUploaded(storageKey, _storageOptions.Bucket);
            asset.MarkVerified();
            await _assetRepository.UpdateAsync(asset, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return ToResponse(asset);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<AssetResponse?> GetByIdAsync(
        Guid organizationId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdAsync(organizationId, assetId, cancellationToken);
        return asset is null ? null : ToResponse(asset);
    }

    private string BuildStorageKey(DataAsset asset)
    {
        return $"orgs/{asset.OrganizationId}/sessions/{asset.SessionId}/assets/{asset.Id}/r{asset.Revision}";
    }

    private static AssetResponse ToResponse(DataAsset asset) => new(
        asset.Id,
        asset.SessionId,
        asset.TrialId,
        asset.AssetType,
        asset.ExternalId,
        asset.MimeType,
        asset.Size,
        asset.Checksum,
        asset.Revision,
        asset.Status.ToString(),
        asset.UploadedAt,
        asset.VerifiedAt,
        asset.FailureReason);
}

public class ObjectStorageOptions
{
    public string Bucket { get; set; } = "kwinhub-assets";
    public TimeSpan UploadGrantExpiry { get; set; } = TimeSpan.FromMinutes(15);
}

public class AssetNotFoundException : Exception
{
    public AssetNotFoundException(Guid assetId)
        : base($"Asset {assetId} not found.")
    {
    }
}

public class AssetAlreadyUploadedException : Exception
{
    public AssetAlreadyUploadedException(Guid assetId)
        : base($"Asset {assetId} is already uploaded and verified.")
    {
    }
}