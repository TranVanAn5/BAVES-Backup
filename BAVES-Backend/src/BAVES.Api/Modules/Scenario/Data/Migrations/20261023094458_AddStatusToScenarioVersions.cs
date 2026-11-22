using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Scenario.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToScenarioVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScenarioVersions_Scenarios_ScenarioId",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "scenario",
                table: "ScenarioVersions",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ScenarioVersions_Difficulty",
                schema: "scenario",
                table: "ScenarioVersions",
                sql: "[Difficulty] IN (N'Beginner', N'Intermediate', N'Advanced')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ScenarioVersions_Status",
                schema: "scenario",
                table: "ScenarioVersions",
                sql: "[Status] IN (N'Draft', N'Published', N'Archived')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ScenarioVersions_VersionNo",
                schema: "scenario",
                table: "ScenarioVersions",
                sql: "[VersionNo] > 0");

            migrationBuilder.AddForeignKey(
                name: "FK_ScenarioVersions_Scenarios_ScenarioId",
                schema: "scenario",
                table: "ScenarioVersions",
                column: "ScenarioId",
                principalSchema: "scenario",
                principalTable: "Scenarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScenarioVersions_Scenarios_ScenarioId",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ScenarioVersions_Difficulty",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ScenarioVersions_Status",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ScenarioVersions_VersionNo",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "scenario",
                table: "ScenarioVersions");

            migrationBuilder.AddForeignKey(
                name: "FK_ScenarioVersions_Scenarios_ScenarioId",
                schema: "scenario",
                table: "ScenarioVersions",
                column: "ScenarioId",
                principalSchema: "scenario",
                principalTable: "Scenarios",
                principalColumn: "Id");
        }
    }
}
