using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Simulation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachFeedbacks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoachFeedbacks",
                schema: "simulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    StrengthsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GapsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnclearPointsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImprovementHintsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromptVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachFeedbacks_SimulationSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "simulation",
                        principalTable: "SimulationSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoachFeedbacks_SessionId_CreatedAt",
                schema: "simulation",
                table: "CoachFeedbacks",
                columns: new[] { "SessionId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachFeedbacks",
                schema: "simulation");
        }
    }
}
