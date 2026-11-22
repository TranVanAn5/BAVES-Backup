using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Scenario.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScenarioSprints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // IsFreeAccess có thể đã có từ seed script thủ công — chỉ thêm khi thiếu.
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'scenario.ScenarioVersions', N'IsFreeAccess') IS NULL
                BEGIN
                    ALTER TABLE [scenario].[ScenarioVersions]
                    ADD [IsFreeAccess] bit NOT NULL
                        CONSTRAINT [DF_ScenarioVersions_IsFreeAccess_Mig] DEFAULT (0);
                END
                """);

            migrationBuilder.CreateTable(
                name: "ScenarioSprints",
                schema: "scenario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioVersionId = table.Column<int>(type: "int", nullable: false),
                    SprintNo = table.Column<int>(type: "int", nullable: false),
                    Focus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DurationValue = table.Column<int>(type: "int", nullable: false),
                    DurationUnit = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    TemplateCodes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioSprints", x => x.Id);
                    table.CheckConstraint("CK_ScenarioSprints_DurationUnit", "[DurationUnit] IN (N'days', N'weeks')");
                    table.CheckConstraint("CK_ScenarioSprints_DurationValue", "[DurationValue] > 0");
                    table.CheckConstraint("CK_ScenarioSprints_SprintNo", "[SprintNo] > 0");
                    table.ForeignKey(
                        name: "FK_ScenarioSprints_ScenarioVersions_ScenarioVersionId",
                        column: x => x.ScenarioVersionId,
                        principalSchema: "scenario",
                        principalTable: "ScenarioVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioSprints_ScenarioVersionId_SprintNo",
                schema: "scenario",
                table: "ScenarioSprints",
                columns: new[] { "ScenarioVersionId", "SprintNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScenarioSprints",
                schema: "scenario");

            // Không drop IsFreeAccess — cột có thể đã tồn tại trước migration này.
        }
    }
}
