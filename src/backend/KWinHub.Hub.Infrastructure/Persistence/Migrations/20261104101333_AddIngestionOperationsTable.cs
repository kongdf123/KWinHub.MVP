using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KWinHub.Hub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIngestionOperationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                schema: "hub",
                table: "metrics");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                schema: "hub",
                table: "metrics",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Side",
                schema: "hub",
                table: "metrics",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MetricCode",
                schema: "hub",
                table: "metrics",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "CategoricalValue",
                schema: "hub",
                table: "metrics",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LeftValue",
                schema: "hub",
                table: "metrics",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MetricDefinitionId",
                schema: "hub",
                table: "metrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RightValue",
                schema: "hub",
                table: "metrics",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ScalarValue",
                schema: "hub",
                table: "metrics",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StructuredValue",
                schema: "hub",
                table: "metrics",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ValueType",
                schema: "hub",
                table: "metrics",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "data_sources",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceApp = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_sources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SerialNumber = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FirmwareVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ingestion_operations",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientOperationId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ErrorCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ErrorDetail = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingestion_operations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "metric_definitions",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ValueType = table.Column<string>(type: "text", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Method = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AlgorithmVersion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metric_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "trials",
                schema: "hub",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TrialIndex = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_results_TrialId",
                schema: "hub",
                table: "results",
                column: "TrialId");

            migrationBuilder.CreateIndex(
                name: "IX_metrics_MetricDefinitionId",
                schema: "hub",
                table: "metrics",
                column: "MetricDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_data_sources_OrganizationId",
                schema: "hub",
                table: "data_sources",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_data_sources_OrganizationId_Code",
                schema: "hub",
                table: "data_sources",
                columns: new[] { "OrganizationId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_devices_OrganizationId",
                schema: "hub",
                table: "devices",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_operations_OrganizationId_ClientOperationId",
                schema: "hub",
                table: "ingestion_operations",
                columns: new[] { "OrganizationId", "ClientOperationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ingestion_operations_SessionId",
                schema: "hub",
                table: "ingestion_operations",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_metric_definitions_OrganizationId",
                schema: "hub",
                table: "metric_definitions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_metric_definitions_OrganizationId_Code_Version",
                schema: "hub",
                table: "metric_definitions",
                columns: new[] { "OrganizationId", "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_trials_OrganizationId",
                schema: "hub",
                table: "trials",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_trials_OrganizationId_ExternalId",
                schema: "hub",
                table: "trials",
                columns: new[] { "OrganizationId", "ExternalId" });

            migrationBuilder.CreateIndex(
                name: "IX_trials_SessionId",
                schema: "hub",
                table: "trials",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_trials_SessionId_TrialIndex",
                schema: "hub",
                table: "trials",
                columns: new[] { "SessionId", "TrialIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_sources",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "devices",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "ingestion_operations",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "metric_definitions",
                schema: "hub");

            migrationBuilder.DropTable(
                name: "trials",
                schema: "hub");

            migrationBuilder.DropIndex(
                name: "IX_results_TrialId",
                schema: "hub",
                table: "results");

            migrationBuilder.DropIndex(
                name: "IX_metrics_MetricDefinitionId",
                schema: "hub",
                table: "metrics");

            migrationBuilder.DropColumn(
                name: "CategoricalValue",
                schema: "hub",
                table: "metrics");

            migrationBuilder.DropColumn(
                name: "LeftValue",
                schema: "hub",
                table: "metrics");

            migrationBuilder.DropColumn(
                name: "MetricDefinitionId",
                schema: "hub",
                table: "metrics");

            migrationBuilder.DropColumn(
                name: "RightValue",
                schema: "hub",
                table: "metrics");

            migrationBuilder.DropColumn(
                name: "ScalarValue",
                schema: "hub",
                table: "metrics");

            migrationBuilder.DropColumn(
                name: "StructuredValue",
                schema: "hub",
                table: "metrics");

            migrationBuilder.DropColumn(
                name: "ValueType",
                schema: "hub",
                table: "metrics");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                schema: "hub",
                table: "metrics",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Side",
                schema: "hub",
                table: "metrics",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MetricCode",
                schema: "hub",
                table: "metrics",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<double>(
                name: "Value",
                schema: "hub",
                table: "metrics",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
