using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.Entities;

public class KnowledgeItem
{
    public int Id { get; set; }

    public int ScenarioVersionId { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public KnowledgeCategory Category { get; set; }

    public KnowledgeImportance Importance { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public ScenarioVersion ScenarioVersion { get; set; } = null!;

    public ICollection<StakeholderKnowledgeAccess> StakeholderKnowledgeAccess { get; set; } = new List<StakeholderKnowledgeAccess>();

}
