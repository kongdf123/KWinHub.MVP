using KWinHub.Platform.Audit;
using KWinHub.Platform.Catalogs;
using KWinHub.Platform.Organizations;
using KWinHub.Platform.Subscriptions;
using KWinHub.Platform.Users;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Platform.Infrastructure.Persistence;

/// <summary>
/// EF Core context owning the Platform bounded-context tables (platform schema).
/// Per ADR-0001 this context must never map Hub-owned tables
/// (memberships, subjects, protocols, sessions, results, metrics).
/// </summary>
public class PlatformDbContext : DbContext
{
    public const string SchemaName = "platform";

    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<User> Users => Set<User>();

    public DbSet<EmailVerificationToken> EmailVerificationTokens => Set<EmailVerificationToken>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<GlobalProtocol> GlobalProtocols => Set<GlobalProtocol>();

    public DbSet<GlobalMetric> GlobalMetrics => Set<GlobalMetric>();

    public DbSet<DeviceModel> DeviceModels => Set<DeviceModel>();

    public DbSet<AlgorithmVersion> AlgorithmVersions => Set<AlgorithmVersion>();

    public DbSet<FeatureDefinition> FeatureDefinitions => Set<FeatureDefinition>();

    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();

    public DbSet<OrganizationSubscription> OrganizationSubscriptions => Set<OrganizationSubscription>();

    public DbSet<OrganizationFeatureOverride> OrganizationFeatureOverrides => Set<OrganizationFeatureOverride>();

    public DbSet<OrganizationQuotaOverride> OrganizationQuotaOverrides => Set<OrganizationQuotaOverride>();

    public DbSet<UsageLedgerEntry> UsageLedgerEntries => Set<UsageLedgerEntry>();

    public DbSet<UsageAggregate> UsageAggregates => Set<UsageAggregate>();

    public DbSet<SubscriptionAuditEvent> SubscriptionAuditEvents => Set<SubscriptionAuditEvent>();

    public DbSet<OrganizationLifecycleEvent> OrganizationLifecycleEvents => Set<OrganizationLifecycleEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Organization>(b =>
        {
            b.ToTable("organizations", SchemaName);
            b.HasKey(o => o.Id);
            b.Property(o => o.Name).IsRequired().HasMaxLength(200);
            b.Property(o => o.Slug).IsRequired().HasMaxLength(100);
            b.HasIndex(o => o.Slug).IsUnique();
            b.Property(o => o.Status).HasConversion<string>();
            b.Property(o => o.Type).HasConversion<string>().HasMaxLength(20);
            b.Property(o => o.CountryCode).HasMaxLength(2);
            b.Property(o => o.Region).HasMaxLength(100);
            b.Property(o => o.Timezone).HasMaxLength(100);
            b.Property(o => o.Locale).HasMaxLength(20);
            b.HasIndex(o => o.Type);
            b.HasIndex(o => o.Status);
            b.HasIndex(o => o.CreatedAt);
        });

