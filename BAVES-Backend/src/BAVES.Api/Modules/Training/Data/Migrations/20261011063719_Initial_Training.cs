using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Training.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Training : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "training");

            migrationBuilder.CreateTable(
                name: "Cohorts",
                schema: "training",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstructorId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cohorts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                schema: "training",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioVersionId = table.Column<int>(type: "int", nullable: false),
                    CohortId = table.Column<int>(type: "int", nullable: true),
                    LearnerId = table.Column<int>(type: "int", nullable: true),
                    RubricId = table.Column<int>(type: "int", nullable: true),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    DueAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    MaxAttempts = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.CheckConstraint("CK_Assignments_MaxAttempts", "[MaxAttempts] IS NULL OR [MaxAttempts] > 0");
                    table.CheckConstraint("CK_Assignments_Recipient", "([CohortId] IS NOT NULL AND [LearnerId] IS NULL) OR ([CohortId] IS NULL AND [LearnerId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Assignments_Cohorts_CohortId",
                        column: x => x.CohortId,
                        principalSchema: "training",
                        principalTable: "Cohorts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CohortMembers",
                schema: "training",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CohortId = table.Column<int>(type: "int", nullable: false),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CohortMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CohortMembers_Cohorts_CohortId",
                        column: x => x.CohortId,
                        principalSchema: "training",
                        principalTable: "Cohorts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_AssignedByUserId",
                schema: "training",
                table: "Assignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_CohortId",
                schema: "training",
                table: "Assignments",
                column: "CohortId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_LearnerId",
                schema: "training",
                table: "Assignments",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_RubricId",
                schema: "training",
                table: "Assignments",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_ScenarioVersionId",
                schema: "training",
                table: "Assignments",
                column: "ScenarioVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CohortMembers_CohortId_LearnerId",
                schema: "training",
                table: "CohortMembers",
                columns: new[] { "CohortId", "LearnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CohortMembers_LearnerId",
                schema: "training",
                table: "CohortMembers",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Cohorts_Code",
                schema: "training",
                table: "Cohorts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cohorts_InstructorId",
                schema: "training",
                table: "Cohorts",
                column: "InstructorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments",
                schema: "training");

            migrationBuilder.DropTable(
                name: "CohortMembers",
                schema: "training");

            migrationBuilder.DropTable(
                name: "Cohorts",
                schema: "training");
        }
    }
}
