using KWinHub.Domain.Common;

namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Time-bounded quota capacity override for an organization. LimitValue = null
/// means explicitly unlimited; 0 means no capacity. Signed value supports
/// explicit reductions.
/// </summary>
public class OrganizationQuotaOverride : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string QuotaKey { get; private set; } = string.Empty;

    public long? LimitValue { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset? EndsAt { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public Guid? CreatedBy { get; private set; }

    private OrganizationQuotaOverride()
    {
    }

    public OrganizationQuotaOverride(
        Guid organizationId,
        string quotaKey,
        long? limitValue,
        DateTimeOffset startsAt,
        DateTimeOffset? endsAt,
        string reason,
        Guid? createdBy = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required for overrides.", nameof(reason));
        }

        if (limitValue is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limitValue), "Quota limit cannot be negative.");
        }

        if (endsAt.HasValue && endsAt.Value <= startsAt)
        {
            throw new ArgumentException("EndsAt must be after StartsAt.", nameof(endsAt));
        }

        Id = id ?? Guid.NewGuid();
        OrganizationId = organizationId;
        QuotaKey = quotaKey;
        LimitValue = limitValue;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Reason = reason;
        CreatedBy = createdBy;
    }

    public bool IsEffective(DateTimeOffset now) =>
        now >= StartsAt && (!EndsAt.HasValue || now <= EndsAt.Value);
}
