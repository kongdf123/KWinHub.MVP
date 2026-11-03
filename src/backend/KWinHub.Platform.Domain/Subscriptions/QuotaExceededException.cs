namespace KWinHub.Domain.Subscriptions;

/// <summary>
/// Thrown when a usage consumption request would exceed the effective quota.
/// Mapped to HTTP 422 with code "quota_exceeded" by the API layer.
/// </summary>
public class QuotaExceededException : Exception
{
    public string MetricKey { get; }

    public long Consumed { get; }

    public long? Limit { get; }

    public long Requested { get; }

    public QuotaExceededException(
        string metricKey,
        long consumed,
        long? limit,
        long requested,
        string? message = null)
        : base(message ?? $"Quota '{metricKey}' exceeded: consumed {consumed}, requested {requested}, limit {limit?.ToString() ?? "unlimited"}.")
    {
        MetricKey = metricKey;
        Consumed = consumed;
        Limit = limit;
        Requested = requested;
    }
}