        modelBuilder.Entity<User>(b =>
        {
            b.ToTable("users", SchemaName);
            b.HasKey(u => u.Id);
            b.Property(u => u.Email).IsRequired().HasMaxLength(256);
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.PasswordHash).IsRequired();
            b.Property(u => u.DisplayName).IsRequired().HasMaxLength(200);
            b.Property(u => u.Status).HasConversion<string>().HasMaxLength(40);
            b.Property(u => u.PlatformRole).HasMaxLength(50);
            b.Property(u => u.PreferredLanguage).IsRequired().HasMaxLength(20);
            b.Property(u => u.SecurityStamp).IsRequired().HasMaxLength(64);
            b.HasIndex(u => u.Status);
        });

        modelBuilder.Entity<EmailVerificationToken>(b =>
        {
            b.ToTable("email_verification_tokens", SchemaName);
            b.HasKey(t => t.Id);
            b.Property(t => t.TokenHash).IsRequired().HasMaxLength(128);
            b.HasIndex(t => t.TokenHash).IsUnique();
            b.HasIndex(t => t.UserId);
            b.Ignore(t => t.IsUsable);
        });

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("audit_logs", SchemaName);
            b.HasKey(a => a.Id);
            b.Property(a => a.Action).IsRequired().HasMaxLength(100);
            b.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
            b.Property(a => a.EntityId).HasMaxLength(100);
            b.Property(a => a.Summary).IsRequired().HasMaxLength(500);
            b.Property(a => a.ActorEmail).HasMaxLength(256);
            b.Property(a => a.IpAddress).HasMaxLength(64);
            b.HasIndex(a => a.CreatedAt);
            b.HasIndex(a => a.EntityType);
            b.HasIndex(a => a.ActorUserId);
        });

        modelBuilder.Entity<GlobalProtocol>(b =>
        {
            b.ToTable("global_protocols", SchemaName);
            b.HasKey(p => p.Id);
            b.Property(p => p.Code).IsRequired().HasMaxLength(50);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Version).IsRequired().HasMaxLength(20);
            b.Property(p => p.Category).HasMaxLength(100);
            b.Property(p => p.Sport).HasMaxLength(100);
            b.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(p => new { p.Code, p.Version }).IsUnique();
        });

        modelBuilder.Entity<GlobalMetric>(b =>
        {
            b.ToTable("global_metrics", SchemaName);
            b.HasKey(m => m.Id);
            b.Property(m => m.Code).IsRequired().HasMaxLength(50);
            b.HasIndex(m => m.Code).IsUnique();
            b.Property(m => m.Name).IsRequired().HasMaxLength(200);
            b.Property(m => m.Unit).IsRequired().HasMaxLength(20);
            b.Property(m => m.Category).HasMaxLength(100);
            b.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<DeviceModel>(b =>
        {
            b.ToTable("device_models", SchemaName);
            b.HasKey(d => d.Id);
            b.Property(d => d.Manufacturer).IsRequired().HasMaxLength(100);
            b.Property(d => d.Model).IsRequired().HasMaxLength(100);
            b.Property(d => d.DeviceType).IsRequired().HasMaxLength(50);
            b.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(d => new { d.Manufacturer, d.Model }).IsUnique();
        });

        modelBuilder.Entity<AlgorithmVersion>(b =>
        {
            b.ToTable("algorithm_versions", SchemaName);
            b.HasKey(a => a.Id);
            b.Property(a => a.Name).IsRequired().HasMaxLength(100);
            b.Property(a => a.Version).IsRequired().HasMaxLength(20);
            b.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(a => new { a.Name, a.Version }).IsUnique();
        });

        // ---- Subscriptions (frozen per PRD D-08; relocated wholesale into Platform) ----

        modelBuilder.Entity<FeatureDefinition>(b =>
        {
            b.ToTable("feature_definitions", SchemaName);
            b.HasKey(f => f.Id);
            b.Property(f => f.Key).IsRequired().HasMaxLength(100);
            b.HasIndex(f => f.Key).IsUnique();
            b.Property(f => f.Category).IsRequired().HasMaxLength(100);
            b.Property(f => f.DisplayName).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<SubscriptionPlan>(b =>
        {
            b.ToTable("subscription_plans", SchemaName);
            b.HasKey(p => p.Id);
            b.Property(p => p.Code).IsRequired().HasMaxLength(100);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
            b.Property(p => p.Description).HasMaxLength(1000);
            b.HasIndex(p => new { p.Code, p.Version }).IsUnique();
            b.HasMany(p => p.Features)
                .WithOne()
                .HasForeignKey(f => f.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasMany(p => p.Quotas)
                .WithOne()
                .HasForeignKey(q => q.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlanFeature>(b =>
        {
            b.ToTable("plan_features", SchemaName);
            b.HasKey(f => new { f.PlanId, f.FeatureKey });
            b.Property(f => f.FeatureKey).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<PlanQuota>(b =>
        {
            b.ToTable("plan_quotas", SchemaName);
            b.HasKey(q => new { q.PlanId, q.QuotaKey });
            b.Property(q => q.QuotaKey).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<OrganizationSubscription>(b =>
        {
            b.ToTable("organization_subscriptions", SchemaName);
            b.HasKey(s => s.Id);
            b.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
            b.Property(s => s.RenewalMode).HasConversion<string>().HasMaxLength(20);
            b.Property(s => s.Currency).HasMaxLength(3);
            b.Property(s => s.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate();
            b.Property(s => s.Revision).HasDefaultValue(1);
            b.HasIndex(s => new { s.OrganizationId, s.StartsAt, s.EndsAt });
        });

        modelBuilder.Entity<OrganizationFeatureOverride>(b =>
        {
            b.ToTable("organization_feature_overrides", SchemaName);
            b.HasKey(o => o.Id);
            b.Property(o => o.FeatureKey).IsRequired().HasMaxLength(100);
            b.Property(o => o.Reason).IsRequired().HasMaxLength(500);
            b.HasIndex(o => new { o.OrganizationId, o.FeatureKey, o.StartsAt, o.EndsAt });
        });

        modelBuilder.Entity<OrganizationQuotaOverride>(b =>
        {
            b.ToTable("organization_quota_overrides", SchemaName);
            b.HasKey(o => o.Id);
            b.Property(o => o.QuotaKey).IsRequired().HasMaxLength(100);
            b.Property(o => o.Reason).IsRequired().HasMaxLength(500);
            b.HasIndex(o => new { o.OrganizationId, o.QuotaKey, o.StartsAt, o.EndsAt });
        });

        modelBuilder.Entity<UsageLedgerEntry>(b =>
        {
            b.ToTable("usage_ledger", SchemaName);
            b.HasKey(l => l.Id);
            b.Property(l => l.MetricKey).IsRequired().HasMaxLength(100);
            b.Property(l => l.BusinessEventId).IsRequired().HasMaxLength(200);
            b.HasIndex(l => new { l.OrganizationId, l.MetricKey, l.BusinessEventId }).IsUnique();
        });

        modelBuilder.Entity<UsageAggregate>(b =>
        {
            b.ToTable("usage_aggregates", SchemaName);
            b.HasKey(a => a.Id);
            b.Property(a => a.MetricKey).IsRequired().HasMaxLength(100);
            b.HasIndex(a => new { a.OrganizationId, a.MetricKey, a.PeriodStart }).IsUnique();
        });

        modelBuilder.Entity<SubscriptionAuditEvent>(b =>
        {
            b.ToTable("subscription_audit_events", SchemaName);
            b.HasKey(e => e.Id);
            b.Property(e => e.Action).IsRequired().HasMaxLength(100);
            b.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            b.Property(e => e.BeforeJson).HasColumnType("jsonb");
            b.Property(e => e.AfterJson).HasColumnType("jsonb");
            b.Property(e => e.RequestId).HasMaxLength(100);
            b.HasIndex(e => new { e.OrganizationId, e.OccurredAt });
        });

        modelBuilder.Entity<OrganizationLifecycleEvent>(b =>
        {
            b.ToTable("organization_lifecycle_events", SchemaName);
            b.HasKey(e => e.Id);
            b.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            b.Property(e => e.Status).IsRequired().HasMaxLength(20);
            b.Property(e => e.PayloadJson).IsRequired().HasColumnType("jsonb");
            b.Property(e => e.LastError).HasMaxLength(1000);
            b.HasIndex(e => new { e.Status, e.CreatedAt });
            b.HasIndex(e => new { e.OrganizationId, e.EventVersion }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}