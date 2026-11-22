using Entity = BAVES.Api.Modules.Scenario.Models.Entities.KnowledgeItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Scenario.Data.Configurations;

public sealed class KnowledgeItemConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("KnowledgeItems", "scenario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Importance).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.HasIndex(x => new { x.ScenarioVersionId, x.Code }).IsUnique();
        builder.HasOne(x => x.ScenarioVersion)
            .WithMany(x => x.KnowledgeItems)
            .HasForeignKey(x => x.ScenarioVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
