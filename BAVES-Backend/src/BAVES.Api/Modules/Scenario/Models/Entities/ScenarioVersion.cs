using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.Entities;

public class ScenarioVersion
{
    public int Id { get; set; }

    public int ScenarioId { get; set; }

    public int VersionNo { get; set; }

    public string? Summary { get; set; }

    public string BusinessContext { get; set; } = null!;

    public string BusinessProblem { get; set; } = null!;

    public string BusinessObjectives { get; set; } = null!;

    public string BARole { get; set; } = null!;

    /// <summary>
    /// Deprecated (giữ cột DB). Lộ trình nhiệm vụ BA nằm ở <see cref="Sprints"/>.
    /// Không nhầm với Training.Assignment (giao bài cho learner).
    /// </summary>
    public string Assignment { get; set; } = string.Empty;

    public string? InScope { get; set; }

    public string? OutOfScope { get; set; }

    public string? KnownConstraints { get; set; }

    public ScenarioDifficulty Difficulty { get; set; }

    /// <summary>
    /// Learner được chơi miễn phí (không cần mua gói). Chỉ dùng cho scenario Beginner được gắn tay.
    /// </summary>
    public bool IsFreeAccess { get; set; }

    public ScenarioStatus Status { get; set; } = ScenarioStatus.Draft;

    public bool IsPublished { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }

    public DateTimeOffset? SubmittedForReviewAt { get; set; }

    public int? ReviewedByUserId { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public string? ReviewComment { get; set; }

    public Scenario Scenario { get; set; } = null!;

    public ICollection<StakeholderPersona> StakeholderPersonas { get; set; } = new List<StakeholderPersona>();

    public ICollection<KnowledgeItem> KnowledgeItems { get; set; } = new List<KnowledgeItem>();

    public ICollection<ScenarioBusinessRule> BusinessRules { get; set; } = new List<ScenarioBusinessRule>();

    public ICollection<ScenarioEvent> ScenarioEvents { get; set; } = new List<ScenarioEvent>();

    public ICollection<ScenarioSprint> Sprints { get; set; } = new List<ScenarioSprint>();

}
