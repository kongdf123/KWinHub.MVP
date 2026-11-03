using KWinHub.Domain.Subscriptions;

namespace KWinHub.Application.Subscriptions;

public sealed record EntitlementResult(
    string FeatureKey,
    bool Enabled,
    EntitlementSource Source,
    DateTimeOffset? EffectiveUntil,
    string? Reason,
    int Revision);

/// <summary>
/// Resolves effective feature entitlements for an organization by combining the
/// active subscription's immutable plan version with any currently effective
/// time-bounded overrides. Deny overrides win on conflict.
/// </summary>
public interface IEntitlementResolver
{
    Task<EntitlementResult> ResolveAsync(
        Guid organizationId,
        string featureKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EntitlementResult>> ResolveAllAsync(
        Guid organizationId,
        CancellationToken cancellationToken = default);
}
