namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class RubricCriterion
{
    public int Id { get; set; }

    public int RubricId { get; set; }

    public int BabokTaskId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Weight { get; set; }

    public decimal MaxScore { get; set; }

    public string? EvidenceGuidance { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsRequired { get; set; }

    public Rubric Rubric { get; set; } = null!;

    public BabokTask BabokTask { get; set; } = null!;

    public ICollection<AIFeedbackCriterionScore> AIFeedbackCriterionScores { get; set; } = new List<AIFeedbackCriterionScore>();

    public ICollection<HumanReviewCriterionScore> HumanReviewCriterionScores { get; set; } = new List<HumanReviewCriterionScore>();

}
