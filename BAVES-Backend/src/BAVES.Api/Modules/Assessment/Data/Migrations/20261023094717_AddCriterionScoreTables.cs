using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Assessment.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCriterionScoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIFeedbackCriterionScores",
                schema: "assessment",
                columns: table => new
                {
                    AIFeedbackId = table.Column<int>(type: "int", nullable: false),
                    RubricCriterionId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvidenceSnippet = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIFeedbackCriterionScores", x => new { x.AIFeedbackId, x.RubricCriterionId });
                    table.CheckConstraint("CK_AIFeedbackCriterionScores_Score", "[Score] >= 0");
                    table.ForeignKey(
                        name: "FK_AIFeedbackCriterionScores_AIPracticeFeedbacks_AIFeedbackId",
                        column: x => x.AIFeedbackId,
                        principalSchema: "assessment",
                        principalTable: "AIPracticeFeedbacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AIFeedbackCriterionScores_RubricCriteria_RubricCriterionId",
                        column: x => x.RubricCriterionId,
                        principalSchema: "assessment",
                        principalTable: "RubricCriteria",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HumanReviewCriterionScores",
                schema: "assessment",
                columns: table => new
                {
                    HumanReviewId = table.Column<int>(type: "int", nullable: false),
                    RubricCriterionId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HumanReviewCriterionScores", x => new { x.HumanReviewId, x.RubricCriterionId });
                    table.CheckConstraint("CK_HumanReviewCriterionScores_Score", "[Score] >= 0");
                    table.ForeignKey(
                        name: "FK_HumanReviewCriterionScores_HumanReviews_HumanReviewId",
                        column: x => x.HumanReviewId,
                        principalSchema: "assessment",
                        principalTable: "HumanReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HumanReviewCriterionScores_RubricCriteria_RubricCriterionId",
                        column: x => x.RubricCriterionId,
                        principalSchema: "assessment",
                        principalTable: "RubricCriteria",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIFeedbackCriterionScores_RubricCriterionId",
                schema: "assessment",
                table: "AIFeedbackCriterionScores",
                column: "RubricCriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_HumanReviewCriterionScores_RubricCriterionId",
                schema: "assessment",
                table: "HumanReviewCriterionScores",
                column: "RubricCriterionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIFeedbackCriterionScores",
                schema: "assessment");

            migrationBuilder.DropTable(
                name: "HumanReviewCriterionScores",
                schema: "assessment");
        }
    }
}
