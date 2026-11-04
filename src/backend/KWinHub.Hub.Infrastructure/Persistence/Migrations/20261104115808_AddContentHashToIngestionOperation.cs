using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KWinHub.Hub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddContentHashToIngestionOperation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentHash",
                schema: "hub",
                table: "ingestion_operations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentHash",
                schema: "hub",
                table: "ingestion_operations");
        }
    }
}
