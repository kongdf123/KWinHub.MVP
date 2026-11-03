using KWinHub.Contracts.Subscriptions;

namespace KWinHub.Application.Subscriptions;

/// <summary>
/// Platform-facing subscription management. All mutations require a mandatory
/// reason and produce an audit event. Subscription mutations use optimistic
/// concurrency (revision); a stale revision throws a concurrency exception
/// mapped to HTTP 409.
/// </summary>
public interface IPlatformSubscriptionService
{
    // Plans
    Task<IReadOnlyList<PlanResponse>> ListPlansAsync(CancellationToken cancellationToken = default);
    Task<PlanResponse> CreatePlanAsync(CreatePlanRequest request, Guid actorId, CancellationToken cancellationToken = default);
    Task<PlanResponse> PublishPlanAsync(Guid planId, Guid actorId, CancellationToken cancellationToken = default);

    // Subscriptions
    Task<IReadOnlyList<PlatformSubscriptionResponse>> ListSubscriptionsAsync(
        Guid? organizationId = null, CancellationToken cancellationToken = default);
    Task<PlatformSubscriptionResponse> AssignSubscriptionAsync(
        Guid organizationId,
        AssignSubscriptionRequest request,
        Guid actorId,
        int? expectedRevision,
        CancellationToken cancellationToken = default);

    // Overrides
    Task<FeatureOverrideResponse> SetFeatureOverrideAsync(
        Guid organizationId,
        string featureKey,
        SetFeatureOverrideRequest request,
        Guid actorId,
        CancellationToken cancellationToken = default);

    Task<QuotaOverrideResponse> SetQuotaOverrideAsync(
        Guid organizationId,
        string quotaKey,
        SetQuotaOverrideRequest request,
        Guid actorId,
        CancellationToken cancellationToken = default);

    // Audit
    Task<IReadOnlyList<SubscriptionAuditEventResponse>> ListAuditEventsAsync(
        Guid organizationId, CancellationToken cancellationToken = default);
}
