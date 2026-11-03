namespace KWinHub.Contracts.Subscriptions;

// ---- Plans ----

public record PlanFeatureResponse(string FeatureKey, bool Enabled);

public record PlanQuotaResponse(string QuotaKey, long? LimitValue);

public record PlanResponse(
    Guid Id,
    string Code,
    string Name,
    int Version,
    string Status,
    string? Description,
    IReadOnlyList<PlanFeatureResponse> Features,
    IReadOnlyList<PlanQuotaResponse> Quotas,
    DateTimeOffset CreatedAt);

public record CreatePlanRequest(
    string Code,
    string Name,
    int Version,
    string? Description,
    IReadOnlyList<PlanFeatureRequest> Features,
    IReadOnlyList<PlanQuotaRequest> Quotas);

public record PlanFeatureRequest(string FeatureKey, bool Enabled);

public record PlanQuotaRequest(string QuotaKey, long? LimitValue);

// ---- Organization subscriptions ----

public record PlatformSubscriptionResponse(
    Guid Id,
    Guid OrganizationId,
    Guid PlanId,
    string PlanCode,
    string PlanName,
    int PlanVersion,
    string Status,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    DateTimeOffset? GraceUntil,
    bool CancelAtPeriodEnd,
    string RenewalMode,
    int Revision);

public record AssignSubscriptionRequest(
    Guid PlanId,
    string Status,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    DateTimeOffset? GraceUntil,
    string Reason);

// ---- Overrides ----

public record FeatureOverrideResponse(
    Guid Id,
    Guid OrganizationId,
    string FeatureKey,
    bool Enabled,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    string Reason,
    DateTimeOffset CreatedAt);

public record SetFeatureOverrideRequest(
    bool Enabled,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    string Reason);

public record QuotaOverrideResponse(
    Guid Id,
    Guid OrganizationId,
    string QuotaKey,
    long? LimitValue,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    string Reason,
    DateTimeOffset CreatedAt);

public record SetQuotaOverrideRequest(
    long? LimitValue,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    string Reason);

// ---- Audit ----

public record SubscriptionAuditEventResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? ActorId,
    string Action,
    string? BeforeJson,
    string? AfterJson,
    string Reason,
    DateTimeOffset OccurredAt,
    string? RequestId);
