using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Commerce.Data.Configurations;

public class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines", "commerce");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("VND");
        builder.Property(x => x.PlanCode).HasMaxLength(80);
        builder.Property(x => x.PlanName).HasMaxLength(200);
        builder.Property(x => x.TargetLabel).HasMaxLength(300);
        builder.HasIndex(x => x.OrderId);
        builder.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
