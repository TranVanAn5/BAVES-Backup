using BAVES.Api.Modules.Scenario.Models.Enums;
using Entity = BAVES.Api.Modules.Scenario.Models.Entities.ScenarioVersion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Scenario.Data.Configurations;

public sealed class ScenarioVersionConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("ScenarioVersions", "scenario", table =>
        {
            table.HasCheckConstraint("CK_ScenarioVersions_VersionNo", "[VersionNo] > 0");
            table.HasCheckConstraint("CK_ScenarioVersions_Difficulty", "[Difficulty] IN (N'Beginner', N'Intermediate', N'Advanced')");
            table.HasCheckConstraint("CK_ScenarioVersions_Status", "[Status] IN (N'Draft', N'Published', N'Archived', N'PendingReview', N'Rejected')");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Summary);
        builder.Property(x => x.BusinessContext).IsRequired();
        builder.Property(x => x.BusinessProblem).IsRequired();
        builder.Property(x => x.BusinessObjectives).IsRequired();
        builder.Property(x => x.BARole).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Assignment).IsRequired();
        builder.Property(x => x.InScope);
        builder.Property(x => x.OutOfScope);
        builder.Property(x => x.KnownConstraints);
        builder.Property(x => x.Difficulty).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.IsFreeAccess).HasDefaultValue(false);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).HasDefaultValue(ScenarioStatus.Draft);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.Property(x => x.PublishedAt).HasPrecision(3);
        builder.Property(x => x.SubmittedForReviewAt).HasPrecision(3);
        builder.Property(x => x.ReviewedAt).HasPrecision(3);
        builder.Property(x => x.ReviewComment).HasMaxLength(4000);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.ScenarioId, x.VersionNo }).IsUnique();
        builder.HasOne(x => x.Scenario)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.ScenarioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
