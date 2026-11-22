using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.DTOs;

public sealed record ScenarioVersionSummaryDto(
    int Id,
    int ScenarioId,
    int VersionNo,
    string? Summary,
    ScenarioDifficulty Difficulty,
    bool IsPublished,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt
);
