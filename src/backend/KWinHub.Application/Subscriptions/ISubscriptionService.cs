using KWinHub.Contracts.Subscriptions;

namespace KWinHub.Application.Subscriptions;

/// <summary>
/// Customer-facing subscription read services. All methods enforce that the
/// requesting organization matches the caller's organization (or return null
/// to avoid tenant enumeration).
/// </summary>
public interface ISubscriptionService
{
    Task<SubscriptionSummary?> GetSummaryAsync(
        Guid callerOrganizationId,
        Guid requestedOrganizationId,
        CancellationToken cancellationToken = default);

    Task<UsageSummary?> GetUsageAsync(
        Guid callerOrganizationId,
        Guid requestedOrganizationId,
        CancellationToken cancellationToken = default);

    Task<EntitlementsResponse?> GetEntitlementsAsync(
        Guid callerOrganizationId,
        Guid requestedOrganizationId,
        CancellationToken cancellationToken = default);

    Task<AccessResponse?> GetAccessAsync(
        Guid callerOrganizationId,
        Guid requestedOrganizationId,
        CancellationToken cancellationToken = default);
}
