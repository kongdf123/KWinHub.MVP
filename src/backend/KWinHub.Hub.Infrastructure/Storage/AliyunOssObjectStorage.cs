using Aliyun.OSS;
using Aliyun.OSS.Common;
using KWinHub.Hub.Assets;

namespace KWinHub.Hub.Infrastructure.Storage;

public class AliyunOssObjectStorage : IObjectStorage
{
    private readonly IOss _ossClient;

    public AliyunOssObjectStorage(IOss ossClient)
    {
        _ossClient = ossClient;
    }

    public Task<PresignedUploadGrant> CreateUploadGrantAsync(
        string bucket,
        string key,
        string contentType,
        long contentLength,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var request = new GeneratePresignedUriRequest(bucket, key, SignHttpMethod.Put)
        {
            Expiration = DateTime.Now.Add(expiry),
            ContentType = contentType
        };

        var uri = _ossClient.GeneratePresignedUri(request);

        var headers = new Dictionary<string, string>
        {
            ["Content-Type"] = contentType
        };

        var grant = new PresignedUploadGrant(
            uri.ToString(),
            key,
            bucket,
            DateTimeOffset.UtcNow.Add(expiry),
            headers);

        return Task.FromResult(grant);
    }

    public Task<ObjectStorageObject?> GetObjectInfoAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = _ossClient.GetObjectMetadata(bucket, key);
            var obj = new ObjectStorageObject(
                key,
                bucket,
                metadata.ContentLength,
                metadata.ETag,
                metadata.ContentType,
                metadata.LastModified);
            return Task.FromResult<ObjectStorageObject?>(obj);
        }
        catch (OssException ex) when (ex.ErrorCode == "NoSuchKey")
        {
            return Task.FromResult<ObjectStorageObject?>(null);
        }
    }

    public Task<string?> GetObjectChecksumAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = _ossClient.GetObjectMetadata(bucket, key);
            var etag = metadata.ETag?.Trim('"');
            return Task.FromResult<string?>(etag);
        }
        catch (OssException ex) when (ex.ErrorCode == "NoSuchKey")
        {
            return Task.FromResult<string?>(null);
        }
    }

    public Task DeleteObjectAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        _ossClient.DeleteObject(bucket, key);
        return Task.CompletedTask;
    }

    public Task<bool> ObjectExistsAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        var exists = _ossClient.DoesObjectExist(bucket, key);
        return Task.FromResult(exists);
    }
}