namespace BAVES.Api.Modules.Scenario.Models.DTOs;

public sealed class SprintReviewRuntimeResponse
{
    public string? RequestId { get; set; }
    public int SessionId { get; set; }
    public int SprintNo { get; set; }
    public string ChairSpeakerName { get; set; } = "Hội đồng";
    public string ChairReply { get; set; } = "";
    public List<string> ChallengeQuestions { get; set; } = [];
    public SprintReviewRuntimeSummary? SprintSummary { get; set; }
    public string? PromptVersion { get; set; }
    public string? Error { get; set; }
}

public sealed class SprintReviewRuntimeSummary
{
    public int SprintNo { get; set; }
    public string Decision { get; set; } = "ApprovedWithRevisions";
    public decimal SprintScore { get; set; }
    public List<string> ApprovedPoints { get; set; } = [];
    public List<string> ActionItemsForNextSprint { get; set; } = [];
    public string CompressedDigest { get; set; } = "";
    public bool NextSprintUnlocked { get; set; } = true;
}
