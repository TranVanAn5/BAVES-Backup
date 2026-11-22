using Entity = BAVES.Api.Modules.Assessment.Models.Entities.AIFeedbackCriterionScore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Assessment.Data.Configurations;

public sealed class AIFeedbackCriterionScoreConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("AIFeedbackCriterionScores", "assessment", table =>
        {
            table.HasCheckConstraint("CK_AIFeedbackCriterionScores_Score", "[Score] >= 0");
        });
        builder.HasKey(x => new { x.AIFeedbackId, x.RubricCriterionId });
        builder.Property(x => x.Score).HasPrecision(9, 4);
        builder.Property(x => x.Comment);
        builder.Property(x => x.EvidenceSnippet);
        builder.HasIndex(x => x.RubricCriterionId);
        builder.HasOne(x => x.AIFeedback)
            .WithMany(x => x.CriterionScores)
            .HasForeignKey(x => x.AIFeedbackId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.RubricCriterion)
            .WithMany(x => x.AIFeedbackCriterionScores)
            .HasForeignKey(x => x.RubricCriterionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
