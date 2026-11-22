using Entity = BAVES.Api.Modules.Assessment.Models.Entities.RubricCriterion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Assessment.Data.Configurations;

public sealed class RubricCriterionConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("RubricCriteria", "assessment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Weight).HasPrecision(9, 4);
        builder.Property(x => x.MaxScore).HasPrecision(9, 4);
        builder.Property(x => x.EvidenceGuidance);
        builder.HasIndex(x => new { x.RubricId, x.Code }).IsUnique();
        builder.HasOne(x => x.Rubric)
            .WithMany(x => x.RubricCriteria)
            .HasForeignKey(x => x.RubricId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.BabokTask)
            .WithMany(x => x.RubricCriteria)
            .HasForeignKey(x => x.BabokTaskId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
