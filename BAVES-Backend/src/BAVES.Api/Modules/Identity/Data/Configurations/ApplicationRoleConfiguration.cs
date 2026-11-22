using Entity = BAVES.Api.Modules.Identity.Models.Entities.ApplicationRole;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Identity.Data.Configurations;

public sealed class ApplicationRoleConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Roles", "identity");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Description);
        builder.Property(x => x.CreatedAt).HasPrecision(3);
    }
}
