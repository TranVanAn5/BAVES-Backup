using Entity = BAVES.Api.Modules.Analysis.Models.Entities.AnalysisItem;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Analysis.Data.Configurations;

public sealed class AnalysisItemConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("AnalysisItems", "analysis", table =>
        {
            table.HasCheckConstraint("CK_AnalysisItems_MetadataJson", "[MetadataJson] IS NULL OR ISJSON([MetadataJson]) = 1");
            table.HasCheckConstraint("CK_AnalysisItems_NoSelfParent", "[ParentAnalysisItemId] IS NULL OR [ParentAnalysisItemId] <> [Id]");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.ItemType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.Priority).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.MetadataJson);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.Property(x => x.UpdatedAt).HasPrecision(3);
        builder.HasOne(x => x.ParentAnalysisItem)
            .WithMany(x => x.ChildAnalysisItems)
            .HasForeignKey(x => x.ParentAnalysisItemId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => new { x.SessionId, x.Code }).IsUnique();
        builder.HasIndex(x => new { x.SessionId, x.ItemType });
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.ParentAnalysisItemId);
        builder.HasIndex(x => x.CreatedByUserId);
    }
}
