using Entity = BAVES.Api.Modules.Analysis.Models.Entities.MessageEvidenceLink;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Analysis.Data.Configurations;

public sealed class MessageEvidenceLinkConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("MessageEvidenceLinks", "analysis");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RelationshipType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.HasIndex(x => new { x.MessageId, x.AnalysisItemId, x.RelationshipType }).IsUnique();
        builder.HasIndex(x => x.MessageId);
        builder.HasOne(x => x.AnalysisItem)
            .WithMany(x => x.MessageEvidenceLinks)
            .HasForeignKey(x => x.AnalysisItemId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
