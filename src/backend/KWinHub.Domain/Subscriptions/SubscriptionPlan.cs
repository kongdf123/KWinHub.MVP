using KWinHub.Domain.Common;

namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// An immutable-once-published plan version. Each (Code, Version) is unique.
/// Published versions cannot be modified; create a new version to change terms.
/// </summary>
public class SubscriptionPlan : BaseEntity<Guid>
{
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public int Version { get; private set; }

    public PlanStatus Status { get; private set; } = PlanStatus.Draft;

    public string? Description { get; private set; }

    public IReadOnlyList<PlanFeature> Features => _features.ToList();

    public IReadOnlyList<PlanQuota> Quotas => _quotas.ToList();

    private readonly List<PlanFeature> _features = new();
    private readonly List<PlanQuota> _quotas = new();

    private SubscriptionPlan()
    {
    }

    public SubscriptionPlan(
        string code,
        string name,
        int version,
        string? description = null,
        Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        Code = code;
        Name = name;
        Version = version;
        Description = description;
    }

    public void AddFeature(string featureKey, bool enabled)
    {
        EnsureDraft();
        if (_features.Any(f => f.FeatureKey == featureKey))
        {
            throw new InvalidOperationException($"Feature '{featureKey}' already defined on plan.");
        }

        _features.Add(new PlanFeature(Id, featureKey, enabled));
    }

    public void SetFeature(string featureKey, bool enabled)
    {
        EnsureDraft();
        var existing = _features.FirstOrDefault(f => f.FeatureKey == featureKey);
        if (existing is null)
        {
            _features.Add(new PlanFeature(Id, featureKey, enabled));
        }
        else
        {
            existing.SetEnabled(enabled);
        }
    }

    public void AddQuota(string quotaKey, long? limitValue)
    {
        EnsureDraft();
        if (limitValue is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limitValue), "Quota limit cannot be negative.");
        }

        if (_quotas.Any(q => q.QuotaKey == quotaKey))
        {
            throw new InvalidOperationException($"Quota '{quotaKey}' already defined on plan.");
        }

        _quotas.Add(new PlanQuota(Id, quotaKey, limitValue));
    }

    public void SetQuota(string quotaKey, long? limitValue)
    {
        EnsureDraft();
        if (limitValue is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limitValue), "Quota limit cannot be negative.");
        }

        var existing = _quotas.FirstOrDefault(q => q.QuotaKey == quotaKey);
        if (existing is null)
        {
            _quotas.Add(new PlanQuota(Id, quotaKey, limitValue));
        }
        else
        {
            existing.SetLimit(limitValue);
        }
    }

    public void Publish()
    {
        if (Status == PlanStatus.Published)
        {
            throw new InvalidOperationException("Plan is already published.");
        }

        Status = PlanStatus.Published;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        if (Status != PlanStatus.Published)
        {
            throw new InvalidOperationException("Only published plans can be archived.");
        }

        Status = PlanStatus.Archived;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureDraft()
    {
        if (Status != PlanStatus.Draft)
        {
            throw new InvalidOperationException("Only draft plans can be modified. Create a new version to change a published plan.");
        }
    }
}
