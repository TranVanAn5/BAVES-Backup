using Entity = BAVES.Api.Modules.Training.Models.Entities.CohortMember;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Training.Data.Configurations;

public sealed class CohortMemberConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("CohortMembers", "training");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.JoinedAt).HasPrecision(3);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(x => new { x.CohortId, x.LearnerId }).IsUnique();
        builder.HasOne(x => x.Cohort)
            .WithMany(x => x.CohortMembers)
            .HasForeignKey(x => x.CohortId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.LearnerId);
    }
}
