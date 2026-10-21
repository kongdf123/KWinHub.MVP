using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace KWinHub.Infrastructure.Migrations;

public partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("KWinHub.Domain.Organizations.Organization", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<DateTimeOffset>("CreatedAt");
            b.Property<string>("Description");
            b.Property<string>("Name").IsRequired().HasMaxLength(200);
            b.Property<string>("Slug").IsRequired().HasMaxLength(100);
            b.Property<string>("Status");
            b.Property<DateTimeOffset?>("UpdatedAt");
            b.HasKey("Id");
            b.ToTable("organizations");
            b.HasIndex("Slug").IsUnique();
        });

        modelBuilder.Entity("KWinHub.Domain.Organizations.OrganizationMembership", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<DateTimeOffset?>("AcceptedAt");
            b.Property<DateTimeOffset>("CreatedAt");
            b.Property<DateTimeOffset?>("InvitedAt");
            b.Property<Guid>("OrganizationId");
            b.Property<string>("Role").IsRequired().HasMaxLength(50);
            b.Property<string>("Status").IsRequired().HasMaxLength(50);
            b.Property<Guid>("UserId");
            b.Property<DateTimeOffset?>("UpdatedAt");
            b.HasKey("Id");
            b.ToTable("organization_memberships");
            b.HasIndex("OrganizationId", "UserId").IsUnique();
            b.HasIndex("UserId");
        });

        modelBuilder.Entity("KWinHub.Domain.Protocols.Protocol", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<string>("Category");
            b.Property<string>("Code").IsRequired().HasMaxLength(50);
            b.Property<DateTimeOffset>("CreatedAt");
            b.Property<string>("Description");
            b.Property<string>("Name").IsRequired().HasMaxLength(200);
            b.Property<Guid>("OrganizationId");
            b.Property<string>("Sport");
            b.Property<string>("Status");
            b.Property<DateTimeOffset?>("UpdatedAt");
            b.Property<string>("Version");
            b.HasKey("Id");
            b.ToTable("protocols");
            b.HasIndex("OrganizationId");
            b.HasIndex("OrganizationId", "Code", "Version").IsUnique();
        });

        modelBuilder.Entity("KWinHub.Domain.Sessions.MeasurementSession", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<DateTimeOffset>("CollectedAt");
            b.Property<DateTimeOffset>("CreatedAt");
            b.Property<string>("DeviceId");
            b.Property<string>("ExternalId");
            b.Property<Guid>("OrganizationId");
            b.Property<Guid?>("ProtocolId");
            b.Property<string>("Status");
            b.Property<Guid>("SubjectId");
            b.Property<DateTimeOffset?>("UpdatedAt");
            b.HasKey("Id");
            b.ToTable("measurement_sessions");
            b.HasIndex("OrganizationId");
            b.HasIndex("OrganizationId", "ExternalId");
            b.HasIndex("ProtocolId");
            b.HasIndex("SubjectId");
        });

        modelBuilder.Entity("KWinHub.Domain.Results.Result", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<string>("AlgorithmVersion");
            b.Property<DateTimeOffset>("CreatedAt");
            b.Property<Guid>("OrganizationId");
            b.Property<Guid?>("ProtocolId");
            b.Property<string>("ProtocolVersion");
            b.Property<DateTimeOffset?>("PublishedAt");
            b.Property<Guid>("SessionId");
            b.Property<string>("Status").IsRequired().HasMaxLength(50);
            b.Property<DateTimeOffset?>("UpdatedAt");
            b.HasKey("Id");
            b.ToTable("results");
            b.HasIndex("OrganizationId");
            b.HasIndex("ProtocolId");
            b.HasIndex("SessionId");
        });

        modelBuilder.Entity("KWinHub.Domain.Subjects.Subject", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<DateTimeOffset>("CreatedAt");
            b.Property<DateTimeOffset?>("DateOfBirth");
            b.Property<string>("DisplayName");
            b.Property<string>("ExternalId");
            b.Property<string>("FirstName").IsRequired().HasMaxLength(100);
            b.Property<Guid>("OrganizationId");
            b.Property<string>("Sex");
            b.Property<string>("Status").IsRequired().HasMaxLength(50);
            b.Property<string>("SubjectType");
            b.Property<string>("LastName").IsRequired().HasMaxLength(100);
            b.Property<DateTimeOffset?>("UpdatedAt");
            b.HasKey("Id");
            b.ToTable("subjects");
            b.HasIndex("OrganizationId");
            b.HasIndex("OrganizationId", "ExternalId");
        });

        modelBuilder.Entity("KWinHub.Domain.Results.Metric", b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<DateTimeOffset>("CreatedAt");
            b.Property<string>("MetricCode").IsRequired().HasMaxLength(50);
            b.Property<string>("MetricName").IsRequired().HasMaxLength(200);
            b.Property<Guid>("ResultId");
            b.Property<string>("Side");
            b.Property<string>("Unit").IsRequired().HasMaxLength(20);
            b.Property<DateTimeOffset?>("UpdatedAt");
            b.Property<double>("Value");
            b.HasKey("Id");
            b.ToTable("metrics");
            b.HasIndex("ResultId");
        });

        modelBuilder.Entity("KWinHub.Domain.Organizations.OrganizationMembership", b =>
        {
            b.HasOne("KWinHub.Domain.Organizations.Organization", null!)
                .WithMany()
                .HasForeignKey("OrganizationId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("KWinHub.Domain.Protocols.Protocol", b =>
        {
            b.HasOne("KWinHub.Domain.Organizations.Organization", null!)
                .WithMany()
                .HasForeignKey("OrganizationId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("KWinHub.Domain.Subjects.Subject", b =>
        {
            b.HasOne("KWinHub.Domain.Organizations.Organization", null!)
                .WithMany()
                .HasForeignKey("OrganizationId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("KWinHub.Domain.Sessions.MeasurementSession", b =>
        {
            b.HasOne("KWinHub.Domain.Organizations.Organization", null!)
                .WithMany()
                .HasForeignKey("OrganizationId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("KWinHub.Domain.Protocols.Protocol", null!)
                .WithMany()
                .HasForeignKey("ProtocolId")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne("KWinHub.Domain.Subjects.Subject", null!)
                .WithMany()
                .HasForeignKey("SubjectId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("KWinHub.Domain.Results.Result", b =>
        {
            b.HasOne("KWinHub.Domain.Organizations.Organization", null!)
                .WithMany()
                .HasForeignKey("OrganizationId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("KWinHub.Domain.Protocols.Protocol", null!)
                .WithMany()
                .HasForeignKey("ProtocolId")
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne("KWinHub.Domain.Sessions.MeasurementSession", null!)
                .WithMany()
                .HasForeignKey("SessionId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity("KWinHub.Domain.Results.Metric", b =>
        {
            b.HasOne("KWinHub.Domain.Results.Result", null!)
                .WithMany("Metrics")
                .HasForeignKey("ResultId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });
    }
}