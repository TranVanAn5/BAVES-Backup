using BAVES.Api.Modules.Assessment.Models.Enums;

namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class ReviewRequest
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    public int LearnerId { get; set; }

    public int? AssignedReviewerId { get; set; }

    public string? RequestNote { get; set; }

    public ReviewRequestStatus Status { get; set; } = ReviewRequestStatus.Pending;

    public DateTimeOffset RequestedAt { get; set; }

    public DateTimeOffset? AcceptedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }
}
