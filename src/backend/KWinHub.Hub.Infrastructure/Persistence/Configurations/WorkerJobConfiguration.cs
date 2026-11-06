using KWinHub.Hub.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KWinHub.Hub.Infrastructure.Persistence.Configurations;

public class WorkerJobConfiguration : IEntityTypeConfiguration<WorkerJob>
{
    public void Configure(EntityTypeBuilder<WorkerJob> builder)
    {
        builder.ToTable("worker_jobs", HubDbContext.SchemaName);

        builder.HasKey(j => j.Id);

        builder.Property(j => j.AssetId).IsRequired();
        builder.Property(j => j.OrganizationId).IsRequired();
        builder.Property(j => j.QueueName).IsRequired().HasMaxLength(50);
        builder.Property(j => j.JobType).IsRequired().HasMaxLength(100);
        builder.Property(j => j.Status).IsRequired().HasMaxLength(50);
        builder.Property(j => j.Attempt).IsRequired();
        builder.Property(j => j.MaxAttempts).IsRequired();
        builder.Property(j => j.PayloadJson).HasColumnType("jsonb");
        builder.Property(j => j.ResultJson).HasColumnType("jsonb");
        builder.Property(j => j.LeaseExpiresAt);
        builder.Property(j => j.LastError).HasMaxLength(1000);
        builder.Property(j => j.CompletedAt);

        builder.HasIndex(j => new { j.Status, j.CreatedAt });
        builder.HasIndex(j => new { j.QueueName, j.Status, j.CreatedAt });
        builder.HasIndex(j => j.AssetId);
        builder.HasIndex(j => j.OrganizationId);
        builder.HasIndex(j => new { j.AssetId, j.JobType });
    }
}