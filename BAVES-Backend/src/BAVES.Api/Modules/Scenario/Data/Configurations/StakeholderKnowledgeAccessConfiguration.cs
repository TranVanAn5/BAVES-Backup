using Entity = BAVES.Api.Modules.Scenario.Models.Entities.StakeholderKnowledgeAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Scenario.Data.Configurations;

public sealed class StakeholderKnowledgeAccessConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("StakeholderKnowledgeAccess", "scenario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.AccessType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.DisclosureRule);
        builder.HasIndex(x => new { x.StakeholderPersonaId, x.KnowledgeItemId }).IsUnique();
        builder.HasOne(x => x.StakeholderPersona)
            .WithMany(x => x.StakeholderKnowledgeAccess)
            .HasForeignKey(x => x.StakeholderPersonaId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.KnowledgeItem)
            .WithMany(x => x.StakeholderKnowledgeAccess)
            .HasForeignKey(x => x.KnowledgeItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
