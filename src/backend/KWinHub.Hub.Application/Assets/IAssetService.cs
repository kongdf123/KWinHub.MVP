namespace KWinHub.Hub.Assets;

public record AssetUploadGrantResponse(
    Guid AssetId,
    string UploadUrl,
    string StorageKey,
    string Bucket,
    DateTimeOffset ExpiresAt,
    IReadOnlyDictionary<string, string> Headers);

public record AssetCompletionRequest(
    Guid AssetId,
    string? Checksum = null);

public record AssetResponse(
    Guid Id,
    Guid SessionId,
    Guid? TrialId,
    string AssetType,
    string? ExternalId,
    string? MimeType,
    long Size,
    string? Checksum,
    int Revision,
    string Status,
    DateTimeOffset? UploadedAt,
    DateTimeOffset? VerifiedAt,
    string? FailureReason,
    string? MediaProviderId = null,
    string? PlaybackUrl = null,
    DateTimeOffset? ProcessedAt = null);

public record PlaybackCredentialResponse(
    Guid AssetId,
    string DownloadUrl,
    DateTimeOffset ExpiresAt,
    string? MimeType);

public interface IAssetService
{
    Task<IReadOnlyList<AssetResponse>> ListBySessionAsync(Guid organizationId, Guid sessionId, CancellationToken cancellationToken = default);

    Task<AssetUploadGrantResponse> RequestUploadGrantAsync(Guid organizationId, Guid assetId, CancellationToken cancellationToken = default);

    Task<AssetResponse> CompleteUploadAsync(Guid organizationId, AssetCompletionRequest request, CancellationToken cancellationToken = default);

    Task<AssetResponse?> GetByIdAsync(Guid organizationId, Guid assetId, CancellationToken cancellationToken = default);

    Task<PlaybackCredentialResponse> RequestPlaybackCredentialAsync(Guid organizationId, Guid assetId, CancellationToken cancellationToken = default);
}