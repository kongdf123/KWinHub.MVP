using KWinHub.Hub.Groups;
using KWinHub.Hub.Organizations;
using KWinHub.Hub.Protocols;
using KWinHub.Hub.Results;
using KWinHub.Hub.Sessions;
using KWinHub.Hub.Subjects;
using KWinHub.Hub.Teams;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Hub.Infrastructure.Persistence;

/// <summary>
/// EF Core context owning the Hub bounded-context tables (hub schema).
/// OrganizationId/UserId columns are plain identifiers referencing Platform-owned
/// aggregates; per ADR-0001 there are no foreign keys or navigation properties
/// crossing into the platform schema.
/// </summary>
public class HubDbContext : DbContext
{
    public const string SchemaName = "hub";

    public HubDbContext(DbContextOptions<HubDbContext> options)
        : base(options)
    {
    }

    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();

    public DbSet<OrganizationInvitation> OrganizationInvitations => Set<OrganizationInvitation>();

    public DbSet<OrganizationStatus> OrganizationStatuses => Set<OrganizationStatus>();

    public DbSet<OrganizationEventConsumption> OrganizationEventConsumptions => Set<OrganizationEventConsumption>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<SourceIdentityMapping> SourceIdentityMappings => Set<SourceIdentityMapping>();

    public DbSet<Team> Teams => Set<Team>();

    public DbSet<TeamMembership> TeamMemberships => Set<TeamMembership>();

    public DbSet<Group> Groups => Set<Group>();

    public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();

    public DbSet<Protocol> Protocols => Set<Protocol>();

    public DbSet<MetricDefinition> MetricDefinitions => Set<MetricDefinition>();

    public DbSet<DataSource> DataSources => Set<DataSource>();

    public DbSet<Device> Devices => Set<Device>();

    public DbSet<MeasurementSession> MeasurementSessions => Set<MeasurementSession>();

    public DbSet<Trial> Trials => Set<Trial>();

    public DbSet<Result> Results => Set<Result>();

    public DbSet<Metric> Metrics => Set<Metric>();

