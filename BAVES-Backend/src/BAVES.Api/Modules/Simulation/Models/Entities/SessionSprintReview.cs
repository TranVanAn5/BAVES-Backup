namespace BAVES.Api.Modules.Simulation.Models.Entities;

/// <summary>
/// Một sprint của phiên mô phỏng: hội đồng stakeholder phản biện artifact,
/// rồi chốt điểm và bản tóm tắt khi learner kết thúc review.
/// </summary>
public class SessionSprintReview
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int SprintNo { get; set; }

    public string Status { get; set; } = "Open";

    public string DiscussionJson { get; set; } = "[]";

    public string? ChairSpeakerName { get; set; }

    public string? ChairReply { get; set; }

    public string ChallengeQuestionsJson { get; set; } = "[]";

    public string? Decision { get; set; }

    public decimal? SprintScore { get; set; }

    public string? CompressedDigest { get; set; }

    public string ApprovedPointsJson { get; set; } = "[]";

    public string ActionItemsJson { get; set; } = "[]";

    /// <summary>
    /// Snapshot artifact đã nộp khi chốt sprint (JSON) để so sánh cải thiện ở sprint sau.
    /// </summary>
    public string? ArtifactSnapshotJson { get; set; }

    public string? PromptVersion { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? FinalizedAt { get; set; }

    public SimulationSession Session { get; set; } = null!;
}
