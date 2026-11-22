using Entity = BAVES.Api.Modules.Scenario.Models.Entities.StakeholderPersona;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Scenario.Data.Configurations;

public sealed class StakeholderPersonaConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("StakeholderPersonas", "scenario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.StakeholderRole).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Department).HasMaxLength(200);
        builder.Property(x => x.Responsibilities);
        builder.Property(x => x.Goals);
        builder.Property(x => x.PainPoints);
        builder.Property(x => x.BusinessConcerns);
        builder.Property(x => x.CommunicationStyle).HasMaxLength(200);
        builder.Property(x => x.Personality);
        builder.HasOne(x => x.ScenarioVersion)
            .WithMany(x => x.StakeholderPersonas)
            .HasForeignKey(x => x.ScenarioVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
