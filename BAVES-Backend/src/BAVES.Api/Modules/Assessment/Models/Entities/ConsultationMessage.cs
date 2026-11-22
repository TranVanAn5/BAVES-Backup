namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class ConsultationMessage
{
    public long Id { get; set; }

    public int BookingId { get; set; }

    public int SenderId { get; set; }

    public string SenderRole { get; set; } = string.Empty; // "Learner" | "Expert"

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsRead { get; set; }

    public ExpertBooking Booking { get; set; } = null!;
}
