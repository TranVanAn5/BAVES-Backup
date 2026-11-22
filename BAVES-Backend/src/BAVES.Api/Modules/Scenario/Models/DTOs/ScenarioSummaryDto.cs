using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.DTOs;

public sealed record ScenarioSummaryDto(
    int Id,
    int ApplicationId,
    string Code,
    string Title,
    string? Description,
    ScenarioStatus Status,
    int InitialVersionId,
    DateTimeOffset CreatedAt
);
