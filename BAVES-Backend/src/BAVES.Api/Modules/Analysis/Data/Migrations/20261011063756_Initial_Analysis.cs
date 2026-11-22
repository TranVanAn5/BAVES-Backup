using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Analysis.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Analysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "analysis");

            migrationBuilder.CreateTable(
                name: "AnalysisItems",
                schema: "analysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisItems", x => x.Id);
                    table.CheckConstraint("CK_AnalysisItems_MetadataJson", "[MetadataJson] IS NULL OR ISJSON([MetadataJson]) = 1");
                });

            migrationBuilder.CreateTable(
                name: "AnalysisItemRelations",
                schema: "analysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SourceAnalysisItemId = table.Column<int>(type: "int", nullable: false),
                    TargetAnalysisItemId = table.Column<int>(type: "int", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisItemRelations", x => x.Id);
                    table.CheckConstraint("CK_AnalysisItemRelations_NoSelfLink", "[SourceAnalysisItemId] <> [TargetAnalysisItemId]");
                    table.ForeignKey(
                        name: "FK_AnalysisItemRelations_AnalysisItems_SourceAnalysisItemId",
                        column: x => x.SourceAnalysisItemId,
                        principalSchema: "analysis",
                        principalTable: "AnalysisItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AnalysisItemRelations_AnalysisItems_TargetAnalysisItemId",
                        column: x => x.TargetAnalysisItemId,
                        principalSchema: "analysis",
                        principalTable: "AnalysisItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MessageEvidenceLinks",
                schema: "analysis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    AnalysisItemId = table.Column<int>(type: "int", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageEvidenceLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessageEvidenceLinks_AnalysisItems_AnalysisItemId",
                        column: x => x.AnalysisItemId,
                        principalSchema: "analysis",
                        principalTable: "AnalysisItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisItemRelations_SourceAnalysisItemId_TargetAnalysisItemId_RelationshipType",
                schema: "analysis",
                table: "AnalysisItemRelations",
                columns: new[] { "SourceAnalysisItemId", "TargetAnalysisItemId", "RelationshipType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisItemRelations_TargetAnalysisItemId",
                schema: "analysis",
                table: "AnalysisItemRelations",
                column: "TargetAnalysisItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisItems_CreatedByUserId",
                schema: "analysis",
                table: "AnalysisItems",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisItems_SessionId",
                schema: "analysis",
                table: "AnalysisItems",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisItems_SessionId_Code",
                schema: "analysis",
                table: "AnalysisItems",
                columns: new[] { "SessionId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisItems_SessionId_ItemType",
                schema: "analysis",
                table: "AnalysisItems",
                columns: new[] { "SessionId", "ItemType" });

            migrationBuilder.CreateIndex(
                name: "IX_MessageEvidenceLinks_AnalysisItemId",
                schema: "analysis",
                table: "MessageEvidenceLinks",
                column: "AnalysisItemId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageEvidenceLinks_MessageId",
                schema: "analysis",
                table: "MessageEvidenceLinks",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageEvidenceLinks_MessageId_AnalysisItemId_RelationshipType",
                schema: "analysis",
                table: "MessageEvidenceLinks",
                columns: new[] { "MessageId", "AnalysisItemId", "RelationshipType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisItemRelations",
                schema: "analysis");

            migrationBuilder.DropTable(
                name: "MessageEvidenceLinks",
                schema: "analysis");

            migrationBuilder.DropTable(
                name: "AnalysisItems",
                schema: "analysis");
        }
    }
}
