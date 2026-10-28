using KWinHub.Domain.Common;

namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Time-bounded explicit grant/deny of a feature for an organization, overriding
/// the plan default. Deny wins if overlapping conflicting overrides exist.
/// Temporary support grants must have an EndsAt.
/// </summary>
public class OrganizationFeatureOverride : BaseEntity<Guid>
{
    public Guid OrganizationId { get; private set; }

    public string FeatureKey { get; private set; } = string.Empty;

    public bool Enabled { get; private set; }

    public DateTimeOffset StartsAt { get; private set; }

    public DateTimeOffset? EndsAt { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public Guid? CreatedBy { get; private set; }

    private OrganizationFeatureOverride()
    {
    }

    public OrganizationFeatureOverride(
        Guid organizationId,
        string featureKey,
        bool enabled,
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

        if (endsAt.HasValue && endsAt.Value <= startsAt)
        {
            throw new ArgumentException("EndsAt must be after StartsAt.", nameof(endsAt));
        }

        Id = id ?? Guid.NewGuid();
        OrganizationId = organizationId;
        FeatureKey = featureKey;
        Enabled = enabled;
        StartsAt = startsAt;
        EndsAt = endsAt;
        Reason = reason;
        CreatedBy = createdBy;
    }

    public bool IsEffective(DateTimeOffset now) =>
        now >= StartsAt && (!EndsAt.HasValue || now <= EndsAt.Value);
}
