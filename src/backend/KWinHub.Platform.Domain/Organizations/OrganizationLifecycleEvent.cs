using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Platform.Organizations;

public static class OrganizationLifecycleEventTypes
{
    public const string Created = "organization.created";
    public const string Suspended = "organization.suspended";
    public const string Reactivated = "organization.reactivated";
    public const string Deleted = "organization.deleted";
}

public class OrganizationLifecycleEvent : BaseEntity<Guid>
{
    public const string StatusPending = "Pending";
    public const string StatusSent = "Sent";
    public const string StatusFailed = "Failed";

    public Guid OrganizationId { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public long EventVersion { get; private set; }

    public string Status { get; private set; } = StatusPending;

    public string PayloadJson { get; private set; } = string.Empty;

    public int RetryCount { get; private set; }

    public DateTimeOffset? SentAt { get; private set; }

    public string? LastError { get; private set; }

    private OrganizationLifecycleEvent()
    {
    }

    public OrganizationLifecycleEvent(
        Guid organizationId,
        string eventType,
        long eventVersion,
        string payloadJson)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        EventType = eventType;
        EventVersion = eventVersion;
        PayloadJson = payloadJson;
        Status = StatusPending;
    }

    public void MarkSent()
    {
        Status = StatusSent;
        SentAt = DateTimeOffset.UtcNow;
        LastError = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = StatusFailed;
        RetryCount++;
        LastError = error;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetForRetry()
    {
        Status = StatusPending;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}