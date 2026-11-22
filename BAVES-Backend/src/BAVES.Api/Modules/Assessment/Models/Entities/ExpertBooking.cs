using BAVES.Api.Modules.Assessment.Models.Enums;

namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class ExpertBooking
{
    public int Id { get; set; }

    public int ReviewRequestId { get; set; }

    public int SubmissionId { get; set; }

    public int LearnerId { get; set; }

    public int ExpertId { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTimeOffset? ScheduledStartTime { get; set; }

    public DateTimeOffset? ScheduledEndTime { get; set; }

    public DateTimeOffset? ActualStartTime { get; set; }

    public DateTimeOffset? ActualEndTime { get; set; }

    public string? LearnerNote { get; set; }

    public string? ExpertSummaryNote { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ReviewRequest ReviewRequest { get; set; } = null!;

    public ICollection<ConsultationMessage> Messages { get; set; } = new List<ConsultationMessage>();
}
