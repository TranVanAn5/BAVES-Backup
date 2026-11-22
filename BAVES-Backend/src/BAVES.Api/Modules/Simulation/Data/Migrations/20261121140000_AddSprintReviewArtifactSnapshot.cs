using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BAVES.Api.Modules.Simulation.Data.Migrations;

/// <inheritdoc />
public class AddSprintReviewArtifactSnapshot : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ArtifactSnapshotJson",
            schema: "simulation",
            table: "SessionSprintReviews",
            type: "nvarchar(max)",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ArtifactSnapshotJson",
            schema: "simulation",
            table: "SessionSprintReviews");
    }
}
