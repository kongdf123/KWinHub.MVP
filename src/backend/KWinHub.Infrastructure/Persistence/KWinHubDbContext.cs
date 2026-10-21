using KWinHub.Domain.Organizations;
using KWinHub.Domain.Protocols;
using KWinHub.Domain.Results;
using KWinHub.Domain.Sessions;
using KWinHub.Domain.Subjects;
using Microsoft.EntityFrameworkCore;

namespace KWinHub.Infrastructure.Persistence;

public class KWinHubDbContext : DbContext
{
    public KWinHubDbContext(DbContextOptions<KWinHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<Protocol> Protocols => Set<Protocol>();

    public DbSet<MeasurementSession> MeasurementSessions => Set<MeasurementSession>();

    public DbSet<Result> Results => Set<Result>();

    public DbSet<Metric> Metrics => Set<Metric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Organization>(b =>
        {
            b.ToTable("organizations");
            b.HasKey(o => o.Id);
            b.Property(o => o.Name).IsRequired().HasMaxLength(200);
            b.Property(o => o.Slug).IsRequired().HasMaxLength(100);
            b.HasIndex(o => o.Slug).IsUnique();
            b.Property(o => o.Status).HasConversion<string>();
        });

        modelBuilder.Entity<OrganizationMembership>(b =>
        {
            b.ToTable("organization_memberships");
            b.HasKey(m => m.Id);
            b.Property(m => m.Role).IsRequired().HasMaxLength(50);
            b.Property(m => m.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(m => new { m.OrganizationId, m.UserId }).IsUnique();
            b.HasIndex(m => m.UserId);
        });

        modelBuilder.Entity<Subject>(b =>
        {
            b.ToTable("subjects");
            b.HasKey(s => s.Id);
            b.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
            b.Property(s => s.LastName).IsRequired().HasMaxLength(100);
            b.Property(s => s.SubjectType).HasConversion<string>();
            b.Property(s => s.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(s => s.OrganizationId);
            b.HasIndex(s => new { s.OrganizationId, s.ExternalId });
        });

        modelBuilder.Entity<Protocol>(b =>
        {
            b.ToTable("protocols");
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Code).IsRequired().HasMaxLength(50);
            b.Property(p => p.Status).HasConversion<string>();
            b.HasIndex(p => new { p.OrganizationId, p.Code, p.Version }).IsUnique();
            b.HasIndex(p => p.OrganizationId);
        });

        modelBuilder.Entity<MeasurementSession>(b =>
        {
            b.ToTable("measurement_sessions");
            b.HasKey(s => s.Id);
            b.Property(s => s.Status).HasConversion<string>();
            b.HasIndex(s => s.OrganizationId);
            b.HasIndex(s => s.SubjectId);
            b.HasIndex(s => s.ProtocolId);
            b.HasIndex(s => new { s.OrganizationId, s.ExternalId });
        });

        modelBuilder.Entity<Result>(b =>
        {
            b.ToTable("results");
            b.HasKey(r => r.Id);
            b.Property(r => r.Status).IsRequired().HasMaxLength(50);
            b.HasIndex(r => r.OrganizationId);
            b.HasIndex(r => r.SessionId);
            b.HasIndex(r => r.ProtocolId);
            b.HasMany(r => r.Metrics)
                .WithOne()
                .HasForeignKey(m => m.ResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Metric>(b =>
        {
            b.ToTable("metrics");
            b.HasKey(m => m.Id);
            b.Property(m => m.MetricCode).IsRequired().HasMaxLength(50);
            b.Property(m => m.MetricName).IsRequired().HasMaxLength(200);
            b.Property(m => m.Unit).IsRequired().HasMaxLength(20);
        });

        base.OnModelCreating(modelBuilder);
    }
}