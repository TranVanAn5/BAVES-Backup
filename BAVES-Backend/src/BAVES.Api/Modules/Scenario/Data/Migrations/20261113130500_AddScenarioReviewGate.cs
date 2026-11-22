using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Scenario.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScenarioReviewGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewComment",
                schema: "scenario",
                table: "ScenarioVersions",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                schema: "scenario",
                table: "ScenarioVersions",
                type: "datetimeoffset(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                schema: "scenario",
                table: "ScenarioVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmittedForReviewAt",
                schema: "scenario",
                table: "ScenarioVersions",
                type: "datetimeoffset(3)",
                precision: 3,
                nullable: true);

            migrationBuilder.DropCheckConstraint(
                name: "CK_ScenarioVersions_Status",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ScenarioVersions_Status",
                schema: "scenario",
                table: "ScenarioVersions",
                sql: "[Status] IN (N'Draft', N'Published', N'Archived', N'PendingReview', N'Rejected')");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioVersions_Status",
                schema: "scenario",
                table: "ScenarioVersions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ScenarioVersions_Status",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ScenarioVersions_Status",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ScenarioVersions_Status",
                schema: "scenario",
                table: "ScenarioVersions",
                sql: "[Status] IN (N'Draft', N'Published', N'Archived')");

            migrationBuilder.DropColumn(
                name: "ReviewComment",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.DropColumn(
                name: "SubmittedForReviewAt",
                schema: "scenario",
                table: "ScenarioVersions");
        }
    }
}
