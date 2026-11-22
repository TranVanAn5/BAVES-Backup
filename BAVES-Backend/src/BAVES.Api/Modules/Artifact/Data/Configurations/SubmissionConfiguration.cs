using Entity = BAVES.Api.Modules.Artifact.Models.Entities.Submission;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Artifact.Data.Configurations;

public sealed class SubmissionConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Submissions", "artifact");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.SubmittedAt).HasPrecision(3);
        builder.Property(x => x.FinalizedAt).HasPrecision(3);
        builder.HasIndex(x => new { x.SessionId, x.AttemptNo }).IsUnique();
        builder.HasIndex(x => x.SessionId);
        builder.HasOne(x => x.ArtifactVersion)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.ArtifactVersionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
