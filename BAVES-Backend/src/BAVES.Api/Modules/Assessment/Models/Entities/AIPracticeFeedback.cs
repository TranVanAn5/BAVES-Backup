using BAVES.Api.Modules.Assessment.Models.Enums;

namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class AIPracticeFeedback
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    public string Provider { get; set; } = null!;

    public string Model { get; set; } = null!;

    public AIFeedbackStatus Status { get; set; } = AIFeedbackStatus.Pending;

    public string? OverallFeedback { get; set; }

    public string FeedbackItemsJson { get; set; } = "[]";

    public decimal? SuggestedScore { get; set; }

    public decimal? Confidence { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<AIFeedbackCriterionScore> CriterionScores { get; set; } = new List<AIFeedbackCriterionScore>();

}
