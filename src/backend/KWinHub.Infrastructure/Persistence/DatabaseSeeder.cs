using KWinHub.Domain.Catalogs;
using KWinHub.Domain.Organizations;
using KWinHub.Domain.Protocols;
using KWinHub.Domain.Subjects;
using KWinHub.Domain.Subscriptions;
using KWinHub.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    // Deterministic IDs used by DevAuth token issuer and frontend fixtures
    public static readonly Guid DemoOrganizationId = new("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DemoUserId         = new("22222222-2222-2222-2222-222222222222");
    public static readonly Guid PlatformAdminUserId = new("33333333-3333-3333-3333-333333333333");

    public const string PlatformAdminEmail = "admin@kwinhub.com";
    public const string PlatformAdminPassword = "Admin@12345";
    public const string DemoUserEmail = "demo@kwinhub.com";
    public const string DemoUserPassword = "Demo@12345";

    public static async Task EnsureDevelopmentSeedDataAsync(KWinHubDbContext db, CancellationToken ct = default)
    {
        // Users
        if (!await db.Users.AnyAsync(ct))
        {
            var hasher = new PasswordHasher<User>();

            var platformAdmin = new User(
                email: PlatformAdminEmail,
                passwordHash: string.Empty,
                displayName: "Platform Administrator",
                platformRole: User.PlatformAdminRole,
                id: PlatformAdminUserId);
            platformAdmin.ChangePassword(hasher.HashPassword(platformAdmin, PlatformAdminPassword));
            platformAdmin.MarkEmailVerified();

            var demoUser = new User(
                email: DemoUserEmail,
                passwordHash: string.Empty,
                displayName: "Demo Coach",
                id: DemoUserId);
            demoUser.ChangePassword(hasher.HashPassword(demoUser, DemoUserPassword));
            demoUser.MarkEmailVerified();

            await db.Users.AddRangeAsync(new[] { platformAdmin, demoUser }, ct);
            await db.SaveChangesAsync(ct);
        }

        // Organization
        if (!await db.Organizations.AnyAsync(ct))
        {
            var org = new Organization(
                name: "Kunwei Demo Organization",
                slug: "kunwei-demo",
                id: DemoOrganizationId);
            await db.Organizations.AddAsync(org, ct);

            var membership = new OrganizationMembership(
                userId: DemoUserId,
                organizationId: DemoOrganizationId,
                role: "Owner");
            await db.OrganizationMemberships.AddAsync(membership, ct);
            await db.SaveChangesAsync(ct);
        }

        // Protocols (force plate focused)
        if (!await db.Protocols.AnyAsync(ct))
        {
            var protocols = new[]
            {
                CreateProtocol(DemoOrganizationId, "Countermovement Jump", "CMJ", "1.0", "Force Plate", "General", "Standard bilateral CMJ protocol"),
                CreateProtocol(DemoOrganizationId, "Isometric Mid-Thigh Pull", "IMTP", "1.0", "Force Plate", "Strength", "Maximal isometric strength test"),
                CreateProtocol(DemoOrganizationId, "Drop Jump", "DJ", "1.0", "Force Plate", "Jumping", "Drop jump from 40cm box"),
            };
            await db.Protocols.AddRangeAsync(protocols, ct);
            await db.SaveChangesAsync(ct);
        }

        // Subjects / Athletes
        if (!await db.Subjects.AnyAsync(ct))
        {
            var subjects = new[]
            {
                new Subject(DemoOrganizationId, "伟", "张", SubjectType.Athlete, displayName: "Zhang Wei", sex: "M", dateOfBirth: new DateTimeOffset(2001, 3, 15, 0, 0, 0, TimeSpan.Zero)),
                new Subject(DemoOrganizationId, "明", "李", SubjectType.Athlete, displayName: "Li Ming", sex: "M", dateOfBirth: new DateTimeOffset(2002, 7, 22, 0, 0, 0, TimeSpan.Zero)),
                new Subject(DemoOrganizationId, "浩", "王", SubjectType.Athlete, displayName: "Wang Hao", sex: "M", dateOfBirth: new DateTimeOffset(2000, 11, 8, 0, 0, 0, TimeSpan.Zero)),
                new Subject(DemoOrganizationId, "陈", "杰", SubjectType.Athlete, displayName: "Chen Jie", sex: "M", dateOfBirth: new DateTimeOffset(2003, 1, 30, 0, 0, 0, TimeSpan.Zero)),
                new Subject(DemoOrganizationId, "刘洋", "", SubjectType.Athlete, displayName: "Liu Yang", sex: "M", dateOfBirth: new DateTimeOffset(1999, 5, 14, 0, 0, 0, TimeSpan.Zero)),
            };
            await db.Subjects.AddRangeAsync(subjects, ct);
            await db.SaveChangesAsync(ct);
        }

        // Global protocol catalog
        if (!await db.GlobalProtocols.AnyAsync(ct))
        {
            var protocols = new[]
            {
                CreateGlobalProtocol("CMJ", "Countermovement Jump", "1.0", "Force Plate", "General", "Standard bilateral CMJ protocol", published: true),
                CreateGlobalProtocol("IMTP", "Isometric Mid-Thigh Pull", "1.0", "Force Plate", "Strength", "Maximal isometric strength test", published: true),
                CreateGlobalProtocol("DJ", "Drop Jump", "1.0", "Force Plate", "Jumping", "Drop jump from a 40cm box", published: true),
                CreateGlobalProtocol("SJ", "Squat Jump", "1.0", "Force Plate", "Jumping", "Static squat jump", published: true),
                CreateGlobalProtocol("CMJ-3.0", "Countermovement Jump 3.0", "2.0", "Force Plate", "General", "Next generation CMJ protocol", published: false),
            };
            await db.GlobalProtocols.AddRangeAsync(protocols, ct);
            await db.SaveChangesAsync(ct);
        }

        // Global metric catalog
        if (!await db.GlobalMetrics.AnyAsync(ct))
        {
            var metrics = new[]
            {
                new GlobalMetric("jump_height", "Jump Height", "m", "Kinematics", "Peak jump height"),
                new GlobalMetric("peak_force", "Peak Force", "N", "Force", "Maximum force produced"),
                new GlobalMetric("peak_power", "Peak Power", "W", "Power", "Maximum power output"),
                new GlobalMetric("impulse", "Impulse", "N·s", "Force", "Force-time integral"),
                new GlobalMetric("flight_time", "Flight Time", "s", "Temporal", "Time spent in the air"),
                new GlobalMetric("ground_contact_time", "Ground Contact Time", "s", "Temporal", "Time spent on the ground"),
                new GlobalMetric("reactive_strength_index", "Reactive Strength Index", "m/s", "Performance", "Jump height divided by ground contact time"),
                new GlobalMetric("relative_peak_force", "Relative Peak Force", "N/kg", "Force", "Peak force normalized to body mass"),
                new GlobalMetric("force_at_50ms", "Force at 50ms", "N", "Force", "Force at 50 milliseconds"),
                new GlobalMetric("force_at_100ms", "Force at 100ms", "N", "Force", "Force at 100 milliseconds"),
            };
            await db.GlobalMetrics.AddRangeAsync(metrics, ct);
            await db.SaveChangesAsync(ct);
        }

        // Device model catalog
        if (!await db.DeviceModels.AnyAsync(ct))
        {
            var devices = new[]
            {
                new DeviceModel("Kistler", "9260AA6", "Force Plate", "Piezoelectric force plate"),
                new DeviceModel("Bertec", "FP4060-10", "Force Plate", "Strain gauge force plate"),
                new DeviceModel("Hawkin Dynamics", "HD Force Plate", "Force Plate", "Portable dual force plate"),
            };
            await db.DeviceModels.AddRangeAsync(devices, ct);
            await db.SaveChangesAsync(ct);
        }

        // Algorithm version catalog
        if (!await db.AlgorithmVersions.AnyAsync(ct))
        {
            var publishedAlgorithm = new AlgorithmVersion(
                "ForcePlate Analysis",
                "1.0.0",
                "jump_height,peak_force,peak_power,impulse,flight_time",
                "Initial force plate analysis pipeline");
            publishedAlgorithm.Publish();

            var draftAlgorithm = new AlgorithmVersion(
                "ForcePlate Analysis",
                "1.1.0",
                "jump_height,peak_force,peak_power,impulse,flight_time,reactive_strength_index",
                "Adds reactive strength index calculation");

            await db.AlgorithmVersions.AddRangeAsync(new[] { publishedAlgorithm, draftAlgorithm }, ct);
            await db.SaveChangesAsync(ct);
        }

        // Feature definitions (stable catalog)
        if (!await db.FeatureDefinitions.AnyAsync(ct))
        {
            var features = new[]
            {
                new FeatureDefinition(SubscriptionCatalog.FeatureTestingSprint, "Testing", "Sprint Testing", defaultEnabled: false),
                new FeatureDefinition(SubscriptionCatalog.FeatureTestingForceplate, "Testing", "Force Plate Testing", defaultEnabled: true),
                new FeatureDefinition(SubscriptionCatalog.FeatureAnalyticsAdvanced, "Analytics", "Advanced Analytics", defaultEnabled: false),
                new FeatureDefinition(SubscriptionCatalog.FeatureIntegrationsApi, "Integrations", "API Integrations", defaultEnabled: false),
                new FeatureDefinition(SubscriptionCatalog.FeatureReportsExport, "Reports", "Report Export", defaultEnabled: true),
                new FeatureDefinition(SubscriptionCatalog.FeatureSubscriptionView, "Subscription", "View Subscription", defaultEnabled: true),
                new FeatureDefinition(SubscriptionCatalog.FeatureSubscriptionManage, "Subscription", "Manage Subscription", defaultEnabled: false),
            };
            await db.FeatureDefinitions.AddRangeAsync(features, ct);
            await db.SaveChangesAsync(ct);
        }

        // Starter published plan + demo organization subscription
        if (!await db.SubscriptionPlans.AnyAsync(ct))
        {
            var starterPlan = new SubscriptionPlan(
                code: "starter",
                name: "Starter",
                version: 1,
                description: "Entry-level plan for small teams.");
            starterPlan.AddFeature(SubscriptionCatalog.FeatureTestingForceplate, enabled: true);
            starterPlan.AddFeature(SubscriptionCatalog.FeatureReportsExport, enabled: true);
            starterPlan.AddFeature(SubscriptionCatalog.FeatureSubscriptionView, enabled: true);
            starterPlan.AddFeature(SubscriptionCatalog.FeatureTestingSprint, enabled: false);
            starterPlan.AddFeature(SubscriptionCatalog.FeatureAnalyticsAdvanced, enabled: false);
            starterPlan.AddFeature(SubscriptionCatalog.FeatureIntegrationsApi, enabled: false);
            starterPlan.AddQuota(SubscriptionCatalog.QuotaMembersActive, 25);
            starterPlan.AddQuota(SubscriptionCatalog.QuotaDevicesRegistered, 10);
            starterPlan.AddQuota(SubscriptionCatalog.QuotaSessionsMonthly, 1000);
            starterPlan.AddQuota(SubscriptionCatalog.QuotaStorageBytes, 5_000_000_000L);
            starterPlan.Publish();

            var proPlan = new SubscriptionPlan(
                code: "performance-pro",
                name: "Performance Pro",
                version: 1,
                description: "Professional plan with advanced analytics and integrations.");
            proPlan.AddFeature(SubscriptionCatalog.FeatureTestingForceplate, enabled: true);
            proPlan.AddFeature(SubscriptionCatalog.FeatureTestingSprint, enabled: true);
            proPlan.AddFeature(SubscriptionCatalog.FeatureAnalyticsAdvanced, enabled: true);
            proPlan.AddFeature(SubscriptionCatalog.FeatureIntegrationsApi, enabled: true);
            proPlan.AddFeature(SubscriptionCatalog.FeatureReportsExport, enabled: true);
            proPlan.AddFeature(SubscriptionCatalog.FeatureSubscriptionView, enabled: true);
            proPlan.AddQuota(SubscriptionCatalog.QuotaMembersActive, 100);
            proPlan.AddQuota(SubscriptionCatalog.QuotaDevicesRegistered, 50);
            proPlan.AddQuota(SubscriptionCatalog.QuotaSessionsMonthly, 5000);
            proPlan.AddQuota(SubscriptionCatalog.QuotaStorageBytes, 50_000_000_000L);
            proPlan.Publish();

            await db.SubscriptionPlans.AddRangeAsync(new[] { starterPlan, proPlan }, ct);
            await db.SaveChangesAsync(ct);

            if (!await db.OrganizationSubscriptions.AnyAsync(s => s.OrganizationId == DemoOrganizationId, ct))
            {
                var now = DateTimeOffset.UtcNow;
                var subscription = new OrganizationSubscription(
                    organizationId: DemoOrganizationId,
                    planId: proPlan.Id,
                    status: SubscriptionStatus.Active,
                    startsAt: new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero),
                    endsAt: new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddYears(1),
                    renewalMode: RenewalMode.SalesManaged);
                await db.OrganizationSubscriptions.AddAsync(subscription, ct);
                await db.SaveChangesAsync(ct);
            }
        }
    }

    private static GlobalProtocol CreateGlobalProtocol(
        string code,
        string name,
        string version,
        string category,
        string sport,
        string description,
        bool published)
    {
        var protocol = new GlobalProtocol(code, name, version, category, sport, description);
        if (published)
        {
            protocol.Publish();
        }

        return protocol;
    }

    private static Protocol CreateProtocol(Guid orgId, string name, string code, string version, string category, string sport, string description)
    {
        var p = new Protocol(orgId, name, code, version, category, sport, description);
        p.Publish();
        return p;
    }
}
