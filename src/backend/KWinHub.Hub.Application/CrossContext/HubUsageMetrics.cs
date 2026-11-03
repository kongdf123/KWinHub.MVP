namespace KWinHub.Hub.Application.CrossContext;

/// <summary>
/// Usage metric keys the Hub context meters against the Platform subscription
/// service. The literal values mirror Platform's SubscriptionCatalog keys so the
/// two services agree without sharing a domain assembly. M1 introduces a
/// versioned catalog sync; for M0 the frozen seam uses these fixed strings.
/// </summary>
public static class HubUsageMetrics
{
    public const string SessionsMonthly = "sessions.monthly";
}
