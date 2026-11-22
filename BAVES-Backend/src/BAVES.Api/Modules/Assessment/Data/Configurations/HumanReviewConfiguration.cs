using Entity = BAVES.Api.Modules.Assessment.Models.Entities.HumanReview;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Assessment.Data.Configurations;

public sealed class HumanReviewConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("HumanReviews", "assessment", table =>
        {
            table.HasCheckConstraint("CK_HumanReviews_CriterionResultsJson", "ISJSON([CriterionResultsJson]) = 1");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.FinalScore).HasPrecision(9, 4);
        builder.Property(x => x.CriterionResultsJson).IsRequired();
        builder.Property(x => x.OverallFeedback);
        builder.Property(x => x.ReviewedAt).HasPrecision(3);
        builder.HasIndex(x => x.SubmissionId);
        builder.HasOne(x => x.Rubric)
            .WithMany(x => x.HumanReviews)
            .HasForeignKey(x => x.RubricId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.ReviewerId);
    }
}
