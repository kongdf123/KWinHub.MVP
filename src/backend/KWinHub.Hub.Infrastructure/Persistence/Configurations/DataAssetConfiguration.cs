using KWinHub.Hub.Assets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KWinHub.Hub.Infrastructure.Persistence.Configurations;

public class DataAssetConfiguration : IEntityTypeConfiguration<DataAsset>
{
    public void Configure(EntityTypeBuilder<DataAsset> builder)
    {
        builder.ToTable("data_assets", HubDbContext.SchemaName);

        builder.HasKey(a => a.Id);

        builder.Property(a => a.OrganizationId).IsRequired();
        builder.Property(a => a.SessionId).IsRequired();
        builder.Property(a => a.TrialId);
        builder.Property(a => a.AssetType).IsRequired().HasMaxLength(64);
        builder.Property(a => a.ExternalId).HasMaxLength(256);
        builder.Property(a => a.MimeType).HasMaxLength(128);
        builder.Property(a => a.Size).IsRequired();
        builder.Property(a => a.Checksum).HasMaxLength(128);
        builder.Property(a => a.Revision).IsRequired();
        builder.Property(a => a.Status).IsRequired().HasConversion<string>();
        builder.Property(a => a.StorageKey).HasMaxLength(512);
        builder.Property(a => a.StorageBucket).HasMaxLength(128);
        builder.Property(a => a.UploadedAt);
        builder.Property(a => a.VerifiedAt);
        builder.Property(a => a.FailureReason).HasMaxLength(1024);

        builder.HasIndex(a => new { a.OrganizationId, a.SessionId });
        builder.HasIndex(a => new { a.OrganizationId, a.SessionId, a.ExternalId });
    }
}