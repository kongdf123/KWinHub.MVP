namespace KWinHub.Application.Subscriptions;

/// <summary>
/// Thrown when a platform mutation targets a subscription whose revision does
/// not match the caller's expected revision (stale If-Match). Mapped to HTTP 409.
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message) : base(message)
    {
    }
}
