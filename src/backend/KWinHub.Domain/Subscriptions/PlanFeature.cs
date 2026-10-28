namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Feature grant attached to a specific plan version. Enabled indicates the
/// feature is part of the contracted plan.
/// </summary>
public class PlanFeature
{
    public Guid PlanId { get; private set; }

    public string FeatureKey { get; private set; } = string.Empty;

    public bool Enabled { get; private set; }

    private PlanFeature()
    {
    }

    public PlanFeature(Guid planId, string featureKey, bool enabled)
    {
        PlanId = planId;
        FeatureKey = featureKey;
        Enabled = enabled;
    }

    internal void SetEnabled(bool enabled) => Enabled = enabled;
}
