using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Simulation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionSprintReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionSprintReviews",
                schema: "simulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    SprintNo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DiscussionJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChairSpeakerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ChairReply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChallengeQuestionsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    SprintScore = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    CompressedDigest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedPointsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PromptVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionSprintReviews", x => x.Id);
                    table.CheckConstraint("CK_SessionSprintReviews_SprintNo", "[SprintNo] > 0");
                    table.CheckConstraint("CK_SessionSprintReviews_Status", "[Status] IN (N'Open', N'Finalized')");
                    table.ForeignKey(
                        name: "FK_SessionSprintReviews_SimulationSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "simulation",
                        principalTable: "SimulationSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionSprintReviews_SessionId_SprintNo",
                schema: "simulation",
                table: "SessionSprintReviews",
                columns: new[] { "SessionId", "SprintNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionSprintReviews",
                schema: "simulation");
        }
    }
}
