namespace BAVES.Api.Modules.Scenario.Models.DTOs;

public sealed record ScenarioSprintDto(
    int SprintNo,
    string Focus,
    int DurationValue,
    string DurationUnit,
    IReadOnlyList<string> TemplateCodes
);

public sealed record ScenarioSprintListDto(
    int ScenarioVersionId,
    int ScenarioId,
    IReadOnlyList<ScenarioSprintDto> Sprints
);
