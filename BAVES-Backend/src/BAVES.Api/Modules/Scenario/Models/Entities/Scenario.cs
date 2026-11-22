using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.Entities;

public class Scenario
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public int CreatedByUserId { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public ScenarioStatus Status { get; set; } = ScenarioStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ScenarioVersion> Versions { get; set; } = new List<ScenarioVersion>();

}
