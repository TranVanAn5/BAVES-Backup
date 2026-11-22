using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.Entities;

public class StakeholderKnowledgeAccess
{
    public int Id { get; set; }

    public int StakeholderPersonaId { get; set; }

    public int KnowledgeItemId { get; set; }

    public KnowledgeAccessType AccessType { get; set; }

    public string? DisclosureRule { get; set; }

    public bool IsHidden { get; set; }

    public StakeholderPersona StakeholderPersona { get; set; } = null!;

    public KnowledgeItem KnowledgeItem { get; set; } = null!;

}
