using Entity = BAVES.Api.Modules.Assessment.Models.Entities.HumanReviewCriterionScore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Assessment.Data.Configurations;

public sealed class HumanReviewCriterionScoreConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("HumanReviewCriterionScores", "assessment", table =>
        {
            table.HasCheckConstraint("CK_HumanReviewCriterionScores_Score", "[Score] >= 0");
        });
        builder.HasKey(x => new { x.HumanReviewId, x.RubricCriterionId });
        builder.Property(x => x.Score).HasPrecision(9, 4);
        builder.Property(x => x.Comment);
        builder.HasIndex(x => x.RubricCriterionId);
        builder.HasOne(x => x.HumanReview)
            .WithMany(x => x.CriterionScores)
            .HasForeignKey(x => x.HumanReviewId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.RubricCriterion)
            .WithMany(x => x.HumanReviewCriterionScores)
            .HasForeignKey(x => x.RubricCriterionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
