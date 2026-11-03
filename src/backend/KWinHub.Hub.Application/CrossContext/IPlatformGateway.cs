namespace KWinHub.Hub.Application.CrossContext;

/// <summary>
/// Result of a Platform feature-entitlement check, projected into a Hub-local
/// type so the Hub context never references the Platform domain/application.
/// </summary>
public sealed record EntitlementDecision(bool Allowed, string? Reason);

/// <summary>
/// Port used by the Hub context to evaluate subscription entitlements and meter
/// usage against the Platform context. In M0 this is satisfied over an internal,
/// API-key-protected HTTP API. The Hub never recomputes plan entitlements itself
/// (ADR-0001); it only asks the Platform authority and caches nothing.
/// </summary>
public interface IPlatformGateway
{
    Task<EntitlementDecision> ResolveEntitlementAsync(
        Guid organizationId,
        string featureKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records <paramref name="delta"/> units of metered usage against a stable
    /// <paramref name="businessEventId"/> (idempotent on the Platform side) and
    /// returns the resulting period balance. Throws when the quota is exceeded.
    /// </summary>
    Task<long> ConsumeUsageAsync(
        Guid organizationId,
        string metricKey,
        string businessEventId,
        long delta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the security stamp embedded in a user's JWT still matches
    /// the authoritative value held by the Platform. Returns false when the stamp
    /// has been rotated (e.g. session revocation, password change) so the Hub can
    /// reject the token within the revocation SLA.
    /// </summary>
    Task<bool> ValidateSecurityStampAsync(
        Guid userId,
        string securityStamp,
        CancellationToken cancellationToken = default);
}