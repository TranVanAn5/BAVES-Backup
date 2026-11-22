
namespace BAVES.Api.Modules.Scenario.Models.Entities;

public class StakeholderPersona
{
    public int Id { get; set; }

    public int ScenarioVersionId { get; set; }

    public string Name { get; set; } = null!;

    public string StakeholderRole { get; set; } = null!;

    public string? Department { get; set; }

    public string? Responsibilities { get; set; }

    public string? Goals { get; set; }

    public string? PainPoints { get; set; }

    public string? BusinessConcerns { get; set; }

    public string? CommunicationStyle { get; set; }

    public string? Personality { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ScenarioVersion ScenarioVersion { get; set; } = null!;

    public ICollection<StakeholderKnowledgeAccess> StakeholderKnowledgeAccess { get; set; } = new List<StakeholderKnowledgeAccess>();

}
