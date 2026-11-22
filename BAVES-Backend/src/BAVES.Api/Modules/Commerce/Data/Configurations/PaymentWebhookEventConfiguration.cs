using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Commerce.Data.Configurations;

public class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> builder)
    {
        builder.ToTable("PaymentWebhookEvents", "commerce");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
        builder.Property(x => x.EventId).HasMaxLength(150).IsRequired();
        builder.Property(x => x.PayloadHash).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Result).HasMaxLength(50).IsRequired();

        builder.HasIndex(x => new { x.Provider, x.EventId });
        builder.HasIndex(x => x.PayloadHash);
    }
}
