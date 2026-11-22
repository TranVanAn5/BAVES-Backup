namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class HumanReviewCriterionScore
{
    public int HumanReviewId { get; set; }

    public int RubricCriterionId { get; set; }

    public decimal Score { get; set; }

    public string? Comment { get; set; }

    public HumanReview HumanReview { get; set; } = null!;

    public RubricCriterion RubricCriterion { get; set; } = null!;
}
