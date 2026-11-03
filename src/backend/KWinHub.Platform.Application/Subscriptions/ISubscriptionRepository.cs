using KWinHub.Domain.Subscriptions;

namespace KWinHub.Application.Subscriptions;

public interface ISubscriptionRepository
{
    // Plans
    Task<IReadOnlyList<SubscriptionPlan>> ListPlansAsync(CancellationToken cancellationToken = default);
    Task<SubscriptionPlan?> GetPlanAsync(Guid planId, CancellationToken cancellationToken = default);
    Task AddPlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default);
    Task UpdatePlanAsync(SubscriptionPlan plan, CancellationToken cancellationToken = default);

    // Organization subscriptions
    Task<OrganizationSubscription?> GetActiveSubscriptionAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrganizationSubscription>> ListSubscriptionsAsync(
        Guid? organizationId = null,
        CancellationToken cancellationToken = default);
    Task AddSubscriptionAsync(OrganizationSubscription subscription, CancellationToken cancellationToken = default);
    Task UpdateSubscriptionAsync(OrganizationSubscription subscription, CancellationToken cancellationToken = default);

    // Feature definitions
    Task<IReadOnlyList<FeatureDefinition>> ListFeatureDefinitionsAsync(CancellationToken cancellationToken = default);

    // Overrides
    Task<IReadOnlyList<OrganizationFeatureOverride>> GetFeatureOverridesAsync(
        Guid organizationId, CancellationToken cancellationToken = default);
    Task<OrganizationFeatureOverride?> GetFeatureOverrideAsync(
        Guid organizationId, string featureKey, CancellationToken cancellationToken = default);
    Task AddFeatureOverrideAsync(OrganizationFeatureOverride overrideEntity, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrganizationQuotaOverride>> GetQuotaOverridesAsync(
        Guid organizationId, CancellationToken cancellationToken = default);
    Task<OrganizationQuotaOverride?> GetQuotaOverrideAsync(
        Guid organizationId, string quotaKey, CancellationToken cancellationToken = default);
    Task AddQuotaOverrideAsync(OrganizationQuotaOverride overrideEntity, CancellationToken cancellationToken = default);

    // Audit
    Task AddAuditEventAsync(SubscriptionAuditEvent auditEvent, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SubscriptionAuditEvent>> ListAuditEventsAsync(
        Guid organizationId, CancellationToken cancellationToken = default);
}
