using Entity = BAVES.Api.Modules.Artifact.Models.Entities.Artifact;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Artifact.Data.Configurations;

public sealed class ArtifactConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Artifacts", "artifact", table =>
        {
            table.HasCheckConstraint("CK_Artifacts_DraftStructuredContentJson", "[DraftStructuredContentJson] IS NULL OR ISJSON([DraftStructuredContentJson]) = 1");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DraftStructuredContentJson);
        builder.Property(x => x.DraftPlainTextContent);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.Property(x => x.UpdatedAt).HasPrecision(3);
        builder.HasIndex(x => x.SessionId);
        builder.HasOne(x => x.ArtifactTemplate)
            .WithMany(x => x.Artifacts)
            .HasForeignKey(x => x.ArtifactTemplateId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
