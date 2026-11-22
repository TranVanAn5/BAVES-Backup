using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Commerce.Data.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions", "commerce");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ProviderTransactionId).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.Provider, x.ProviderTransactionId }).IsUnique();

        builder.Property(x => x.Amount).HasPrecision(12, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
    }
}
