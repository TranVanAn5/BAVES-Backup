using Entity = BAVES.Api.Modules.Assessment.Models.Entities.BabokTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Assessment.Data.Configurations;

public sealed class BabokTaskConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("BabokTasks", "assessment");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.KnowledgeAreaCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.KnowledgeAreaName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Purpose);
        builder.Property(x => x.Description);
        builder.Property(x => x.BabokVersion).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => new { x.BabokVersion, x.Code }).IsUnique();
        builder.HasIndex(x => x.KnowledgeAreaCode);
    }
}
