using System.Collections.Concurrent;
using KWinHub.Hub.Assets;

namespace KWinHub.IntegrationTests.Assets;

public class InMemoryObjectStorage : IObjectStorage
{
    private readonly ConcurrentDictionary<string, StoredObject> _objects = new();

    public Task<PresignedUploadGrant> CreateUploadGrantAsync(
        string bucket,
        string key,
        string contentType,
        long contentLength,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var grant = new PresignedUploadGrant(
            $"https://storage.example.com/{bucket}/{key}?token={Guid.NewGuid():N}",
            key,
            bucket,
            DateTimeOffset.UtcNow.Add(expiry),
            new Dictionary<string, string> { ["Content-Type"] = contentType });

        return Task.FromResult(grant);
    }

    public Task<ObjectStorageObject?> GetObjectInfoAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        var storageKey = $"{bucket}/{key}";
        if (_objects.TryGetValue(storageKey, out var obj))
        {
            return Task.FromResult<ObjectStorageObject?>(new ObjectStorageObject(
                obj.Key, obj.Bucket, obj.Size, obj.ETag, obj.ContentType, obj.LastModified));
        }
        return Task.FromResult<ObjectStorageObject?>(null);
    }

    public Task<string?> GetObjectChecksumAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        var storageKey = $"{bucket}/{key}";
        if (_objects.TryGetValue(storageKey, out var obj))
        {
            return Task.FromResult<string?>(obj.ETag);
        }
        return Task.FromResult<string?>(null);
    }

    public Task DeleteObjectAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        var storageKey = $"{bucket}/{key}";
        _objects.TryRemove(storageKey, out _);
        return Task.CompletedTask;
    }

    public Task<bool> ObjectExistsAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken = default)
    {
        var storageKey = $"{bucket}/{key}";
        return Task.FromResult(_objects.ContainsKey(storageKey));
    }

    public void SetObjectChecksum(string bucket, string key, string? checksum)
    {
        var storageKey = $"{bucket}/{key}";
        if (_objects.TryGetValue(storageKey, out var obj))
        {
            obj.ETag = checksum;
        }
        else
        {
            _objects[storageKey] = new StoredObject(key, bucket, 0, null, DateTimeOffset.UtcNow) { ETag = checksum };
        }
    }

    public void SetObjectSize(string bucket, string key, long size)
    {
        var storageKey = $"{bucket}/{key}";
        if (_objects.TryGetValue(storageKey, out var obj))
        {
            obj.Size = size;
        }
        else
        {
            _objects[storageKey] = new StoredObject(key, bucket, size, null, DateTimeOffset.UtcNow);
        }
    }

    private class StoredObject
    {
        public string Key { get; }
        public string Bucket { get; }
        public long Size { get; set; }
        public string? ETag { get; set; }
        public string? ContentType { get; }
        public DateTimeOffset LastModified { get; }

        public StoredObject(string key, string bucket, long size, string? contentType, DateTimeOffset lastModified)
        {
            Key = key;
            Bucket = bucket;
            Size = size;
            ContentType = contentType;
            LastModified = lastModified;
        }
    }
}