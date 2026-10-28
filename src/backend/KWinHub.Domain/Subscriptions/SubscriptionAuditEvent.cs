using KWinHub.Domain.Common;

namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Append-only audit record for subscription state changes. Stores before/after
/// snapshots and a mandatory reason.
/// </summary>
public class SubscriptionAuditEvent : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public Guid? ActorId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string? BeforeJson { get; private set; }

    public string? AfterJson { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; } = DateTimeOffset.UtcNow;

    public string? RequestId { get; private set; }

    private SubscriptionAuditEvent()
    {
    }

    public SubscriptionAuditEvent(
        Guid organizationId,
        string action,
        string reason,
        Guid? actorId = null,
        string? beforeJson = null,
        string? afterJson = null,
        string? requestId = null,
        DateTimeOffset? occurredAt = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required for audit events.", nameof(reason));
        }

        Id = id ?? Guid.NewGuid();
        OrganizationId = organizationId;
        Action = action;
        Reason = reason;
        ActorId = actorId;
        BeforeJson = beforeJson;
        AfterJson = afterJson;
        RequestId = requestId;
        OccurredAt = occurredAt ?? DateTimeOffset.UtcNow;
    }
}
