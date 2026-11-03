using KWinHub.Application.Subscriptions;
using KWinHub.Domain.Subscriptions;

namespace KWinHub.Infrastructure.Subscriptions;

public class EntitlementResolver : IEntitlementResolver
{
    private readonly ISubscriptionRepository _repository;
    private readonly TimeProvider _clock;

    public EntitlementResolver(ISubscriptionRepository repository, TimeProvider clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<EntitlementResult> ResolveAsync(
        Guid organizationId,
        string featureKey,
        CancellationToken cancellationToken = default)
    {
        var now = _clock.GetUtcNow();
        var subscription = await _repository.GetActiveSubscriptionAsync(organizationId, cancellationToken);

        // No subscription: fall back to feature default policy (deny by default).
        if (subscription is null)
        {
            var defs = await _repository.ListFeatureDefinitionsAsync(cancellationToken);
            var def = defs.FirstOrDefault(d => d.Key == featureKey);
            return new EntitlementResult(
                featureKey,
                def?.DefaultEnabled ?? false,
                EntitlementSource.LifecyclePolicy,
                null,
                "No active subscription",
                0);
        }

        var plan = await _repository.GetPlanAsync(subscription.PlanId, cancellationToken);
        var planFeature = plan?.Features.FirstOrDefault(f => f.FeatureKey == featureKey);
        bool planEnabled = planFeature?.Enabled ?? false;

        // Check effective overrides; deny wins if multiple overlap.
        var overrides = await _repository.GetFeatureOverridesAsync(organizationId, cancellationToken);
        var effectiveOverrides = overrides
            .Where(o => o.FeatureKey == featureKey && o.IsEffective(now))
            .ToList();

        if (effectiveOverrides.Count > 0)
        {
            // Deny wins on conflicting overlaps.
            bool anyDeny = effectiveOverrides.Any(o => !o.Enabled);
            bool enabled = !anyDeny;
            var chosen = effectiveOverrides.First(o => o.Enabled == !anyDeny);
            return new EntitlementResult(
                featureKey,
                enabled,
                EntitlementSource.Override,
                chosen.EndsAt,
                chosen.Reason,
                subscription.Revision);
        }

        // Plan default. If subscription is not effective (expired/suspended without grace),
        // deny commercial features but allow lifecycle-basic ones via LifecyclePolicy.
        if (!subscription.IsEffective(now))
        {
            return new EntitlementResult(
                featureKey,
                false,
                EntitlementSource.LifecyclePolicy,
                subscription.GraceUntil,
                $"Subscription status: {subscription.Status}",
                subscription.Revision);
        }

        return new EntitlementResult(
            featureKey,
            planEnabled,
            EntitlementSource.Plan,
            subscription.EndsAt,
            planEnabled ? "Included in plan" : "Not included in plan",
            subscription.Revision);
    }

    public async Task<IReadOnlyList<EntitlementResult>> ResolveAllAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default)
    {
        var defs = await _repository.ListFeatureDefinitionsAsync(cancellationToken);
        var results = new List<EntitlementResult>(defs.Count);
        foreach (var def in defs)
        {
            results.Add(await ResolveAsync(organizationId, def.Key, cancellationToken));
        }

        return results;
    }
}
