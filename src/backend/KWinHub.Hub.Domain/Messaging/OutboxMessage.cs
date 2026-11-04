using KWinHub.BuildingBlocks.Common;

namespace KWinHub.Hub.Messaging;

public static class OutboxMessageStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
}

public class OutboxMessage : BaseEntity<Guid>
{
    public Guid EventId { get; private set; }

    public string EventType { get; private set; } = string.Empty;

    public Guid OrganizationId { get; private set; }

    public string? AggregateType { get; private set; }

    public Guid? AggregateId { get; private set; }

    public long EventVersion { get; private set; }

    public string PayloadJson { get; private set; } = string.Empty;

    public string Status { get; private set; } = OutboxMessageStatus.Pending;

    public int RetryCount { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public DateTimeOffset? LeaseExpiresAt { get; private set; }

    public string? LastError { get; private set; }

    private OutboxMessage()
    {
    }

    public OutboxMessage(
        Guid eventId,
        string eventType,
        Guid organizationId,
        string? aggregateType,
        Guid? aggregateId,
        long eventVersion,
        string payloadJson)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        EventType = eventType;
        OrganizationId = organizationId;
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        EventVersion = eventVersion;
        PayloadJson = payloadJson;
        Status = OutboxMessageStatus.Pending;
    }

    public void Claim(TimeSpan leaseDuration)
    {
        Status = OutboxMessageStatus.Processing;
        LeaseExpiresAt = DateTimeOffset.UtcNow.Add(leaseDuration);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkSent()
    {
        Status = OutboxMessageStatus.Sent;
        ProcessedAt = DateTimeOffset.UtcNow;
        LeaseExpiresAt = null;
        LastError = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = OutboxMessageStatus.Failed;
        RetryCount++;
        LastError = error;
        LeaseExpiresAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ResetForRetry()
    {
        Status = OutboxMessageStatus.Pending;
        LeaseExpiresAt = null;
        LastError = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}