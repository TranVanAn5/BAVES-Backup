using Entity = BAVES.Api.Modules.Planning.Models.Entities.PlannedQuestion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Planning.Data.Configurations;

public sealed class PlannedQuestionConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("PlannedQuestions", "planning");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Topic);
        builder.Property(x => x.InformationNeed);
        builder.Property(x => x.Question).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.HasIndex(x => new { x.BAPlanId, x.DisplayOrder });
        builder.HasOne(x => x.BAPlan)
            .WithMany(x => x.PlannedQuestions)
            .HasForeignKey(x => x.BAPlanId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.StakeholderPersonaId);
    }
}
