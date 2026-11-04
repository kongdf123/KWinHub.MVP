using KWinHub.Hub.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KWinHub.Hub.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", HubDbContext.SchemaName);

        builder.HasKey(m => m.Id);

        builder.Property(m => m.EventId).IsRequired();
        builder.Property(m => m.EventType).IsRequired().HasMaxLength(200);
        builder.Property(m => m.OrganizationId).IsRequired();
        builder.Property(m => m.AggregateType).HasMaxLength(100);
        builder.Property(m => m.EventVersion).IsRequired();
        builder.Property(m => m.PayloadJson).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.Status).IsRequired().HasMaxLength(50);
        builder.Property(m => m.RetryCount).IsRequired();
        builder.Property(m => m.ProcessedAt);
        builder.Property(m => m.LeaseExpiresAt);
        builder.Property(m => m.LastError).HasMaxLength(1000);

        builder.HasIndex(m => m.EventId).IsUnique();
        builder.HasIndex(m => new { m.Status, m.CreatedAt });
        builder.HasIndex(m => new { m.OrganizationId, m.AggregateType, m.AggregateId });
    }
}