using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KWinHub.Hub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerJobsAndMediaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaProviderId",
                schema: "hub",
                table: "data_assets",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaybackUrl",
                schema: "hub",
                table: "data_assets",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessedAt",
                schema: "hub",
                table: "data_assets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "worker_jobs",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Attempt = table.Column<int>(type: "integer", nullable: false),
                    PayloadJson = table.Column<string>(type: "jsonb", nullable: true),
                    ResultJson = table.Column<string>(type: "jsonb", nullable: true),
                    LeaseExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MaxAttempts = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worker_jobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_worker_jobs_AssetId",
                schema: "hub",
                table: "worker_jobs",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_worker_jobs_OrganizationId",
                schema: "hub",
                table: "worker_jobs",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_worker_jobs_Status_CreatedAt",
                schema: "hub",
                table: "worker_jobs",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "worker_jobs",
                schema: "hub");

            migrationBuilder.DropColumn(
                name: "MediaProviderId",
                schema: "hub",
                table: "data_assets");

            migrationBuilder.DropColumn(
                name: "PlaybackUrl",
                schema: "hub",
                table: "data_assets");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                schema: "hub",
                table: "data_assets");
        }
    }
}
