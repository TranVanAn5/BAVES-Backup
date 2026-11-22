using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Assessment.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Assessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "assessment");

            migrationBuilder.CreateTable(
                name: "AIPracticeFeedbacks",
                schema: "assessment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    OverallFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeedbackItemsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuggestedScore = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    Confidence = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: true),
                    StartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIPracticeFeedbacks", x => x.Id);
                    table.CheckConstraint("CK_AIPracticeFeedbacks_Confidence", "[Confidence] IS NULL OR ([Confidence] >= 0 AND [Confidence] <= 1)");
                    table.CheckConstraint("CK_AIPracticeFeedbacks_FeedbackItemsJson", "ISJSON([FeedbackItemsJson]) = 1");
                });

            migrationBuilder.CreateTable(
                name: "BabokKnowledgeAreas",
                schema: "assessment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BabokVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BabokKnowledgeAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rubrics",
                schema: "assessment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScenarioVersionId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rubrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BabokTasks",
                schema: "assessment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KnowledgeAreaId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BabokVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BabokTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BabokTasks_BabokKnowledgeAreas_KnowledgeAreaId",
                        column: x => x.KnowledgeAreaId,
                        principalSchema: "assessment",
                        principalTable: "BabokKnowledgeAreas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HumanReviews",
                schema: "assessment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubmissionId = table.Column<int>(type: "int", nullable: false),
                    RubricId = table.Column<int>(type: "int", nullable: false),
                    ReviewerId = table.Column<int>(type: "int", nullable: false),
                    FinalScore = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    CriterionResultsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OverallFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanReviews", x => x.Id);
                    table.CheckConstraint("CK_HumanReviews_CriterionResultsJson", "ISJSON([CriterionResultsJson]) = 1");
                    table.ForeignKey(
                        name: "FK_HumanReviews_Rubrics_RubricId",
                        column: x => x.RubricId,
                        principalSchema: "assessment",
                        principalTable: "Rubrics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RubricCriteria",
                schema: "assessment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RubricId = table.Column<int>(type: "int", nullable: false),
                    BabokTaskId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    MaxScore = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    EvidenceGuidance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubricCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RubricCriteria_BabokTasks_BabokTaskId",
                        column: x => x.BabokTaskId,
                        principalSchema: "assessment",
                        principalTable: "BabokTasks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RubricCriteria_Rubrics_RubricId",
                        column: x => x.RubricId,
                        principalSchema: "assessment",
                        principalTable: "Rubrics",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIPracticeFeedbacks_SubmissionId",
                schema: "assessment",
                table: "AIPracticeFeedbacks",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_BabokKnowledgeAreas_Code",
                schema: "assessment",
                table: "BabokKnowledgeAreas",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BabokTasks_BabokVersion_Code",
                schema: "assessment",
                table: "BabokTasks",
                columns: new[] { "BabokVersion", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BabokTasks_KnowledgeAreaId",
                schema: "assessment",
                table: "BabokTasks",
                column: "KnowledgeAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanReviews_ReviewerId",
                schema: "assessment",
                table: "HumanReviews",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanReviews_RubricId",
                schema: "assessment",
                table: "HumanReviews",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanReviews_SubmissionId",
                schema: "assessment",
                table: "HumanReviews",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RubricCriteria_BabokTaskId",
                schema: "assessment",
                table: "RubricCriteria",
                column: "BabokTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_RubricCriteria_RubricId_Code",
                schema: "assessment",
                table: "RubricCriteria",
                columns: new[] { "RubricId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rubrics_CreatedByUserId",
                schema: "assessment",
                table: "Rubrics",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rubrics_ScenarioVersionId",
                schema: "assessment",
                table: "Rubrics",
                column: "ScenarioVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Rubrics_ScenarioVersionId_Version",
                schema: "assessment",
                table: "Rubrics",
                columns: new[] { "ScenarioVersionId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIPracticeFeedbacks",
                schema: "assessment");

            migrationBuilder.DropTable(
                name: "HumanReviews",
                schema: "assessment");

            migrationBuilder.DropTable(
                name: "RubricCriteria",
                schema: "assessment");

            migrationBuilder.DropTable(
                name: "BabokTasks",
                schema: "assessment");

            migrationBuilder.DropTable(
                name: "Rubrics",
                schema: "assessment");

            migrationBuilder.DropTable(
                name: "BabokKnowledgeAreas",
                schema: "assessment");
        }
    }
}
