using Entity = BAVES.Api.Modules.Planning.Models.Entities.BAPlan;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Planning.Data.Configurations;

public sealed class BAPlanConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("BAPlans", "planning");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Objective).IsRequired();
        builder.Property(x => x.Approach);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.Property(x => x.UpdatedAt).HasPrecision(3);
        builder.HasIndex(x => x.SessionId).IsUnique();
    }
}
