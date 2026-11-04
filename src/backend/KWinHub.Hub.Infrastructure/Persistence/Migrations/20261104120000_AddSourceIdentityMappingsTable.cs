using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KWinHub.Hub.Infrastructure.Persistence.Migrations
{
    public partial class AddSourceIdentityMappingsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "source_identity_mappings",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceApp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InstallationScope = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConflictReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_source_identity_mappings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_source_identity_mappings_Org_SourceApp_Scope_ExternalId",
                schema: "hub",
                table: "source_identity_mappings",
                columns: new[] { "OrganizationId", "SourceApp", "InstallationScope", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_source_identity_mappings_SubjectId",
                schema: "hub",
                table: "source_identity_mappings",
                column: "SubjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "source_identity_mappings",
                schema: "hub");
        }
    }
}