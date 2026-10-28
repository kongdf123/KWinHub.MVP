using KWinHub.Application.Subscriptions;
using KWinHub.Contracts.Subscriptions;
using KWinHub.Domain.Subscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace KWinHub.IntegrationTests.Subscriptions;

/// <summary>
/// Integration tests for the Subscription Center covering tenant isolation,
/// feature entitlement resolution (deny-wins, override expiry), idempotent
/// quota metering, audit preservation, and optimistic concurrency.
/// </summary>
public class SubscriptionIntegrationTests
{
    // ---- Tenant isolation -------------------------------------------------

    [Fact]
    public async Task GetSummary_CrossTenantRequest_ReturnsNull()
    {
        using var fixture = new SubscriptionTestFixture();

        // OrgA's caller requests OrgB's subscription.
        var result = await fixture.SubscriptionService.GetSummaryAsync(
            callerOrganizationId: fixture.OrgAId,
            requestedOrganizationId: fixture.OrgBId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetSummary_SameTenant_ReturnsSubscription()
    {
        using var fixture = new SubscriptionTestFixture();

        var result = await fixture.SubscriptionService.GetSummaryAsync(
            callerOrganizationId: fixture.OrgAId,
            requestedOrganizationId: fixture.OrgAId);

        Assert.NotNull(result);
        Assert.Equal(fixture.OrgAId, result.OrganizationId);
        Assert.Equal("performance-pro", result.Plan.Code);
        Assert.Equal("Active", result.Status);
    }

    // ---- Authorization: platform mutation requires PlatformAdmin ---------

    [Fact]
    public async Task PlatformMutation_RequiresPlatformAdminClaim()
    {
        using var fixture = new SubscriptionTestFixture();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("PlatformAdmin", policy => policy.RequireClaim("platform_admin", "true"));
        });
        var sp = services.BuildServiceProvider();
        var authz = sp.GetRequiredService<IAuthorizationService>();

        // Org member (no platform_admin claim) is denied.
        var memberPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("org_id", fixture.OrgAId.ToString()),
            new Claim("org_role", "member"),
        }, "test"));

        var memberResult = await authz.AuthorizeAsync(memberPrincipal, "PlatformAdmin");
        Assert.False(memberResult.Succeeded);

        // Platform admin is allowed.
        var adminPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("platform_admin", "true"),
            new Claim("sub", fixture.PlatformAdminId.ToString()),
        }, "test"));

        var adminResult = await authz.AuthorizeAsync(adminPrincipal, "PlatformAdmin");
        Assert.True(adminResult.Succeeded);
    }

    // ---- Feature entitlement resolution ----------------------------------

    [Fact]
    public async Task ResolveEntitlement_FeatureNotInPlan_IsDenied()
    {
        using var fixture = new SubscriptionTestFixture();

        // subscription.manage is NOT enabled on the Pro plan.
        var result = await fixture.EntitlementResolver.ResolveAsync(
            fixture.OrgAId, SubscriptionCatalog.FeatureSubscriptionManage);

        Assert.False(result.Enabled);
        Assert.Equal(EntitlementSource.Plan, result.Source);
    }

    [Fact]
    public async Task ResolveEntitlement_PlanEnabledFeature_IsGranted()
    {
        using var fixture = new SubscriptionTestFixture();

        var result = await fixture.EntitlementResolver.ResolveAsync(
            fixture.OrgAId, SubscriptionCatalog.FeatureTestingSprint);

        Assert.True(result.Enabled);
        Assert.Equal(EntitlementSource.Plan, result.Source);
    }

    [Fact]
    public async Task ResolveEntitlement_DenyOverrideWinsOverPlan()
    {
        using var fixture = new SubscriptionTestFixture();
        var now = fixture.Clock.GetUtcNow();

        // testing.sprint is enabled on the Pro plan. Set a deny override.
        await fixture.PlatformService.SetFeatureOverrideAsync(
            fixture.OrgAId,
            SubscriptionCatalog.FeatureTestingSprint,
            new SetFeatureOverrideRequest(
                Enabled: false,
                StartsAt: now,
                EndsAt: now.AddDays(30),
                Reason: "Disable sprint for compliance review"),
            actorId: fixture.PlatformAdminId);

        var result = await fixture.EntitlementResolver.ResolveAsync(
            fixture.OrgAId, SubscriptionCatalog.FeatureTestingSprint);

        Assert.False(result.Enabled);
        Assert.Equal(EntitlementSource.Override, result.Source);
    }

    [Fact]
    public async Task ResolveEntitlement_GrantOverrideExpires_RevertsToPlan()
    {
        using var fixture = new SubscriptionTestFixture();
        var now = fixture.Clock.GetUtcNow();

        // analytics.advanced IS enabled on Pro plan. Set a short deny override,
        // then advance past its end and verify it reverts to plan (enabled).
        await fixture.PlatformService.SetFeatureOverrideAsync(
            fixture.OrgAId,
            SubscriptionCatalog.FeatureAnalyticsAdvanced,
            new SetFeatureOverrideRequest(
                Enabled: false,
                StartsAt: now,
                EndsAt: now.AddDays(7),
                Reason: "Temporary disable during migration"),
            actorId: fixture.PlatformAdminId);

        // While active, deny wins.
        var active = await fixture.EntitlementResolver.ResolveAsync(
            fixture.OrgAId, SubscriptionCatalog.FeatureAnalyticsAdvanced);
        Assert.False(active.Enabled);
        Assert.Equal(EntitlementSource.Override, active.Source);

        // Advance past the override end.
        fixture.Clock.Advance(TimeSpan.FromDays(8));

        var expired = await fixture.EntitlementResolver.ResolveAsync(
            fixture.OrgAId, SubscriptionCatalog.FeatureAnalyticsAdvanced);
        Assert.True(expired.Enabled);
        Assert.Equal(EntitlementSource.Plan, expired.Source);
    }

    [Fact]
    public async Task ResolveEntitlement_ConflictingOverrides_DenyWins()
    {
        using var fixture = new SubscriptionTestFixture();
        var now = fixture.Clock.GetUtcNow();

        // Grant override...
        await fixture.PlatformService.SetFeatureOverrideAsync(
            fixture.OrgAId,
            SubscriptionCatalog.FeatureTestingForceplate,
            new SetFeatureOverrideRequest(true, now, now.AddDays(30), "Grant"),
            fixture.PlatformAdminId);

        // ...and an overlapping deny override.
        await fixture.PlatformService.SetFeatureOverrideAsync(
            fixture.OrgAId,
            SubscriptionCatalog.FeatureTestingForceplate,
            new SetFeatureOverrideRequest(false, now, now.AddDays(30), "Deny wins"),
            fixture.PlatformAdminId);

        var result = await fixture.EntitlementResolver.ResolveAsync(
            fixture.OrgAId, SubscriptionCatalog.FeatureTestingForceplate);

        Assert.False(result.Enabled);
        Assert.Equal(EntitlementSource.Override, result.Source);
    }

    // ---- Quota metering ---------------------------------------------------

    [Fact]
    public async Task Consume_DuplicateBusinessEventId_NoDoubleCount()
    {
        using var fixture = new SubscriptionTestFixture();
        var eventId = "session-abc-123";

        var first = await fixture.UsageMeter.ConsumeAsync(
            fixture.OrgAId, SubscriptionCatalog.QuotaSessionsMonthly, eventId, delta: 1);
        Assert.Equal(1, first);

        // Same business event id again — should be a no-op returning the prior delta.
        var second = await fixture.UsageMeter.ConsumeAsync(
            fixture.OrgAId, SubscriptionCatalog.QuotaSessionsMonthly, eventId, delta: 1);
        Assert.Equal(1, second);

        var consumed = await fixture.UsageMeter.GetConsumedAsync(
            fixture.OrgAId, SubscriptionCatalog.QuotaSessionsMonthly);
        Assert.Equal(1, consumed);
    }

    [Fact]
    public async Task Consume_ExceedsHardLimit_ThrowsQuotaExceeded()
    {
        using var fixture = new SubscriptionTestFixture();

        // Pro plan allows 5000 monthly sessions. Override to a tiny limit.
        var now = fixture.Clock.GetUtcNow();
        await fixture.PlatformService.SetQuotaOverrideAsync(
            fixture.OrgAId,
            SubscriptionCatalog.QuotaSessionsMonthly,
            new SetQuotaOverrideRequest(LimitValue: 2, StartsAt: now, EndsAt: now.AddDays(30), Reason: "Trial cap"),
            fixture.PlatformAdminId);

        // Consume up to the limit.
        await fixture.UsageMeter.ConsumeAsync(fixture.OrgAId, SubscriptionCatalog.QuotaSessionsMonthly, "evt-1", 1);
        await fixture.UsageMeter.ConsumeAsync(fixture.OrgAId, SubscriptionCatalog.QuotaSessionsMonthly, "evt-2", 1);

        // Third session should exceed the limit.
        var ex = await Assert.ThrowsAsync<QuotaExceededException>(() =>
            fixture.UsageMeter.ConsumeAsync(fixture.OrgAId, SubscriptionCatalog.QuotaSessionsMonthly, "evt-3", 1));

        Assert.Equal(SubscriptionCatalog.QuotaSessionsMonthly, ex.MetricKey);

        var consumed = await fixture.UsageMeter.GetConsumedAsync(
            fixture.OrgAId, SubscriptionCatalog.QuotaSessionsMonthly);
        Assert.Equal(2, consumed);
    }

    [Fact]
    public async Task GetEffectiveLimit_OverrideReplacesPlanLimit()
    {
        using var fixture = new SubscriptionTestFixture();
        var now = fixture.Clock.GetUtcNow();

        // Plan limit is 5000.
        var planLimit = await fixture.UsageMeter.GetEffectiveLimitAsync(
            fixture.OrgAId, SubscriptionCatalog.QuotaSessionsMonthly);
        Assert.Equal(5000, planLimit);

        await fixture.PlatformService.SetQuotaOverrideAsync(
            fixture.OrgAId,
            SubscriptionCatalog.QuotaSessionsMonthly,
            new SetQuotaOverrideRequest(LimitValue: 42, StartsAt: now, EndsAt: now.AddDays(30), Reason: "Adjustment"),
            fixture.PlatformAdminId);

        var overridden = await fixture.UsageMeter.GetEffectiveLimitAsync(
            fixture.OrgAId, SubscriptionCatalog.QuotaSessionsMonthly);
        Assert.Equal(42, overridden);
    }

    // ---- Audit & concurrency ----------------------------------------------

    [Fact]
    public async Task AssignSubscription_CreatesAuditEvent()
    {
        using var fixture = new SubscriptionTestFixture();
        var now = fixture.Clock.GetUtcNow();

        // OrgB has no subscription yet.
        await fixture.PlatformService.AssignSubscriptionAsync(
            fixture.OrgBId,
            new AssignSubscriptionRequest(
                PlanId: fixture.StarterPlanId,
                Status: "Active",
                StartsAt: now,
                EndsAt: now.AddYears(1),
                GraceUntil: null,
                Reason: "Sales onboarding"),
            actorId: fixture.PlatformAdminId,
            expectedRevision: null);

        var audit = await fixture.PlatformService.ListAuditEventsAsync(fixture.OrgBId);
        Assert.NotEmpty(audit);
        Assert.Contains(audit, e => e.Action == "subscription.created" && e.Reason == "Sales onboarding");
    }

    [Fact]
    public async Task AssignSubscription_PlanChange_PreservesAuditAndBumpsRevision()
    {
        using var fixture = new SubscriptionTestFixture();
        var now = fixture.Clock.GetUtcNow();

        var before = await fixture.PlatformService.ListSubscriptionsAsync(fixture.OrgAId);
        var originalRevision = before.Single().Revision;

        // Change OrgA from Pro to Starter.
        var updated = await fixture.PlatformService.AssignSubscriptionAsync(
            fixture.OrgAId,
            new AssignSubscriptionRequest(
                PlanId: fixture.StarterPlanId,
                Status: "Active",
                StartsAt: now,
                EndsAt: now.AddYears(1),
                GraceUntil: null,
                Reason: "Downgrade per request"),
            actorId: fixture.PlatformAdminId,
            expectedRevision: originalRevision);

        Assert.True(updated.Revision > originalRevision, "Revision should bump on mutation.");
        Assert.Equal("starter", updated.PlanCode);

        var audit = await fixture.PlatformService.ListAuditEventsAsync(fixture.OrgAId);
        Assert.Contains(audit, e => e.Action == "subscription.changed" && e.Reason == "Downgrade per request");
    }

    [Fact]
    public async Task AssignSubscription_StaleRevision_ThrowsConcurrencyConflict()
    {
        using var fixture = new SubscriptionTestFixture();
        var now = fixture.Clock.GetUtcNow();

        var subs = await fixture.PlatformService.ListSubscriptionsAsync(fixture.OrgAId);
        var currentRevision = subs.Single().Revision;

        // Pass a stale (too-low) expected revision.
        var ex = await Assert.ThrowsAsync<ConcurrencyConflictException>(() =>
            fixture.PlatformService.AssignSubscriptionAsync(
                fixture.OrgAId,
                new AssignSubscriptionRequest(
                    PlanId: fixture.StarterPlanId,
                    Status: "Active",
                    StartsAt: now,
                    EndsAt: now.AddYears(1),
                    GraceUntil: null,
                    Reason: "Stale attempt"),
                actorId: fixture.PlatformAdminId,
                expectedRevision: currentRevision - 1));

        Assert.Contains("revision mismatch", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
