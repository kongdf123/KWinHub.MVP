using KWinHub.Domain.Common;

namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Rolling aggregate of consumed usage per (OrganizationId, MetricKey, PeriodStart).
/// Updated transactionally with the ledger to keep quota checks consistent.
/// </summary>
public class UsageAggregate : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string MetricKey { get; private set; } = string.Empty;

    public DateTimeOffset PeriodStart { get; private set; }

    public DateTimeOffset PeriodEnd { get; private set; }

    public long Consumed { get; private set; }

    private UsageAggregate()
    {
    }

    public UsageAggregate(
        Guid organizationId,
        string metricKey,
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        long consumed = 0,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        OrganizationId = organizationId;
        MetricKey = metricKey;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        Consumed = consumed;
    }

    public void Add(long delta)
    {
        Consumed += delta;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
