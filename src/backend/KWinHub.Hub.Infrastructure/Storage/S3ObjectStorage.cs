using Amazon.S3;
using Amazon.S3.Model;
using KWinHub.Hub.Assets;

namespace KWinHub.Hub.Infrastructure.Storage;

public class S3ObjectStorage : IObjectStorage
{
    private readonly IAmazonS3 _s3Client;

    public S3ObjectStorage(IAmazonS3 s3Client)
    {
        _s3Client = s3Client;
    }

    public async Task<PresignedUploadGrant> CreateUploadGrantAsync(
        string bucket,
        string key,
        string contentType,
        long contentLength,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType = contentType,
            Expires = DateTime.UtcNow.Add(expiry)
        };

        var url = await Task.Run(() => _s3Client.GetPreSignedURL(request), cancellationToken);

        return new PresignedUploadGrant(
            url,
            key,
            bucket,
            DateTimeOffset.UtcNow.Add(expiry),
            new Dictionary<string, string>
            {
                ["Content-Type"] = contentType
            });
    }

    public async Task<ObjectStorageObject?> GetObjectInfoAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _s3Client.GetObjectMetadataAsync(bucket, key, cancellationToken);
            return new ObjectStorageObject(
                key,
                bucket,
                response.ContentLength,
                response.ETag,
                response.Headers.ContentType,
                response.LastModified);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<string?> GetObjectChecksumAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _s3Client.GetObjectMetadataAsync(bucket, key, cancellationToken);
            return response.ETag?.Trim('"');
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteObjectAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        await _s3Client.DeleteObjectAsync(bucket, key, cancellationToken);
    }

    public async Task<bool> ObjectExistsAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.GetObjectMetadataAsync(bucket, key, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }
}