    public DbSet<IngestionOperation> IngestionOperations => Set<IngestionOperation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<OrganizationMembership>(b =>
        {
            b.ToTable("organization_memberships", SchemaName);
            b.HasKey(m => m.Id);
            b.Property(m => m.Role).IsRequired().HasMaxLength(50);
            b.Property(m => m.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(m => new { m.OrganizationId, m.UserId }).IsUnique();
            b.HasIndex(m => m.UserId);
            // OrganizationId/User_id are cross-context identifiers (platform schema);
            // intentionally no FK constraint.
        });

        modelBuilder.Entity<OrganizationInvitation>(b =>
        {
            b.ToTable("organization_invitations", SchemaName);
            b.HasKey(i => i.Id);
            b.Property(i => i.Email).IsRequired().HasMaxLength(254);
            b.Property(i => i.Role).IsRequired().HasMaxLength(50);
            b.Property(i => i.Token).IsRequired().HasMaxLength(100);
            b.Property(i => i.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(i => i.Token).IsUnique();
            b.HasIndex(i => i.OrganizationId);
            b.HasIndex(i => new { i.OrganizationId, i.Email, i.Status });
        });

        modelBuilder.Entity<OrganizationStatus>(b =>
        {
            b.ToTable("organization_statuses", SchemaName);
            b.HasKey(s => s.Id);
            b.Property(s => s.OrganizationId).IsRequired();
            b.Property(s => s.Status).IsRequired().HasMaxLength(50);
            b.Property(s => s.Version).IsRequired();
            b.HasIndex(s => s.OrganizationId).IsUnique();
        });

        modelBuilder.Entity<OrganizationEventConsumption>(b =>
        {
            b.ToTable("organization_event_consumptions", SchemaName);
            b.HasKey(c => c.Id);
            b.Property(c => c.EventId).IsRequired();
            b.Property(c => c.OrganizationId).IsRequired();
            b.Property(c => c.EventVersion).IsRequired();
            b.Property(c => c.EventType).IsRequired().HasMaxLength(100);
            b.HasIndex(c => c.EventId).IsUnique();
            b.HasIndex(c => new { c.OrganizationId, c.EventVersion }).IsUnique();
        });

        modelBuilder.Entity<Subject>(b =>
        {
            b.ToTable("subjects", SchemaName);
            b.HasKey(s => s.Id);
            b.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
            b.Property(s => s.LastName).IsRequired().HasMaxLength(100);
            b.Property(s => s.SubjectType).HasConversion<string>();
            b.Property(s => s.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(s => s.OrganizationId);
            b.HasIndex(s => new { s.OrganizationId, s.ExternalId });
        });

        modelBuilder.Entity<SourceIdentityMapping>(b =>
        {
            b.ToTable("source_identity_mappings", SchemaName);
            b.HasKey(m => m.Id);
            b.Property(m => m.SourceApp).IsRequired().HasMaxLength(100);
            b.Property(m => m.InstallationScope).IsRequired().HasMaxLength(200);
            b.Property(m => m.ExternalId).IsRequired().HasMaxLength(200);
            b.Property(m => m.Status).IsRequired().HasMaxLength(50);
            b.Property(m => m.ConflictReason).HasMaxLength(500);
            b.HasIndex(m => new { m.OrganizationId, m.SourceApp, m.InstallationScope, m.ExternalId }).IsUnique();
            b.HasIndex(m => m.SubjectId);
        });

        modelBuilder.Entity<Team>(b =>
        {
            b.ToTable("teams", SchemaName);
            b.HasKey(t => t.Id);
            b.Property(t => t.Name).IsRequired().HasMaxLength(200);
            b.Property(t => t.Description).HasMaxLength(1000);
            b.Property(t => t.SportId).HasMaxLength(100);
            b.Property(t => t.Season).HasMaxLength(50);
            b.Property(t => t.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(t => t.OrganizationId);
            b.HasMany(t => t.Memberships)
                .WithOne()
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TeamMembership>(b =>
        {
            b.ToTable("team_memberships", SchemaName);
            b.HasKey(m => m.Id);
            b.Property(m => m.Role).HasMaxLength(50);
            b.HasIndex(m => new { m.TeamId, m.SubjectId }).IsUnique();
            b.HasIndex(m => m.SubjectId);
        });

        modelBuilder.Entity<Group>(b =>
        {
            b.ToTable("groups", SchemaName);
            b.HasKey(g => g.Id);
            b.Property(g => g.Name).IsRequired().HasMaxLength(200);
            b.Property(g => g.Description).HasMaxLength(1000);
            b.Property(g => g.GroupType).HasMaxLength(100);
            b.Property(g => g.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(g => g.OrganizationId);
            b.HasMany(g => g.Memberships)
                .WithOne()
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GroupMembership>(b =>
        {
            b.ToTable("group_memberships", SchemaName);
            b.HasKey(m => m.Id);
            b.Property(m => m.Role).HasMaxLength(50);
            b.HasIndex(m => new { m.GroupId, m.SubjectId }).IsUnique();
            b.HasIndex(m => m.SubjectId);
        });

        modelBuilder.Entity<Protocol>(b =>
        {
            b.ToTable("protocols", SchemaName);
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Code).IsRequired().HasMaxLength(50);
            b.Property(p => p.Status).HasConversion<string>();
            b.HasIndex(p => new { p.OrganizationId, p.Code, p.Version }).IsUnique();
            b.HasIndex(p => p.OrganizationId);
        });

        modelBuilder.Entity<MetricDefinition>(b =>
        {
            b.ToTable("metric_definitions", SchemaName);
            b.HasKey(m => m.Id);
            b.Property(m => m.Code).IsRequired().HasMaxLength(100);
            b.Property(m => m.Name).IsRequired().HasMaxLength(200);
            b.Property(m => m.ValueType).HasConversion<string>();
            b.Property(m => m.Unit).IsRequired().HasMaxLength(50);
            b.Property(m => m.Version).IsRequired().HasMaxLength(50);
            b.Property(m => m.Status).IsRequired().HasMaxLength(50);
            b.Property(m => m.Method).HasMaxLength(200);
            b.Property(m => m.AlgorithmVersion).HasMaxLength(50);
            b.Property(m => m.Category).HasMaxLength(100);
            b.Property(m => m.Description).HasMaxLength(1000);
            b.HasIndex(m => new { m.OrganizationId, m.Code, m.Version }).IsUnique();
            b.HasIndex(m => m.OrganizationId);
        });

        modelBuilder.Entity<DataSource>(b =>
        {
            b.ToTable("data_sources", SchemaName);
            b.HasKey(d => d.Id);
            b.Property(d => d.Name).IsRequired().HasMaxLength(200);
            b.Property(d => d.Code).IsRequired().HasMaxLength(100);
            b.Property(d => d.SourceApp).IsRequired().HasMaxLength(100);
            b.Property(d => d.Version).HasMaxLength(50);
            b.Property(d => d.Description).HasMaxLength(500);
            b.Property(d => d.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(d => new { d.OrganizationId, d.Code }).IsUnique();
            b.HasIndex(d => d.OrganizationId);
        });

        modelBuilder.Entity<Device>(b =>
        {
            b.ToTable("devices", SchemaName);
            b.HasKey(d => d.Id);
            b.Property(d => d.Name).IsRequired().HasMaxLength(200);
            b.Property(d => d.Model).HasMaxLength(100);
            b.Property(d => d.Manufacturer).HasMaxLength(100);
            b.Property(d => d.SerialNumber).HasMaxLength(200);
            b.Property(d => d.FirmwareVersion).HasMaxLength(50);
            b.Property(d => d.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(d => d.OrganizationId);
        });

        modelBuilder.Entity<MeasurementSession>(b =>
        {
            b.ToTable("measurement_sessions", SchemaName);
            b.HasKey(s => s.Id);
            b.Property(s => s.Status).HasConversion<string>();
            b.HasIndex(s => s.OrganizationId);
            b.HasIndex(s => s.SubjectId);
            b.HasIndex(s => s.ProtocolId);
            b.HasIndex(s => new { s.OrganizationId, s.ExternalId }).IsUnique();
        });

        modelBuilder.Entity<Trial>(b =>
        {
            b.ToTable("trials", SchemaName);
            b.HasKey(t => t.Id);
            b.Property(t => t.Status).HasConversion<string>();
            b.Property(t => t.ExternalId).HasMaxLength(200);
            b.Property(t => t.Notes).HasMaxLength(1000);
            b.HasIndex(t => t.OrganizationId);
            b.HasIndex(t => t.SessionId);
            b.HasIndex(t => new { t.SessionId, t.TrialIndex }).IsUnique();
            b.HasIndex(t => new { t.OrganizationId, t.ExternalId });
        });

        modelBuilder.Entity<Result>(b =>
        {
            b.ToTable("results", SchemaName);
            b.HasKey(r => r.Id);
            b.Property(r => r.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(r => r.OrganizationId);
            b.HasIndex(r => r.SessionId);
            b.HasIndex(r => r.ProtocolId);
            b.HasIndex(r => r.TrialId);
            b.HasMany(r => r.Metrics)
                .WithOne()
                .HasForeignKey(m => m.ResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Metric>(b =>
        {
            b.ToTable("metrics", SchemaName);
            b.HasKey(m => m.Id);
            b.Property(m => m.MetricCode).IsRequired().HasMaxLength(100);
            b.Property(m => m.MetricName).IsRequired().HasMaxLength(200);
            b.Property(m => m.ValueType).HasConversion<string>();
            b.Property(m => m.Unit).IsRequired().HasMaxLength(50);
            b.Property(m => m.Side).HasMaxLength(20);
            b.Property(m => m.CategoricalValue).HasMaxLength(200);
            b.Property(m => m.StructuredValue).HasColumnType("jsonb");
            b.HasIndex(m => m.ResultId);
            b.HasIndex(m => m.MetricDefinitionId);
        });

        modelBuilder.Entity<IngestionOperation>(b =>
        {
            b.ToTable("ingestion_operations", SchemaName);
            b.HasKey(o => o.Id);
            b.Property(o => o.ClientOperationId).IsRequired().HasMaxLength(200);
            b.Property(o => o.Status).HasConversion<string>();
            b.Property(o => o.ErrorCode).HasMaxLength(100);
            b.Property(o => o.ErrorDetail).HasMaxLength(1000);
            b.Property(o => o.SessionExternalId).HasMaxLength(200);
            b.HasIndex(o => new { o.OrganizationId, o.ClientOperationId }).IsUnique();
            b.HasIndex(o => o.SessionId);
        });

        base.OnModelCreating(modelBuilder);
    }
}