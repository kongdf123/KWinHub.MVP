using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KWinHub.Hub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hub");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "measurement_sessions",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocolVersion = table.Column<string>(type: "text", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Timezone = table.Column<string>(type: "text", nullable: true),
                    ClientApplication = table.Column<string>(type: "text", nullable: true),
                    ClientVersion = table.Column<string>(type: "text", nullable: true),
                    ExternalId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_measurement_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization_event_consumptions",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventVersion = table.Column<long>(type: "bigint", nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConsumedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_event_consumptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization_memberships",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InvitedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AcceptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_memberships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "organization_statuses",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    EffectiveFrom = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_statuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "protocols",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    Sport = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocols", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "results",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrialId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocolVersion = table.Column<string>(type: "text", nullable: false),
                    AlgorithmVersion = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QualityStatus = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_results", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: true),
                    SubjectType = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Sex = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "teams",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SportId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Season = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "metrics",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetricName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Side = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_metrics_results_ResultId",
                        column: x => x.ResultId,
                        principalSchema: "hub",
                        principalTable: "results",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "team_memberships",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_memberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_team_memberships_teams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hub",
                        principalTable: "teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_measurement_sessions_OrganizationId",
                schema: "hub",
                table: "measurement_sessions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_measurement_sessions_OrganizationId_ExternalId",
                schema: "hub",
                table: "measurement_sessions",
                columns: new[] { "OrganizationId", "ExternalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_measurement_sessions_ProtocolId",
                schema: "hub",
                table: "measurement_sessions",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_measurement_sessions_SubjectId",
                schema: "hub",
                table: "measurement_sessions",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_metrics_ResultId",
                schema: "hub",
                table: "metrics",
                column: "ResultId");

            migrationBuilder.CreateIndex(
                name: "IX_organization_event_consumptions_EventId",
                schema: "hub",
                table: "organization_event_consumptions",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_event_consumptions_OrganizationId_EventVersion",
                schema: "hub",
                table: "organization_event_consumptions",
                columns: new[] { "OrganizationId", "EventVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_memberships_OrganizationId_UserId",
                schema: "hub",
                table: "organization_memberships",
                columns: new[] { "OrganizationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_memberships_UserId",
                schema: "hub",
                table: "organization_memberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_organization_statuses_OrganizationId",
                schema: "hub",
                table: "organization_statuses",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_protocols_OrganizationId",
                schema: "hub",
                table: "protocols",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_protocols_OrganizationId_Code_Version",
                schema: "hub",
                table: "protocols",
                columns: new[] { "OrganizationId", "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_results_OrganizationId",
                schema: "hub",
                table: "results",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_results_ProtocolId",
                schema: "hub",
                table: "results",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_results_SessionId",
                schema: "hub",
                table: "results",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_OrganizationId",
                schema: "hub",
                table: "subjects",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_OrganizationId_ExternalId",
                schema: "hub",
                table: "subjects",
                columns: new[] { "OrganizationId", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_team_memberships_SubjectId",
                schema: "hub",
                table: "team_memberships",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_team_memberships_TeamId_SubjectId",
                schema: "hub",
                table: "team_memberships",
                columns: new[] { "TeamId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teams_OrganizationId",
                schema: "hub",
                table: "teams",
                column: "OrganizationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "measurement_sessions",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "metrics",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "organization_event_consumptions",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "organization_memberships",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "organization_statuses",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "protocols",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "subjects",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "team_memberships",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "results",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "teams",
                schema: "hub");
        }
    }
}
