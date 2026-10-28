namespace KWinHub.Contracts.Subscriptions;

public record PlanSummary(
    string Code,
    string Name,
    int Version);

public record SubscriptionSummary(
    Guid OrganizationId,
    Guid SubscriptionId,
    PlanSummary Plan,
    string Status,
    DateTimeOffset StartsAt,
    DateTimeOffset CurrentPeriodStart,
    DateTimeOffset CurrentPeriodEnd,
    DateTimeOffset? GraceUntil,
    bool CancelAtPeriodEnd,
    string RenewalMode,
    string? Currency,
    long? AmountMinor,
    int Revision);

public record UsageItem(
    string Key,
    long Consumed,
    long? Limit,
    string Unit,
    DateTimeOffset? PeriodStart,
    DateTimeOffset? PeriodEnd);

public record UsageSummary(
    Guid OrganizationId,
    DateTimeOffset AsOf,
    IReadOnlyList<UsageItem> Items);

public record EntitlementItem(
    string FeatureKey,
    bool Enabled,
    string Source,
    DateTimeOffset? EffectiveUntil,
    string? Reason);

public record EntitlementsResponse(
    Guid OrganizationId,
    IReadOnlyList<EntitlementItem> Items);

public record AccessResponse(
    Guid OrganizationId,
    IReadOnlyList<string> Permissions,
    IReadOnlyDictionary<string, bool> Features,
    string EffectiveStatus,
    int Revision);
