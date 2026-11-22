using BAVES.Api.Modules.Assessment.Models.Enums;

namespace BAVES.Api.Modules.Assessment.Models.DTOs;

public sealed record BabokTaskDto(
    int Id,
    string KnowledgeAreaCode,
    string KnowledgeAreaName,
    string Code,
    string Name,
    string? Purpose,
    string? Description,
    string BabokVersion,
    int DisplayOrder
);

public sealed record RubricCriterionDetailDto(
    int Id,
    int RubricId,
    int BabokTaskId,
    string? BabokTaskCode,
    string? BabokTaskName,
    string Code,
    string Name,
    string Description,
    decimal Weight,
    decimal MaxScore,
    string? EvidenceGuidance,
    int DisplayOrder,
    bool IsRequired
);

public sealed record RubricDetailDto(
    int Id,
    int ScenarioVersionId,
    int CreatedByUserId,
    string Name,
    int Version,
    string? Description,
    RubricStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    IReadOnlyList<RubricCriterionDetailDto> Criteria
);
