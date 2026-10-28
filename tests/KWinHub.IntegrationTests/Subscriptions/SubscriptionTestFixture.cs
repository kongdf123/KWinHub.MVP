using KWinHub.Application.Subscriptions;
using KWinHub.Domain.Organizations;
using KWinHub.Domain.Subscriptions;
using KWinHub.Domain.Users;
using KWinHub.Infrastructure.Persistence;
using KWinHub.Infrastructure.Persistence.Repositories;
using KWinHub.Infrastructure.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KWinHub.IntegrationTests.Subscriptions;

/// <summary>
/// Builds a fresh in-memory <see cref="KWinHubDbContext"/> seeded with two
/// organizations, users, memberships, feature definitions, two published plans
/// and an active Pro subscription on OrgA. Exposes the subscription application
/// services wired to a controllable <see cref="TestTimeProvider"/>.
/// </summary>
public sealed class SubscriptionTestFixture : IDisposable
{
    public KWinHubDbContext Db { get; }
    public TestTimeProvider Clock { get; } = new();

    public Guid OrgAId { get; } = Guid.NewGuid();
    public Guid OrgBId { get; } = Guid.NewGuid();
    public Guid UserAId { get; } = Guid.NewGuid();
    public Guid UserBId { get; } = Guid.NewGuid();
    public Guid PlatformAdminId { get; } = Guid.NewGuid();

    public Guid StarterPlanId { get; private set; }
    public Guid ProPlanId { get; private set; }
    public Guid OrgASubscriptionId { get; private set; }

    public ISubscriptionRepository Repository { get; }
    public IEntitlementResolver EntitlementResolver { get; }
    public IUsageMeter UsageMeter { get; }
    public ISubscriptionService SubscriptionService { get; }
    public IPlatformSubscriptionService PlatformService { get; }

    public SubscriptionTestFixture()
    {
        var options = new DbContextOptionsBuilder<KWinHubDbContext>()
            .UseInMemoryDatabase(databaseName: $"kwinhub_sub_{Guid.NewGuid():N}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        Db = new KWinHubDbContext(options);
        Seed();

        Repository = new SubscriptionRepository(Db);
        EntitlementResolver = new EntitlementResolver(Repository, Clock);
        UsageMeter = new UsageMeter(Db, Repository, Clock);
        SubscriptionService = new SubscriptionService(Repository, EntitlementResolver, UsageMeter, Clock);
        PlatformService = new PlatformSubscriptionService(Repository, Clock);
    }

    private void Seed()
    {
        // Feature definitions (matching DatabaseSeeder catalog)
        Db.FeatureDefinitions.AddRange(
            new FeatureDefinition(SubscriptionCatalog.FeatureTestingSprint, "Testing", "Sprint Testing", defaultEnabled: false),
            new FeatureDefinition(SubscriptionCatalog.FeatureTestingForceplate, "Testing", "Force Plate Testing", defaultEnabled: true),
            new FeatureDefinition(SubscriptionCatalog.FeatureAnalyticsAdvanced, "Analytics", "Advanced Analytics", defaultEnabled: false),
            new FeatureDefinition(SubscriptionCatalog.FeatureIntegrationsApi, "Integrations", "API Integrations", defaultEnabled: false),
            new FeatureDefinition(SubscriptionCatalog.FeatureReportsExport, "Reports", "Report Export", defaultEnabled: true),
            new FeatureDefinition(SubscriptionCatalog.FeatureSubscriptionView, "Subscription", "View Subscription", defaultEnabled: true),
            new FeatureDefinition(SubscriptionCatalog.FeatureSubscriptionManage, "Subscription", "Manage Subscription", defaultEnabled: false));

        // Starter plan
        var starter = new SubscriptionPlan("starter", "Starter", 1, "Starter plan");
        starter.AddFeature(SubscriptionCatalog.FeatureTestingForceplate, enabled: true);
        starter.AddFeature(SubscriptionCatalog.FeatureReportsExport, enabled: true);
        starter.AddFeature(SubscriptionCatalog.FeatureSubscriptionView, enabled: true);
        starter.AddFeature(SubscriptionCatalog.FeatureTestingSprint, enabled: false);
        starter.AddFeature(SubscriptionCatalog.FeatureAnalyticsAdvanced, enabled: false);
        starter.AddFeature(SubscriptionCatalog.FeatureIntegrationsApi, enabled: false);
        starter.AddQuota(SubscriptionCatalog.QuotaMembersActive, 25);
        starter.AddQuota(SubscriptionCatalog.QuotaDevicesRegistered, 10);
        starter.AddQuota(SubscriptionCatalog.QuotaSessionsMonthly, 1000);
        starter.AddQuota(SubscriptionCatalog.QuotaStorageBytes, 5_000_000_000L);
        starter.Publish();
        StarterPlanId = starter.Id;

        // Pro plan
        var pro = new SubscriptionPlan("performance-pro", "Performance Pro", 1, "Performance Pro plan");
        pro.AddFeature(SubscriptionCatalog.FeatureTestingForceplate, enabled: true);
        pro.AddFeature(SubscriptionCatalog.FeatureTestingSprint, enabled: true);
        pro.AddFeature(SubscriptionCatalog.FeatureAnalyticsAdvanced, enabled: true);
        pro.AddFeature(SubscriptionCatalog.FeatureIntegrationsApi, enabled: true);
        pro.AddFeature(SubscriptionCatalog.FeatureReportsExport, enabled: true);
        pro.AddFeature(SubscriptionCatalog.FeatureSubscriptionView, enabled: true);
        pro.AddQuota(SubscriptionCatalog.QuotaMembersActive, 100);
        pro.AddQuota(SubscriptionCatalog.QuotaDevicesRegistered, 50);
        pro.AddQuota(SubscriptionCatalog.QuotaSessionsMonthly, 5000);
        pro.AddQuota(SubscriptionCatalog.QuotaStorageBytes, 50_000_000_000L);
        pro.Publish();
        ProPlanId = pro.Id;

        Db.SubscriptionPlans.AddRange(starter, pro);

        // Organizations
        Db.Organizations.Add(new Organization("Org A", "org-a", id: OrgAId));
        Db.Organizations.Add(new Organization("Org B", "org-b", id: OrgBId));

        // Users
        Db.Users.Add(new User("a@example.com", "hash", "User A", id: UserAId));
        Db.Users.Add(new User("b@example.com", "hash", "User B", id: UserBId));
        Db.Users.Add(new User("admin@example.com", "hash", "Platform Admin", platformRole: "PlatformAdmin", id: PlatformAdminId));

        // Active Pro subscription for OrgA (effective now + 1 year)
        var now = Clock.GetUtcNow();
        var subscription = new OrganizationSubscription(
            organizationId: OrgAId,
            planId: ProPlanId,
            status: SubscriptionStatus.Active,
            startsAt: now.AddDays(-1),
            endsAt: now.AddYears(1));
        OrgASubscriptionId = subscription.Id;
        Db.OrganizationSubscriptions.Add(subscription);

        Db.SaveChanges();
    }

    public void Dispose()
    {
        Db.Dispose();
    }
}

/// <summary>
/// Controllable <see cref="TimeProvider"/> for deterministic time-bounded
/// override expiry tests.
/// </summary>
public sealed class TestTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2026, 1, 15, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan delta) => _now = _now.Add(delta);

    public void SetUtcNow(DateTimeOffset value) => _now = value;
}
