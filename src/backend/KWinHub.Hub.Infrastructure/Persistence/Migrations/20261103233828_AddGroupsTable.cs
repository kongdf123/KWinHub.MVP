using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KWinHub.Hub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "groups",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    GroupType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "group_memberships",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_group_memberships_groups_GroupId",
                        column: x => x.GroupId,
                        principalSchema: "hub",
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_group_memberships_GroupId_SubjectId",
                schema: "hub",
                table: "group_memberships",
                columns: new[] { "GroupId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_memberships_SubjectId",
                schema: "hub",
                table: "group_memberships",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_groups_OrganizationId",
                schema: "hub",
                table: "groups",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_memberships",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "groups",
                schema: "hub");
        }
    }
}
