namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Quota rule attached to a specific plan version. LimitValue = null means
/// explicitly unlimited; 0 means no capacity.
/// </summary>
public class PlanQuota
{
    public Guid PlanId { get; private set; }

    public string QuotaKey { get; private set; } = string.Empty;

    public long? LimitValue { get; private set; }

    private PlanQuota()
    {
    }

    public PlanQuota(Guid planId, string quotaKey, long? limitValue)
    {
        PlanId = planId;
        QuotaKey = quotaKey;
        LimitValue = limitValue;
    }

    internal void SetLimit(long? limitValue) => LimitValue = limitValue;
}
