using KWinHub.Domain.Common;

namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// An organization's subscription to a published plan version. Effective-dated
/// and versioned; changes do not silently rewrite past contracts.
/// </summary>
public class OrganizationSubscription : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public Guid PlanId { get; private set; }

    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Trialing;

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset EndsAt { get; private set; }

    public DateTimeOffset? GraceUntil { get; private set; }

    public bool CancelAtPeriodEnd { get; private set; }

    public RenewalMode RenewalMode { get; private set; } = RenewalMode.SalesManaged;

    public string? Currency { get; private set; }

    public long? AmountMinor { get; private set; }

    /// <summary>
    /// Monotonic revision incremented on every mutation; used for optimistic
    /// concurrency (If-Match).
    /// </summary>
    public int Revision { get; private set; } = 1;

    /// <summary>
    /// EF Core concurrency token (rowversion). Mapped to PostgreSQL xmin.
    /// </summary>
    public uint RowVersion { get; private set; }

    private OrganizationSubscription()
    {
    }

    public OrganizationSubscription(
        Guid organizationId,
        Guid planId,
        SubscriptionStatus status,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        DateTimeOffset? graceUntil = null,
        RenewalMode renewalMode = RenewalMode.SalesManaged,
        string? currency = null,
        long? amountMinor = null,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        OrganizationId = organizationId;
        PlanId = planId;
        Status = status;
        StartsAt = startsAt;
        EndsAt = endsAt;
        GraceUntil = graceUntil;
        RenewalMode = renewalMode;
        Currency = currency;
        AmountMinor = amountMinor;
    }

    public void ChangePlan(Guid planId, DateTimeOffset startsAt, DateTimeOffset endsAt, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required for plan changes.", nameof(reason));
        }

        PlanId = planId;
        StartsAt = startsAt;
        EndsAt = endsAt;
        BumpRevision();
    }

    public void ChangeStatus(SubscriptionStatus status, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required for status changes.", nameof(reason));
        }

        Status = status;
        BumpRevision();
    }

    public void SetGraceUntil(DateTimeOffset? graceUntil, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required.", nameof(reason));
        }

        GraceUntil = graceUntil;
        BumpRevision();
    }

    public void SetCancelAtPeriodEnd(bool cancelAtPeriodEnd, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required.", nameof(reason));
        }

        CancelAtPeriodEnd = cancelAtPeriodEnd;
        BumpRevision();
    }

    /// <summary>
    /// Whether the subscription currently permits contracted capabilities,
    /// considering status and grace period.
    /// </summary>
    public bool IsEffective(DateTimeOffset now)
    {
        if (Status == SubscriptionStatus.Canceled || Status == SubscriptionStatus.Expired)
        {
            return GraceUntil.HasValue && now <= GraceUntil.Value;
        }

        if (Status == SubscriptionStatus.Suspended)
        {
            return false;
        }

        return now >= StartsAt && (EndsAt == default || now <= EndsAt || (GraceUntil.HasValue && now <= GraceUntil.Value));
    }

    private void BumpRevision()
    {
        Revision++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
