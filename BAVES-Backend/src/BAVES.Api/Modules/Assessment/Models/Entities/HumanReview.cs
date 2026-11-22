namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class HumanReview
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    public int RubricId { get; set; }

    public int ReviewerId { get; set; }

    public decimal FinalScore { get; set; }

    public string CriterionResultsJson { get; set; } = null!;

    public string? OverallFeedback { get; set; }

    public DateTimeOffset ReviewedAt { get; set; }

    public Rubric Rubric { get; set; } = null!;

    public ICollection<HumanReviewCriterionScore> CriterionScores { get; set; } = new List<HumanReviewCriterionScore>();

}
