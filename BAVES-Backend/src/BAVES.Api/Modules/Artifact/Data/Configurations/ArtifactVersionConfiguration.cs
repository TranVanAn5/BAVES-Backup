using Entity = BAVES.Api.Modules.Artifact.Models.Entities.ArtifactVersion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Artifact.Data.Configurations;

public sealed class ArtifactVersionConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("ArtifactVersions", "artifact", table =>
        {
            table.HasCheckConstraint("CK_ArtifactVersions_StructuredContentJson", "ISJSON([StructuredContentJson]) = 1");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.StructuredContentJson).IsRequired();
        builder.Property(x => x.PlainTextContent).IsRequired();
        builder.Property(x => x.ProcessingStatus).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.HasIndex(x => new { x.ArtifactId, x.VersionNo }).IsUnique();
        builder.HasOne(x => x.Artifact)
            .WithMany(x => x.ArtifactVersions)
            .HasForeignKey(x => x.ArtifactId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.CreatedByUserId);
    }
}
