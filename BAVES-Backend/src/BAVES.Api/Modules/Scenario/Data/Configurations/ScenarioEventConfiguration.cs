using Entity = BAVES.Api.Modules.Scenario.Models.Entities.ScenarioEvent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Scenario.Data.Configurations;

public sealed class ScenarioEventConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("ScenarioEvents", "scenario", table =>
        {
            table.HasCheckConstraint("CK_ScenarioEvents_TriggerConditionJson", "[TriggerConditionJson] IS NULL OR ISJSON([TriggerConditionJson]) = 1");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.TriggerType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.TriggerConditionJson);
        builder.Property(x => x.EventContent).IsRequired();
        builder.HasOne(x => x.ScenarioVersion)
            .WithMany(x => x.ScenarioEvents)
            .HasForeignKey(x => x.ScenarioVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
