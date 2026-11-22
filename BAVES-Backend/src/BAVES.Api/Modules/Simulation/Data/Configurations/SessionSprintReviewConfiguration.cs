using Entity = BAVES.Api.Modules.Simulation.Models.Entities.SessionSprintReview;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Simulation.Data.Configurations;

public sealed class SessionSprintReviewConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("SessionSprintReviews", "simulation", table =>
        {
            table.HasCheckConstraint("CK_SessionSprintReviews_SprintNo", "[SprintNo] > 0");
            table.HasCheckConstraint("CK_SessionSprintReviews_Status", "[Status] IN (N'Open', N'Finalized')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.Property(x => x.DiscussionJson).IsRequired();
        builder.Property(x => x.ChairSpeakerName).HasMaxLength(200);
        builder.Property(x => x.ChairReply);
        builder.Property(x => x.ChallengeQuestionsJson).IsRequired();
        builder.Property(x => x.Decision).HasMaxLength(40);
        builder.Property(x => x.SprintScore).HasPrecision(9, 4);
        builder.Property(x => x.CompressedDigest);
        builder.Property(x => x.ApprovedPointsJson).IsRequired();
        builder.Property(x => x.ActionItemsJson).IsRequired();
        builder.Property(x => x.ArtifactSnapshotJson);
        builder.Property(x => x.PromptVersion).HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.Property(x => x.FinalizedAt).HasPrecision(3);
        builder.HasIndex(x => new { x.SessionId, x.SprintNo }).IsUnique();
        builder.HasOne(x => x.Session)
            .WithMany()
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
