namespace KWinHub.Domain.Subscriptions;

public enum SubscriptionStatus
{
    Trialing,
    Active,
    PastDue,
    GracePeriod,
    Suspended,
    Expired,
    Canceled,
}
