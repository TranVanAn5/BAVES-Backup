using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.Entities;

public class ScenarioBusinessRule
{
    public int Id { get; set; }

    public int ScenarioVersionId { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsHidden { get; set; }

    public ImportanceLevel Importance { get; set; }

    public ScenarioVersion ScenarioVersion { get; set; } = null!;

}
