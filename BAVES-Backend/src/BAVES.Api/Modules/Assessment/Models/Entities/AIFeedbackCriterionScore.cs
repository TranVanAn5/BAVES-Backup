namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class AIFeedbackCriterionScore
{
    public int AIFeedbackId { get; set; }

    public int RubricCriterionId { get; set; }

    public decimal Score { get; set; }

    public string? Comment { get; set; }

    public string? EvidenceSnippet { get; set; }

    public AIPracticeFeedback AIFeedback { get; set; } = null!;

    public RubricCriterion RubricCriterion { get; set; } = null!;
}
