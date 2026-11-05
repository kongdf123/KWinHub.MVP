using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Workers;

public static class WorkerJobStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Skipped = "Skipped";
}

public static class WorkerJobType
{
    public const string TranscodeVideo = "TranscodeVideo";
}

public static class WorkerQueueName
{
    public const string Media = "media";
    public const string Report = "report";
}

public class WorkerJob : BaseEntity<Guid>
{
    public Guid AssetId { get; private set; }

    public Guid OrganizationId { get; private set; }

    public string QueueName { get; private set; } = WorkerQueueName.Media;

    public string JobType { get; private set; } = string.Empty;

    public string Status { get; private set; } = WorkerJobStatus.Pending;

    public int Attempt { get; private set; }

    public string? PayloadJson { get; private set; }

    public string? ResultJson { get; private set; }

    public DateTimeOffset? LeaseExpiresAt { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public int MaxAttempts { get; private set; } = 3;

    private WorkerJob()
    {
    }

    public WorkerJob(
        Guid assetId,
        Guid organizationId,
        string jobType,
        string? payloadJson = null,
        int maxAttempts = 3,
        string queueName = WorkerQueueName.Media)
    {
        Id = Guid.NewGuid();
        AssetId = assetId;
        OrganizationId = organizationId;
        QueueName = queueName;
        JobType = jobType;
        PayloadJson = payloadJson;
        MaxAttempts = maxAttempts;
        Status = WorkerJobStatus.Pending;
    }

    public void Claim(TimeSpan leaseDuration)
    {
        Attempt++;
        Status = WorkerJobStatus.Processing;
        LeaseExpiresAt = DateTimeOffset.UtcNow.Add(leaseDuration);
        LastError = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkCompleted(string? resultJson = null)
    {
        Status = WorkerJobStatus.Completed;
        ResultJson = resultJson;
        CompletedAt = DateTimeOffset.UtcNow;
        LeaseExpiresAt = null;
        LastError = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        if (Attempt >= MaxAttempts)
        {
            Status = WorkerJobStatus.Failed;
        }
        else
        {
            Status = WorkerJobStatus.Pending;
        }

        LastError = error;
        LeaseExpiresAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSkipped(string? reason = null)
    {
        Status = WorkerJobStatus.Skipped;
        ResultJson = reason;
        CompletedAt = DateTimeOffset.UtcNow;
        LeaseExpiresAt = null;
        LastError = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetLease()
    {
        Status = WorkerJobStatus.Pending;
        LeaseExpiresAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RenewLease(TimeSpan leaseDuration)
    {
        LeaseExpiresAt = DateTimeOffset.UtcNow.Add(leaseDuration);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetForRetry()
    {
        Status = WorkerJobStatus.Pending;
        Attempt = 0;
        LeaseExpiresAt = null;
        LastError = null;
        ResultJson = null;
        CompletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsLeaseExpired()
    {
        return LeaseExpiresAt.HasValue && LeaseExpiresAt.Value < DateTimeOffset.UtcNow;
    }
}