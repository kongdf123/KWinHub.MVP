using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KWinHub.Hub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerJobQueueName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QueueName",
                schema: "hub",
                table: "worker_jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "media");

            migrationBuilder.CreateIndex(
                name: "IX_worker_jobs_AssetId_JobType",
                schema: "hub",
                table: "worker_jobs",
                columns: new[] { "AssetId", "JobType" });

            migrationBuilder.CreateIndex(
                name: "IX_worker_jobs_QueueName_Status_CreatedAt",
                schema: "hub",
                table: "worker_jobs",
                columns: new[] { "QueueName", "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_worker_jobs_AssetId_JobType",
                schema: "hub",
                table: "worker_jobs");

            migrationBuilder.DropIndex(
                name: "IX_worker_jobs_QueueName_Status_CreatedAt",
                schema: "hub",
                table: "worker_jobs");

            migrationBuilder.DropColumn(
                name: "QueueName",
                schema: "hub",
                table: "worker_jobs");
        }
    }
}