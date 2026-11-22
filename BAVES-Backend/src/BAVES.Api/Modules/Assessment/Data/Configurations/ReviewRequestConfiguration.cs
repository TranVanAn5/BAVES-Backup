using Entity = BAVES.Api.Modules.Assessment.Models.Entities.ReviewRequest;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Assessment.Data.Configurations;

public sealed class ReviewRequestConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("ReviewRequests", "assessment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RequestNote).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.RequestedAt).HasPrecision(3);
        builder.Property(x => x.AcceptedAt).HasPrecision(3);
        builder.Property(x => x.CompletedAt).HasPrecision(3);

        builder.HasIndex(x => x.SubmissionId);
        builder.HasIndex(x => x.LearnerId);
        builder.HasIndex(x => x.AssignedReviewerId);
        builder.HasIndex(x => x.Status);
    }
}
