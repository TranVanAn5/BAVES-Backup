using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.DTOs;

public sealed record KnowledgeAccessDto(
    int PersonaId,
    string PersonaName,
    KnowledgeAccessType AccessType,
    string? DisclosureRule,
    bool IsHidden
);

public sealed record AuthoringKnowledgeItemDto(
    int Id,
    string Code,
    string Title,
    string Content,
    KnowledgeCategory Category,
    KnowledgeImportance Importance,
    List<KnowledgeAccessDto> AccessList
);

public sealed record AuthoringPersonaDto(
    int Id,
    string Name,
    string StakeholderRole,
    string? Department,
    string? Responsibilities,
    string? Goals,
    string? PainPoints,
    string? BusinessConcerns,
    string? CommunicationStyle,
    string? Personality,
    int DisplayOrder,
    bool IsActive
);

public sealed record AuthoringBusinessRuleDto(
    int Id,
    string Code,
    string Title,
    string Description,
    bool IsHidden,
    ImportanceLevel Importance
);

public sealed record AuthoringScenarioEventDto(
    int Id,
    string Title,
    string Description,
    ScenarioEventTriggerType TriggerType,
    string? TriggerConditionJson,
    string EventContent,
    int DisplayOrder,
    bool IsActive
);

public sealed record ScenarioDefinitionDto(
    int Id,
    int ScenarioId,
    string ScenarioCode,
    string ScenarioTitle,
    int VersionNo,
    string? Summary,
    string BusinessContext,
    string BusinessProblem,
    string BusinessObjectives,
    string BARole,
    string Assignment,
    string? InScope,
    string? OutOfScope,
    string? KnownConstraints,
    ScenarioDifficulty Difficulty,
    bool IsFreeAccess,
    bool IsPublished,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PublishedAt,
    List<AuthoringPersonaDto> Personas,
    List<AuthoringKnowledgeItemDto> KnowledgeItems,
    List<AuthoringBusinessRuleDto> BusinessRules,
    List<AuthoringScenarioEventDto> Events,
    string ReviewState,
    string? ReviewComment,
    DateTimeOffset? SubmittedForReviewAt,
    DateTimeOffset? ReviewedAt,
    List<ScenarioSprintDto> Sprints = null!
);
