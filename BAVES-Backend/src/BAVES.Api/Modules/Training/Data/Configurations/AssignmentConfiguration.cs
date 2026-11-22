using Entity = BAVES.Api.Modules.Training.Models.Entities.Assignment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Training.Data.Configurations;

public sealed class AssignmentConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Assignments", "training", table =>
        {
            table.HasCheckConstraint("CK_Assignments_Recipient", "([CohortId] IS NOT NULL AND [LearnerId] IS NULL) OR ([CohortId] IS NULL AND [LearnerId] IS NOT NULL)");
            table.HasCheckConstraint("CK_Assignments_MaxAttempts", "[MaxAttempts] IS NULL OR [MaxAttempts] > 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Instructions);
        builder.Property(x => x.StartAt).HasPrecision(3);
        builder.Property(x => x.DueAt).HasPrecision(3);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.Property(x => x.UpdatedAt).HasPrecision(3);
        builder.HasIndex(x => x.ScenarioVersionId);
        builder.HasOne(x => x.Cohort)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.CohortId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.LearnerId);
        builder.HasIndex(x => x.RubricId);
        builder.HasIndex(x => x.AssignedByUserId);
    }
}
