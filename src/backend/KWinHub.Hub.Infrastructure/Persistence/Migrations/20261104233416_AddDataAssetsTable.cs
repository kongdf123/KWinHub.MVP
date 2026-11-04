using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KWinHub.Hub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDataAssetsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_assets",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrialId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssetType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    MimeType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Size = table.Column<long>(type: "bigint", nullable: false),
                    Checksum = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Revision = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    StorageBucket = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    VerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_assets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_assets_OrganizationId_SessionId",
                schema: "hub",
                table: "data_assets",
                columns: new[] { "OrganizationId", "SessionId" });

            migrationBuilder.CreateIndex(
                name: "IX_data_assets_OrganizationId_SessionId_ExternalId",
                schema: "hub",
                table: "data_assets",
                columns: new[] { "OrganizationId", "SessionId", "ExternalId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_assets",
                schema: "hub");
        }
    }
}
