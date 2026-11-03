using KWinHub.Contracts.Subscriptions;
using KWinHub.Domain.Subscriptions;

namespace KWinHub.Application.Subscriptions;

public class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly IEntitlementResolver _entitlementResolver;
    private readonly IUsageMeter _usageMeter;
    private readonly TimeProvider _clock;

    public SubscriptionService(
        ISubscriptionRepository repository,
        IEntitlementResolver entitlementResolver,
        IUsageMeter usageMeter,
        TimeProvider clock)
    {
        _repository = repository;
        _entitlementResolver = entitlementResolver;
        _usageMeter = usageMeter;
        _clock = clock;
    }

    public async Task<SubscriptionSummary?> GetSummaryAsync(
        Guid callerOrganizationId,
        Guid requestedOrganizationId,
        CancellationToken cancellationToken = default)
    {
        if (requestedOrganizationId != callerOrganizationId)
        {
            return null; // avoid tenant enumeration
        }

        var subscription = await _repository.GetActiveSubscriptionAsync(callerOrganizationId, cancellationToken);
        if (subscription is null)
        {
            return null;
        }

        var plan = await _repository.GetPlanAsync(subscription.PlanId, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        return new SubscriptionSummary(
            OrganizationId: subscription.OrganizationId,
            SubscriptionId: subscription.Id,
            Plan: new PlanSummary(plan.Code, plan.Name, plan.Version),
            Status: subscription.Status.ToString(),
            StartsAt: subscription.StartsAt,
            CurrentPeriodStart: subscription.StartsAt,
            CurrentPeriodEnd: subscription.EndsAt,
            GraceUntil: subscription.GraceUntil,
            CancelAtPeriodEnd: subscription.CancelAtPeriodEnd,
            RenewalMode: subscription.RenewalMode.ToString(),
            Currency: subscription.Currency,
            AmountMinor: subscription.AmountMinor,
            Revision: subscription.Revision);
    }

    public async Task<UsageSummary?> GetUsageAsync(
        Guid callerOrganizationId,
        Guid requestedOrganizationId,
        CancellationToken cancellationToken = default)
    {
        if (requestedOrganizationId != callerOrganizationId)
        {
            return null;
        }

        var subscription = await _repository.GetActiveSubscriptionAsync(callerOrganizationId, cancellationToken);
        var plan = subscription is not null
            ? await _repository.GetPlanAsync(subscription.PlanId, cancellationToken)
            : null;

        var items = new List<UsageItem>();
        foreach (var quotaKey in SubscriptionCatalog.AllQuotaKeys)
        {
            long consumed = await _usageMeter.GetConsumedAsync(callerOrganizationId, quotaKey, cancellationToken);
            long? limit = await _usageMeter.GetEffectiveLimitAsync(callerOrganizationId, quotaKey, cancellationToken);

            DateTimeOffset? periodStart = null;
            DateTimeOffset? periodEnd = null;
            if (quotaKey == SubscriptionCatalog.QuotaSessionsMonthly)
            {
                var now = _clock.GetUtcNow();
                periodStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                periodEnd = periodStart.Value.AddMonths(1);
            }

            items.Add(new UsageItem(
                Key: quotaKey,
                Consumed: consumed,
                Limit: limit,
                Unit: SubscriptionCatalog.QuotaUnits[quotaKey],
                PeriodStart: periodStart,
                PeriodEnd: periodEnd));
        }

        return new UsageSummary(
            OrganizationId: callerOrganizationId,
            AsOf: _clock.GetUtcNow(),
            Items: items);
    }

    public async Task<EntitlementsResponse?> GetEntitlementsAsync(
        Guid callerOrganizationId,
        Guid requestedOrganizationId,
        CancellationToken cancellationToken = default)
    {
        if (requestedOrganizationId != callerOrganizationId)
        {
            return null;
        }

        var results = await _entitlementResolver.ResolveAllAsync(callerOrganizationId, cancellationToken);
        var items = results.Select(r => new EntitlementItem(
            FeatureKey: r.FeatureKey,
            Enabled: r.Enabled,
            Source: r.Source.ToString(),
            EffectiveUntil: r.EffectiveUntil,
            Reason: r.Reason)).ToList();

        return new EntitlementsResponse(OrganizationId: callerOrganizationId, Items: items);
    }

    public async Task<AccessResponse?> GetAccessAsync(
        Guid callerOrganizationId,
        Guid requestedOrganizationId,
        CancellationToken cancellationToken = default)
    {
        if (requestedOrganizationId != callerOrganizationId)
        {
            return null;
        }

        var results = await _entitlementResolver.ResolveAllAsync(callerOrganizationId, cancellationToken);
        var features = results.ToDictionary(r => r.FeatureKey, r => r.Enabled);

        var subscription = await _repository.GetActiveSubscriptionAsync(callerOrganizationId, cancellationToken);
        var status = subscription?.Status.ToString() ?? "None";

        return new AccessResponse(
            OrganizationId: callerOrganizationId,
            Permissions: Array.Empty<string>(),
            Features: features,
            EffectiveStatus: status,
            Revision: subscription?.Revision ?? 0);
    }
}
