using BAVES.Api.Modules.Assessment.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Assessment.Data.Configurations;

public class ExpertBookingConfiguration : IEntityTypeConfiguration<ExpertBooking>
{
    public void Configure(EntityTypeBuilder<ExpertBooking> builder)
    {
        builder.ToTable("ExpertBookings", "assessment");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.LearnerNote).HasMaxLength(1000);
        builder.Property(x => x.ExpertSummaryNote).HasMaxLength(2000);

        builder.HasOne(x => x.ReviewRequest)
            .WithMany()
            .HasForeignKey(x => x.ReviewRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Messages)
            .WithOne(m => m.Booking)
            .HasForeignKey(m => m.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.LearnerId);
        builder.HasIndex(x => x.ExpertId);
        builder.HasIndex(x => x.ReviewRequestId);
    }
}
