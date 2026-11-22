using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Planning.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Planning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "planning");

            migrationBuilder.CreateTable(
                name: "BAPlans",
                schema: "planning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    Objective = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Approach = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BAPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlannedQuestions",
                schema: "planning",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BAPlanId = table.Column<int>(type: "int", nullable: false),
                    StakeholderPersonaId = table.Column<int>(type: "int", nullable: true),
                    Topic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InformationNeed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlannedQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlannedQuestions_BAPlans_BAPlanId",
                        column: x => x.BAPlanId,
                        principalSchema: "planning",
                        principalTable: "BAPlans",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BAPlans_SessionId",
                schema: "planning",
                table: "BAPlans",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlannedQuestions_BAPlanId_DisplayOrder",
                schema: "planning",
                table: "PlannedQuestions",
                columns: new[] { "BAPlanId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PlannedQuestions_StakeholderPersonaId",
                schema: "planning",
                table: "PlannedQuestions",
                column: "StakeholderPersonaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlannedQuestions",
                schema: "planning");

            migrationBuilder.DropTable(
                name: "BAPlans",
                schema: "planning");
        }
    }
}
