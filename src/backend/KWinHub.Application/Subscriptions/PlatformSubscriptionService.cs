using System.Text.Json;
using KWinHub.Contracts.Subscriptions;
using KWinHub.Domain.Subscriptions;

namespace KWinHub.Application.Subscriptions;

public class PlatformSubscriptionService : IPlatformSubscriptionService
{
    private readonly ISubscriptionRepository _repository;
    private readonly TimeProvider _clock;

    public PlatformSubscriptionService(ISubscriptionRepository repository, TimeProvider clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<IReadOnlyList<PlanResponse>> ListPlansAsync(CancellationToken cancellationToken = default)
    {
        var plans = await _repository.ListPlansAsync(cancellationToken);
        return plans.Select(ToPlanResponse).ToList();
    }

    public async Task<PlanResponse> CreatePlanAsync(CreatePlanRequest request, Guid actorId, CancellationToken cancellationToken = default)
    {
        var plan = new SubscriptionPlan(request.Code, request.Name, request.Version, request.Description);
        foreach (var f in request.Features)
        {
            plan.AddFeature(f.FeatureKey, f.Enabled);
        }

        foreach (var q in request.Quotas)
        {
            plan.AddQuota(q.QuotaKey, q.LimitValue);
        }

        await _repository.AddPlanAsync(plan, cancellationToken);
        return ToPlanResponse(plan);
    }

    public async Task<PlanResponse> PublishPlanAsync(Guid planId, Guid actorId, CancellationToken cancellationToken = default)
    {
        var plan = await _repository.GetPlanAsync(planId, cancellationToken)
            ?? throw new InvalidOperationException($"Plan {planId} not found.");

        plan.Publish();
        await _repository.UpdatePlanAsync(plan, cancellationToken);
        return ToPlanResponse(plan);
    }

    public async Task<IReadOnlyList<PlatformSubscriptionResponse>> ListSubscriptionsAsync(
        Guid? organizationId = null, CancellationToken cancellationToken = default)
    {
        var subs = await _repository.ListSubscriptionsAsync(organizationId, cancellationToken);
        var result = new List<PlatformSubscriptionResponse>();
        foreach (var sub in subs)
        {
            var plan = await _repository.GetPlanAsync(sub.PlanId, cancellationToken);
            result.Add(new PlatformSubscriptionResponse(
                Id: sub.Id,
                OrganizationId: sub.OrganizationId,
                PlanId: sub.PlanId,
                PlanCode: plan?.Code ?? string.Empty,
                PlanName: plan?.Name ?? string.Empty,
                PlanVersion: plan?.Version ?? 0,
                Status: sub.Status.ToString(),
                StartsAt: sub.StartsAt,
                EndsAt: sub.EndsAt,
                GraceUntil: sub.GraceUntil,
                CancelAtPeriodEnd: sub.CancelAtPeriodEnd,
                RenewalMode: sub.RenewalMode.ToString(),
                Revision: sub.Revision));
        }

        return result;
    }

    public async Task<PlatformSubscriptionResponse> AssignSubscriptionAsync(
        Guid organizationId,
        AssignSubscriptionRequest request,
        Guid actorId,
        int? expectedRevision,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetActiveSubscriptionAsync(organizationId, cancellationToken);
        string beforeJson = existing is not null ? JsonSerializer.Serialize(ToAuditSnapshot(existing)) : "null";

        if (existing is not null && expectedRevision.HasValue && existing.Revision != expectedRevision.Value)
        {
            throw new ConcurrencyConflictException(
                $"Subscription revision mismatch. Expected {expectedRevision}, actual {existing.Revision}.");
        }

        var status = Enum.Parse<SubscriptionStatus>(request.Status);
        if (existing is not null)
        {
            existing.ChangePlan(request.PlanId, request.StartsAt, request.EndsAt, request.Reason);
            existing.ChangeStatus(status, request.Reason);
            if (request.GraceUntil.HasValue)
            {
                existing.SetGraceUntil(request.GraceUntil, request.Reason);
            }

            await _repository.UpdateSubscriptionAsync(existing, cancellationToken);
            await AppendAuditAsync(organizationId, "subscription.changed", request.Reason, actorId, beforeJson, JsonSerializer.Serialize(ToAuditSnapshot(existing)), cancellationToken);
            return await ToPlatformSubscriptionResponse(existing, cancellationToken);
        }

        var subscription = new OrganizationSubscription(
            organizationId: organizationId,
            planId: request.PlanId,
            status: status,
            startsAt: request.StartsAt,
            endsAt: request.EndsAt,
            graceUntil: request.GraceUntil);
        await _repository.AddSubscriptionAsync(subscription, cancellationToken);
        await AppendAuditAsync(organizationId, "subscription.created", request.Reason, actorId, beforeJson, JsonSerializer.Serialize(ToAuditSnapshot(subscription)), cancellationToken);
        return await ToPlatformSubscriptionResponse(subscription, cancellationToken);
    }

    public async Task<FeatureOverrideResponse> SetFeatureOverrideAsync(
        Guid organizationId,
        string featureKey,
        SetFeatureOverrideRequest request,
        Guid actorId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetFeatureOverrideAsync(organizationId, featureKey, cancellationToken);
        string beforeJson = existing is not null ? JsonSerializer.Serialize(new { existing.Enabled, existing.StartsAt, existing.EndsAt }) : "null";

        var overrideEntity = new OrganizationFeatureOverride(
            organizationId: organizationId,
            featureKey: featureKey,
            enabled: request.Enabled,
            startsAt: request.StartsAt,
            endsAt: request.EndsAt,
            reason: request.Reason,
            createdBy: actorId);
        await _repository.AddFeatureOverrideAsync(overrideEntity, cancellationToken);

        await AppendAuditAsync(
            organizationId,
            "feature_override.set",
            request.Reason,
            actorId,
            beforeJson,
            JsonSerializer.Serialize(new { overrideEntity.Enabled, overrideEntity.StartsAt, overrideEntity.EndsAt }),
            cancellationToken);

        return new FeatureOverrideResponse(
            Id: overrideEntity.Id,
            OrganizationId: overrideEntity.OrganizationId,
            FeatureKey: overrideEntity.FeatureKey,
            Enabled: overrideEntity.Enabled,
            StartsAt: overrideEntity.StartsAt,
            EndsAt: overrideEntity.EndsAt,
            Reason: overrideEntity.Reason,
            CreatedAt: overrideEntity.CreatedAt);
    }

    public async Task<QuotaOverrideResponse> SetQuotaOverrideAsync(
        Guid organizationId,
        string quotaKey,
        SetQuotaOverrideRequest request,
        Guid actorId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetQuotaOverrideAsync(organizationId, quotaKey, cancellationToken);
        string beforeJson = existing is not null ? JsonSerializer.Serialize(new { existing.LimitValue, existing.StartsAt, existing.EndsAt }) : "null";

        var overrideEntity = new OrganizationQuotaOverride(
            organizationId: organizationId,
            quotaKey: quotaKey,
            limitValue: request.LimitValue,
            startsAt: request.StartsAt,
            endsAt: request.EndsAt,
            reason: request.Reason,
            createdBy: actorId);
        await _repository.AddQuotaOverrideAsync(overrideEntity, cancellationToken);

        await AppendAuditAsync(
            organizationId,
            "quota_override.set",
            request.Reason,
            actorId,
            beforeJson,
            JsonSerializer.Serialize(new { overrideEntity.LimitValue, overrideEntity.StartsAt, overrideEntity.EndsAt }),
            cancellationToken);

        return new QuotaOverrideResponse(
            Id: overrideEntity.Id,
            OrganizationId: overrideEntity.OrganizationId,
            QuotaKey: overrideEntity.QuotaKey,
            LimitValue: overrideEntity.LimitValue,
            StartsAt: overrideEntity.StartsAt,
            EndsAt: overrideEntity.EndsAt,
            Reason: overrideEntity.Reason,
            CreatedAt: overrideEntity.CreatedAt);
    }

    public async Task<IReadOnlyList<SubscriptionAuditEventResponse>> ListAuditEventsAsync(
        Guid organizationId, CancellationToken cancellationToken = default)
    {
        var events = await _repository.ListAuditEventsAsync(organizationId, cancellationToken);
        return events.Select(e => new SubscriptionAuditEventResponse(
            Id: e.Id,
            OrganizationId: e.OrganizationId,
            ActorId: e.ActorId,
            Action: e.Action,
            BeforeJson: e.BeforeJson,
            AfterJson: e.AfterJson,
            Reason: e.Reason,
            OccurredAt: e.OccurredAt,
            RequestId: e.RequestId)).ToList();
    }

    private async Task AppendAuditAsync(
        Guid organizationId,
        string action,
        string reason,
        Guid actorId,
        string beforeJson,
        string afterJson,
        CancellationToken cancellationToken)
    {
        var audit = new SubscriptionAuditEvent(
            organizationId: organizationId,
            action: action,
            reason: reason,
            actorId: actorId,
            beforeJson: beforeJson,
            afterJson: afterJson);
        await _repository.AddAuditEventAsync(audit, cancellationToken);
    }

    private static object ToAuditSnapshot(OrganizationSubscription sub) => new
    {
        sub.Id,
        sub.PlanId,
        Status = sub.Status.ToString(),
        sub.StartsAt,
        sub.EndsAt,
        sub.GraceUntil,
        sub.Revision,
    };

    private static PlanResponse ToPlanResponse(SubscriptionPlan plan) => new(
        Id: plan.Id,
        Code: plan.Code,
        Name: plan.Name,
        Version: plan.Version,
        Status: plan.Status.ToString(),
        Description: plan.Description,
        Features: plan.Features.Select(f => new PlanFeatureResponse(f.FeatureKey, f.Enabled)).ToList(),
        Quotas: plan.Quotas.Select(q => new PlanQuotaResponse(q.QuotaKey, q.LimitValue)).ToList(),
        CreatedAt: plan.CreatedAt);

    private async Task<PlatformSubscriptionResponse> ToPlatformSubscriptionResponse(
        OrganizationSubscription sub, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetPlanAsync(sub.PlanId, cancellationToken);
        return new PlatformSubscriptionResponse(
            Id: sub.Id,
            OrganizationId: sub.OrganizationId,
            PlanId: sub.PlanId,
            PlanCode: plan?.Code ?? string.Empty,
            PlanName: plan?.Name ?? string.Empty,
            PlanVersion: plan?.Version ?? 0,
            Status: sub.Status.ToString(),
            StartsAt: sub.StartsAt,
            EndsAt: sub.EndsAt,
            GraceUntil: sub.GraceUntil,
            CancelAtPeriodEnd: sub.CancelAtPeriodEnd,
            RenewalMode: sub.RenewalMode.ToString(),
            Revision: sub.Revision);
    }
}
