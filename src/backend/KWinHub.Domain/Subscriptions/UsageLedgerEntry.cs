using KWinHub.Domain.Common;

namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Append-only ledger of usage events. Unique on (OrganizationId, MetricKey,
/// BusinessEventId) to guarantee idempotent metering.
/// </summary>
public class UsageLedgerEntry : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string MetricKey { get; private set; } = string.Empty;

    public string BusinessEventId { get; private set; } = string.Empty;

    public long Delta { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset PeriodStart { get; private set; }

    private UsageLedgerEntry()
    {
    }

    public UsageLedgerEntry(
        Guid organizationId,
        string metricKey,
        string businessEventId,
        long delta,
        DateTimeOffset occurredAt,
        DateTimeOffset periodStart,
        Guid? id = null)
    {
        if (delta <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(delta), "Usage delta must be positive.");
        }

        Id = id ?? Guid.NewGuid();
        OrganizationId = organizationId;
        MetricKey = metricKey;
        BusinessEventId = businessEventId;
        Delta = delta;
        OccurredAt = occurredAt;
        PeriodStart = periodStart;
    }
}
