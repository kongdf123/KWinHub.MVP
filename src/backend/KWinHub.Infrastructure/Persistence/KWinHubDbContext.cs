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

        modelBuilder.Entity<Subject>(b =>
        {
            b.ToTable("subjects");
            b.HasKey(s => s.Id);
            b.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
            b.Property(s => s.LastName).IsRequired().HasMaxLength(100);
            b.Property(s => s.SubjectType).HasConversion<string>();
            b.HasIndex(s => s.OrganizationId);
        });

        modelBuilder.Entity<Protocol>(b =>
        {
            b.ToTable("protocols");
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Code).IsRequired().HasMaxLength(50);
            b.Property(p => p.Status).HasConversion<string>();
            b.HasIndex(p => new { p.OrganizationId, p.Code, p.Version }).IsUnique();
        });

        modelBuilder.Entity<MeasurementSession>(b =>
        {
            b.ToTable("measurement_sessions");
            b.HasKey(s => s.Id);
            b.Property(s => s.Status).HasConversion<string>();
            b.HasIndex(s => s.OrganizationId);
            b.HasIndex(s => s.SubjectId);
        });

        modelBuilder.Entity<Result>(b =>
        {
            b.ToTable("results");
            b.HasKey(r => r.Id);
            b.HasMany(r => r.Metrics)
                .WithOne()
                .HasForeignKey(m => m.ResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Metric>(b =>
        {
            b.ToTable("metrics");
            b.HasKey(m => m.Id);
            b.Property(m => m.Unit).IsRequired().HasMaxLength(20);
        });

        base.OnModelCreating(modelBuilder);
    }
}