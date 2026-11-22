using BAVES.Api.Modules.Assessment.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Assessment.Data.Configurations;

public class ConsultationMessageConfiguration : IEntityTypeConfiguration<ConsultationMessage>
{
    public void Configure(EntityTypeBuilder<ConsultationMessage> builder)
    {
        builder.ToTable("ConsultationMessages", "assessment");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SenderRole).HasMaxLength(20);
        builder.Property(x => x.Content).IsRequired();

        builder.HasIndex(x => new { x.BookingId, x.SentAt });
    }
}
