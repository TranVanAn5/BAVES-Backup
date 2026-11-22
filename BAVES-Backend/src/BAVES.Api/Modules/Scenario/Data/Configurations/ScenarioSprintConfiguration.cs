using Entity = BAVES.Api.Modules.Scenario.Models.Entities.ScenarioSprint;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Scenario.Data.Configurations;

public sealed class ScenarioSprintConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("ScenarioSprints", "scenario", table =>
        {
            table.HasCheckConstraint("CK_ScenarioSprints_SprintNo", "[SprintNo] > 0");
            table.HasCheckConstraint("CK_ScenarioSprints_DurationValue", "[DurationValue] > 0");
            table.HasCheckConstraint(
                "CK_ScenarioSprints_DurationUnit",
                "[DurationUnit] IN (N'days', N'weeks')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Focus).IsRequired();
        builder.Property(x => x.DurationUnit).HasMaxLength(16).IsRequired();
        builder.Property(x => x.TemplateCodes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.ScenarioVersionId, x.SprintNo }).IsUnique();
        builder.HasOne(x => x.ScenarioVersion)
            .WithMany(x => x.Sprints)
            .HasForeignKey(x => x.ScenarioVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
