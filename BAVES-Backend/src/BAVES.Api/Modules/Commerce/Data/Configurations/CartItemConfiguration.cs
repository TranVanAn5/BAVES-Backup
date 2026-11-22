using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Commerce.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems", "commerce");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TargetLabel).HasMaxLength(300);
        builder.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.CartId, x.PlanId, x.TargetScopeId });
    }
}
