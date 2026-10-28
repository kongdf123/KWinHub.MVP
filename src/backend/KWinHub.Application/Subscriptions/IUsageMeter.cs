using KWinHub.Domain.Subscriptions;

namespace KWinHub.Application.Subscriptions;

/// <summary>
/// Atomic, idempotent usage metering. Consumption is recorded once per stable
/// business event id; duplicate events are no-ops. Quota is checked against the
/// effective limit (plan + active override).
/// </summary>
public interface IUsageMeter
{
    /// <summary>
    /// Attempts to consume <paramref name="delta"/> units of <paramref name="metricKey"/>
    /// for the organization. Throws <see cref="QuotaExceededException"/> when the
    /// effective limit would be exceeded. Duplicate <paramref name="businessEventId"/>
    /// is a no-op and returns the previously consumed delta.
    /// </summary>
    Task<long> ConsumeAsync(
        Guid organizationId,
        string metricKey,
        string businessEventId,
        long delta,
        CancellationToken cancellationToken = default);

    Task<long> GetConsumedAsync(
        Guid organizationId,
        string metricKey,
        CancellationToken cancellationToken = default);

    Task<long?> GetEffectiveLimitAsync(
        Guid organizationId,
        string metricKey,
        CancellationToken cancellationToken = default);
}
