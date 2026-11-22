using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Artifact.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Artifact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "artifact");

            migrationBuilder.CreateTable(
                name: "ArtifactTemplates",
                schema: "artifact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArtifactType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Artifacts",
                schema: "artifact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ArtifactTemplateId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DraftStructuredContentJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DraftPlainTextContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CurrentVersionNo = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artifacts", x => x.Id);
                    table.CheckConstraint("CK_Artifacts_DraftStructuredContentJson", "[DraftStructuredContentJson] IS NULL OR ISJSON([DraftStructuredContentJson]) = 1");
                    table.ForeignKey(
                        name: "FK_Artifacts_ArtifactTemplates_ArtifactTemplateId",
                        column: x => x.ArtifactTemplateId,
                        principalSchema: "artifact",
                        principalTable: "ArtifactTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArtifactTemplateSections",
                schema: "artifact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtifactTemplateId = table.Column<int>(type: "int", nullable: false),
                    SectionKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactTemplateSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtifactTemplateSections_ArtifactTemplates_ArtifactTemplateId",
                        column: x => x.ArtifactTemplateId,
                        principalSchema: "artifact",
                        principalTable: "ArtifactTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArtifactVersions",
                schema: "artifact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArtifactId = table.Column<int>(type: "int", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    StructuredContentJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlainTextContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    ProcessingStatus = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtifactVersions", x => x.Id);
                    table.CheckConstraint("CK_ArtifactVersions_StructuredContentJson", "ISJSON([StructuredContentJson]) = 1");
                    table.ForeignKey(
                        name: "FK_ArtifactVersions_Artifacts_ArtifactId",
                        column: x => x.ArtifactId,
                        principalSchema: "artifact",
                        principalTable: "Artifacts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                schema: "artifact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    ArtifactVersionId = table.Column<int>(type: "int", nullable: false),
                    AttemptNo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: false),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "datetimeoffset(3)", precision: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Submissions_ArtifactVersions_ArtifactVersionId",
                        column: x => x.ArtifactVersionId,
                        principalSchema: "artifact",
                        principalTable: "ArtifactVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_ArtifactTemplateId",
                schema: "artifact",
                table: "Artifacts",
                column: "ArtifactTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Artifacts_SessionId",
                schema: "artifact",
                table: "Artifacts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactTemplates_Code",
                schema: "artifact",
                table: "ArtifactTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactTemplateSections_ArtifactTemplateId_SectionKey",
                schema: "artifact",
                table: "ArtifactTemplateSections",
                columns: new[] { "ArtifactTemplateId", "SectionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactVersions_ArtifactId_VersionNo",
                schema: "artifact",
                table: "ArtifactVersions",
                columns: new[] { "ArtifactId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtifactVersions_CreatedByUserId",
                schema: "artifact",
                table: "ArtifactVersions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ArtifactVersionId",
                schema: "artifact",
                table: "Submissions",
                column: "ArtifactVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SessionId",
                schema: "artifact",
                table: "Submissions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SessionId_AttemptNo",
                schema: "artifact",
                table: "Submissions",
                columns: new[] { "SessionId", "AttemptNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtifactTemplateSections",
                schema: "artifact");

            migrationBuilder.DropTable(
                name: "Submissions",
                schema: "artifact");

            migrationBuilder.DropTable(
                name: "ArtifactVersions",
                schema: "artifact");

            migrationBuilder.DropTable(
                name: "Artifacts",
                schema: "artifact");

            migrationBuilder.DropTable(
                name: "ArtifactTemplates",
                schema: "artifact");
        }
    }
}
