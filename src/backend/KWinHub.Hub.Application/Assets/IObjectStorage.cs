namespace KWinHub.Hub.Assets;

public record PresignedUploadGrant(
    string UploadUrl,
    string StorageKey,
    string Bucket,
    DateTimeOffset ExpiresAt,
    IReadOnlyDictionary<string, string> Headers);

public record ObjectStorageObject(
    string Key,
    string Bucket,
    long Size,
    string? ETag,
    string? ContentType,
    DateTimeOffset LastModified);

public interface IObjectStorage
{
    Task<PresignedUploadGrant> CreateUploadGrantAsync(
        string bucket,
        string key,
        string contentType,
        long contentLength,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    Task<ObjectStorageObject?> GetObjectInfoAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default);

    Task<string?> GetObjectChecksumAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default);

    Task DeleteObjectAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default);

    Task<bool> ObjectExistsAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default);
}