using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Commerce.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", "commerce");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.OrderCode).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IdempotencyKey);

        builder.Property(x => x.Amount).HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("VND");
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100);
        builder.Property(x => x.CheckoutUrl).HasMaxLength(500);
        builder.Property(x => x.QrCode).HasMaxLength(2000);

        builder.HasOne(x => x.Plan)
            .WithMany(x => x.Orders)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Entitlements)
            .WithOne(x => x.SourceOrder)
            .HasForeignKey(x => x.SourceOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
