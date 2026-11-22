using Entity = BAVES.Api.Modules.Artifact.Models.Entities.ArtifactTemplate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Artifact.Data.Configurations;

public sealed class ArtifactTemplateConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("ArtifactTemplates", "artifact");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ArtifactType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Description);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.DeliverableCode).HasMaxLength(20);
        builder.Property(x => x.Category).HasMaxLength(80);
        builder.Property(x => x.DefaultTitle).HasMaxLength(300);
        builder.Property(x => x.IssueKeyPrefix).HasMaxLength(40);
        builder.Property(x => x.DisplayOrder);
        builder.Property(x => x.SkeletonHtmlContent);
        builder.Property(x => x.SampleHtmlContent);
        builder.Property(x => x.ChecklistJson);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.Property(x => x.UpdatedAt).HasPrecision(3);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.DeliverableCode);
    }
}
