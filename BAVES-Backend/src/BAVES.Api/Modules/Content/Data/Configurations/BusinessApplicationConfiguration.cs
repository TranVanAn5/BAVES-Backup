using Entity = BAVES.Api.Modules.Content.Models.Entities.BusinessApplication;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Content.Data.Configurations;

public sealed class BusinessApplicationConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Applications", "content");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.Property(x => x.UpdatedAt).HasPrecision(3);
        builder.HasIndex(x => new { x.IndustryId, x.Code }).IsUnique();
        builder.HasOne(x => x.Industry)
            .WithMany(x => x.Applications)
            .HasForeignKey(x => x.IndustryId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
