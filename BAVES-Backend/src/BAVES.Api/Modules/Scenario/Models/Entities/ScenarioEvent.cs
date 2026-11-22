using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.Entities;

public class ScenarioEvent
{
    public int Id { get; set; }

    public int ScenarioVersionId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public ScenarioEventTriggerType TriggerType { get; set; }

    public string? TriggerConditionJson { get; set; }

    public string EventContent { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ScenarioVersion ScenarioVersion { get; set; } = null!;

}
