using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Commerce.Data.Configurations;

public class CreditLedgerConfiguration : IEntityTypeConfiguration<CreditLedger>
{
    public void Configure(EntityTypeBuilder<CreditLedger> builder)
    {
        builder.ToTable("CreditLedger", "commerce");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId);
        builder.Property(x => x.CreditType).HasMaxLength(20).HasDefaultValue("Review");
        builder.Property(x => x.Reason).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Note).HasMaxLength(250);
    }
}
