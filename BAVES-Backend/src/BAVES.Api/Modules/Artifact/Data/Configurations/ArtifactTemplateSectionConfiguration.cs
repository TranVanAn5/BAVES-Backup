using Entity = BAVES.Api.Modules.Artifact.Models.Entities.ArtifactTemplateSection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Artifact.Data.Configurations;

public sealed class ArtifactTemplateSectionConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("ArtifactTemplateSections", "artifact");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.SectionKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Instructions);
        builder.HasIndex(x => new { x.ArtifactTemplateId, x.SectionKey }).IsUnique();
        builder.HasOne(x => x.ArtifactTemplate)
            .WithMany(x => x.ArtifactTemplateSections)
            .HasForeignKey(x => x.ArtifactTemplateId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
