using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Assets;

public class DataAsset : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public Guid SessionId { get; private set; }

    public Guid? TrialId { get; private set; }

    public string AssetType { get; private set; } = string.Empty;

    public string? ExternalId { get; private set; }

    public string? MimeType { get; private set; }

    public long Size { get; private set; }

    public string? Checksum { get; private set; }

    public int Revision { get; private set; } = 1;

    public DataAssetStatus Status { get; private set; } = DataAssetStatus.PendingUpload;

    public string? StorageKey { get; private set; }

    public string? StorageBucket { get; private set; }

    public DateTimeOffset? UploadedAt { get; private set; }

    public DateTimeOffset? VerifiedAt { get; private set; }

    public string? FailureReason { get; private set; }

    public string? MediaProviderId { get; private set; }

    public string? PlaybackUrl { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    private DataAsset()
    {
    }

    public DataAsset(
        Guid organizationId,
        Guid sessionId,
        Guid? trialId,
        string assetType,
        long size,
        string? externalId = null,
        string? mimeType = null,
        string? checksum = null)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        SessionId = sessionId;
        TrialId = trialId;
        AssetType = assetType;
        Size = size;
        ExternalId = externalId;
        MimeType = mimeType;
        Checksum = checksum;
    }

    public void MarkUploadInProgress()
    {
        Status = DataAssetStatus.UploadInProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkUploaded(string storageKey, string storageBucket)
    {
        Status = DataAssetStatus.UploadedUnverified;
        StorageKey = storageKey;
        StorageBucket = storageBucket;
        UploadedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkVerified()
    {
        Status = DataAssetStatus.UploadedVerified;
        VerifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkUploadFailed(string reason)
    {
        Status = DataAssetStatus.UploadFailed;
        FailureReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkVerificationFailed(string reason)
    {
        Status = DataAssetStatus.VerificationFailed;
        FailureReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetForRetry()
    {
        Status = DataAssetStatus.PendingUpload;
        StorageKey = null;
        StorageBucket = null;
        UploadedAt = null;
        VerifiedAt = null;
        FailureReason = null;
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkProcessing()
    {
        if (Status != DataAssetStatus.UploadedVerified)
        {
            throw new InvalidOperationException(
                $"Cannot mark asset as Processing from status {Status}. Expected UploadedVerified.");
        }

        Status = DataAssetStatus.Processing;
        FailureReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkReady(string mediaProviderId, string? playbackUrl = null)
    {
        if (Status != DataAssetStatus.Processing)
        {
            throw new InvalidOperationException(
                $"Cannot mark asset as Ready from status {Status}. Expected Processing.");
        }

        Status = DataAssetStatus.Ready;
        MediaProviderId = mediaProviderId;
        PlaybackUrl = playbackUrl;
        ProcessedAt = DateTimeOffset.UtcNow;
        FailureReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkProcessingFailed(string reason)
    {
        Status = DataAssetStatus.ProcessingFailed;
        FailureReason = reason;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetProcessingForRetry()
    {
        if (Status is not DataAssetStatus.ProcessingFailed)
        {
            throw new InvalidOperationException(
                $"Cannot reset processing for retry from status {Status}. Expected ProcessingFailed.");
        }

        Status = DataAssetStatus.UploadedVerified;
        FailureReason = null;
        MediaProviderId = null;
        PlaybackUrl = null;
        ProcessedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}