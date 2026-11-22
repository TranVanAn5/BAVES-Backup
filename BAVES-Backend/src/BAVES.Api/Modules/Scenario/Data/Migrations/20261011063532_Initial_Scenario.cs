using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Scenario.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Scenario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scenario");

            migrationBuilder.CreateTable(
                name: "Scenarios",
                schema: "scenario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScenarioVersions",
                schema: "scenario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioId = table.Column<int>(type: "int", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessContext = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessProblem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessObjectives = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BARole = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Assignment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InScope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutOfScope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KnownConstraints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Difficulty = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScenarioVersions_Scenarios_ScenarioId",
                        column: x => x.ScenarioId,
                        principalSchema: "scenario",
                        principalTable: "Scenarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BusinessRules",
                schema: "scenario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioVersionId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false),
                    Importance = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessRules_ScenarioVersions_ScenarioVersionId",
                        column: x => x.ScenarioVersionId,
                        principalSchema: "scenario",
                        principalTable: "ScenarioVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "KnowledgeItems",
                schema: "scenario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioVersionId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Importance = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KnowledgeItems_ScenarioVersions_ScenarioVersionId",
                        column: x => x.ScenarioVersionId,
                        principalSchema: "scenario",
                        principalTable: "ScenarioVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScenarioEvents",
                schema: "scenario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioVersionId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TriggerType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TriggerConditionJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScenarioEvents", x => x.Id);
                    table.CheckConstraint("CK_ScenarioEvents_TriggerConditionJson", "[TriggerConditionJson] IS NULL OR ISJSON([TriggerConditionJson]) = 1");
                    table.ForeignKey(
                        name: "FK_ScenarioEvents_ScenarioVersions_ScenarioVersionId",
                        column: x => x.ScenarioVersionId,
                        principalSchema: "scenario",
                        principalTable: "ScenarioVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StakeholderPersonas",
                schema: "scenario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioVersionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StakeholderRole = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Responsibilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Goals = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PainPoints = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessConcerns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommunicationStyle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Personality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderPersonas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderPersonas_ScenarioVersions_ScenarioVersionId",
                        column: x => x.ScenarioVersionId,
                        principalSchema: "scenario",
                        principalTable: "ScenarioVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StakeholderKnowledgeAccess",
                schema: "scenario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StakeholderPersonaId = table.Column<int>(type: "int", nullable: false),
                    KnowledgeItemId = table.Column<int>(type: "int", nullable: false),
                    AccessType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DisclosureRule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHidden = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakeholderKnowledgeAccess", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakeholderKnowledgeAccess_KnowledgeItems_KnowledgeItemId",
                        column: x => x.KnowledgeItemId,
                        principalSchema: "scenario",
                        principalTable: "KnowledgeItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StakeholderKnowledgeAccess_StakeholderPersonas_StakeholderPersonaId",
                        column: x => x.StakeholderPersonaId,
                        principalSchema: "scenario",
                        principalTable: "StakeholderPersonas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessRules_ScenarioVersionId_Code",
                schema: "scenario",
                table: "BusinessRules",
                columns: new[] { "ScenarioVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeItems_ScenarioVersionId_Code",
                schema: "scenario",
                table: "KnowledgeItems",
                columns: new[] { "ScenarioVersionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioEvents_ScenarioVersionId",
                schema: "scenario",
                table: "ScenarioEvents",
                column: "ScenarioVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ApplicationId",
                schema: "scenario",
                table: "Scenarios",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_Code",
                schema: "scenario",
                table: "Scenarios",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_CreatedByUserId",
                schema: "scenario",
                table: "Scenarios",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScenarioVersions_ScenarioId_VersionNo",
                schema: "scenario",
                table: "ScenarioVersions",
                columns: new[] { "ScenarioId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderKnowledgeAccess_KnowledgeItemId",
                schema: "scenario",
                table: "StakeholderKnowledgeAccess",
                column: "KnowledgeItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderKnowledgeAccess_StakeholderPersonaId_KnowledgeItemId",
                schema: "scenario",
                table: "StakeholderKnowledgeAccess",
                columns: new[] { "StakeholderPersonaId", "KnowledgeItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StakeholderPersonas_ScenarioVersionId",
                schema: "scenario",
                table: "StakeholderPersonas",
                column: "ScenarioVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessRules",
                schema: "scenario");

            migrationBuilder.DropTable(
                name: "ScenarioEvents",
                schema: "scenario");

            migrationBuilder.DropTable(
                name: "StakeholderKnowledgeAccess",
                schema: "scenario");

            migrationBuilder.DropTable(
                name: "KnowledgeItems",
                schema: "scenario");

            migrationBuilder.DropTable(
                name: "StakeholderPersonas",
                schema: "scenario");

            migrationBuilder.DropTable(
                name: "ScenarioVersions",
                schema: "scenario");

            migrationBuilder.DropTable(
                name: "Scenarios",
                schema: "scenario");
        }
    }
}
