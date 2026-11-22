using Entity = BAVES.Api.Modules.Assessment.Models.Entities.AIPracticeFeedback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Assessment.Data.Configurations;

public sealed class AIPracticeFeedbackConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("AIPracticeFeedbacks", "assessment", table =>
        {
            table.HasCheckConstraint("CK_AIPracticeFeedbacks_Confidence", "[Confidence] IS NULL OR ([Confidence] >= 0 AND [Confidence] <= 1)");
            table.HasCheckConstraint("CK_AIPracticeFeedbacks_FeedbackItemsJson", "ISJSON([FeedbackItemsJson]) = 1");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.OverallFeedback);
        builder.Property(x => x.FeedbackItemsJson).IsRequired();
        builder.Property(x => x.SuggestedScore).HasPrecision(9, 4);
        builder.Property(x => x.Confidence).HasPrecision(5, 4);
        builder.Property(x => x.StartedAt).HasPrecision(3);
        builder.Property(x => x.CompletedAt).HasPrecision(3);
        builder.HasIndex(x => x.SubmissionId);
    }
}
