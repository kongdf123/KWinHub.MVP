namespace KWinHub.Hub.Application.CrossContext;

/// <summary>
/// Raised when the Platform subscription authority rejects a metered write
/// because the organization has exhausted a quota. Mapped to HTTP 422 by the Hub API.
/// </summary>
public sealed class QuotaExceededException : Exception
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