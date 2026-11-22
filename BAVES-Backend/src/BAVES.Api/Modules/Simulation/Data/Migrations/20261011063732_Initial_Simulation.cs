using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Simulation.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Simulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "simulation");

            migrationBuilder.CreateTable(
                name: "SimulationSessions",
                schema: "simulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioVersionId = table.Column<int>(type: "int", nullable: false),
                    AssignmentId = table.Column<int>(type: "int", nullable: true),
                    LearnerId = table.Column<int>(type: "int", nullable: false),
                    AttemptNo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SessionAgents",
                schema: "simulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    MemorySummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentMood = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TrustLevel = table.Column<byte>(type: "tinyint", nullable: true),
                    PatienceLevel = table.Column<byte>(type: "tinyint", nullable: true),
                    CooperationLevel = table.Column<byte>(type: "tinyint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastInteractionAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionAgents", x => x.Id);
                    table.CheckConstraint("CK_SessionAgents_CooperationLevel", "[CooperationLevel] IS NULL OR [CooperationLevel] <= 100");
                    table.CheckConstraint("CK_SessionAgents_PatienceLevel", "[PatienceLevel] IS NULL OR [PatienceLevel] <= 100");
                    table.CheckConstraint("CK_SessionAgents_TrustLevel", "[TrustLevel] IS NULL OR [TrustLevel] <= 100");
                    table.ForeignKey(
                        name: "FK_SessionAgents_SimulationSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "simulation",
                        principalTable: "SimulationSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionEvents",
                schema: "simulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ScenarioEventId = table.Column<int>(type: "int", nullable: false),
                    TriggerSource = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TriggeredAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false),
                    EventContentSnapshot = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionEvents_SimulationSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "simulation",
                        principalTable: "SimulationSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "simulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    SessionAgentId = table.Column<int>(type: "int", nullable: true),
                    SenderType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_SessionAgents_SessionAgentId",
                        column: x => x.SessionAgentId,
                        principalSchema: "simulation",
                        principalTable: "SessionAgents",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_SimulationSessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "simulation",
                        principalTable: "SimulationSessions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SessionAgentKnowledgeStates",
                schema: "simulation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionAgentId = table.Column<int>(type: "int", nullable: false),
                    KnowledgeItemId = table.Column<int>(type: "int", nullable: false),
                    IsRevealed = table.Column<bool>(type: "bit", nullable: false),
                    RevealedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true),
                    RevealMessageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionAgentKnowledgeStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionAgentKnowledgeStates_Messages_RevealMessageId",
                        column: x => x.RevealMessageId,
                        principalSchema: "simulation",
                        principalTable: "Messages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SessionAgentKnowledgeStates_SessionAgents_SessionAgentId",
                        column: x => x.SessionAgentId,
                        principalSchema: "simulation",
                        principalTable: "SessionAgents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SessionAgentId",
                schema: "simulation",
                table: "Messages",
                column: "SessionAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SessionId_CreatedAt",
                schema: "simulation",
                table: "Messages",
                columns: new[] { "SessionId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionAgentKnowledgeStates_KnowledgeItemId",
                schema: "simulation",
                table: "SessionAgentKnowledgeStates",
                column: "KnowledgeItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAgentKnowledgeStates_RevealMessageId",
                schema: "simulation",
                table: "SessionAgentKnowledgeStates",
                column: "RevealMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAgentKnowledgeStates_SessionAgentId_KnowledgeItemId",
                schema: "simulation",
                table: "SessionAgentKnowledgeStates",
                columns: new[] { "SessionAgentId", "KnowledgeItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionAgents_PersonaId",
                schema: "simulation",
                table: "SessionAgents",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAgents_SessionId_PersonaId",
                schema: "simulation",
                table: "SessionAgents",
                columns: new[] { "SessionId", "PersonaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionEvents_ScenarioEventId",
                schema: "simulation",
                table: "SessionEvents",
                column: "ScenarioEventId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionEvents_SessionId_SequenceNo",
                schema: "simulation",
                table: "SessionEvents",
                columns: new[] { "SessionId", "SequenceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SimulationSessions_AssignmentId",
                schema: "simulation",
                table: "SimulationSessions",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationSessions_LearnerId",
                schema: "simulation",
                table: "SimulationSessions",
                column: "LearnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationSessions_ScenarioVersionId",
                schema: "simulation",
                table: "SimulationSessions",
                column: "ScenarioVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionAgentKnowledgeStates",
                schema: "simulation");

            migrationBuilder.DropTable(
                name: "SessionEvents",
                schema: "simulation");

            migrationBuilder.DropTable(
                name: "Messages",
                schema: "simulation");

            migrationBuilder.DropTable(
                name: "SessionAgents",
                schema: "simulation");

            migrationBuilder.DropTable(
                name: "SimulationSessions",
                schema: "simulation");
        }
    }
}
