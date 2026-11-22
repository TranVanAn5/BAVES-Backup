using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Commerce.Data.Configurations;

public class PlanScopeConfiguration : IEntityTypeConfiguration<PlanScope>
{
    public void Configure(EntityTypeBuilder<PlanScope> builder)
    {
        builder.ToTable("PlanScopes", "commerce");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScopeType).HasConversion<string>().HasMaxLength(40);
        builder.HasIndex(x => new { x.PlanId, x.ScopeType, x.ScopeId });
    }
}
