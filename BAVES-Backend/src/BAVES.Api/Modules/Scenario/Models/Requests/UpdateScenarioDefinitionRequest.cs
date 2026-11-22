using System.ComponentModel.DataAnnotations;
using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.Requests;

public sealed record UpdateKnowledgeAccessRequest
{
    [Required]
    public string PersonaCode { get; init; } = null!;

    public KnowledgeAccessType AccessType { get; init; } = KnowledgeAccessType.Knows;

    public string? DisclosureRule { get; init; }

    public bool IsHidden { get; init; } = false;
}

public sealed record UpdateKnowledgeItemRequest
{
    [Required]
    [MaxLength(80)]
    public string Code { get; init; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = null!;

    [Required]
    public string Content { get; init; } = null!;

    public KnowledgeCategory Category { get; init; } = KnowledgeCategory.Process;

    public KnowledgeImportance Importance { get; init; } = KnowledgeImportance.Medium;

    public List<UpdateKnowledgeAccessRequest> AccessList { get; init; } = [];
}

public sealed record UpdatePersonaRequest
{
    [Required]
    [MaxLength(80)]
    public string Code { get; init; } = null!;

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = null!;

    [Required]
    [MaxLength(200)]
    public string StakeholderRole { get; init; } = null!;

    public string? Department { get; init; }

    public string? Responsibilities { get; init; }

    public string? Goals { get; init; }

    public string? PainPoints { get; init; }

    public string? BusinessConcerns { get; init; }

    public string? CommunicationStyle { get; init; }

    public string? Personality { get; init; }

    public int DisplayOrder { get; init; } = 1;

    public bool IsActive { get; init; } = true;
}

public sealed record UpdateBusinessRuleRequest
{
    [Required]
    [MaxLength(80)]
    public string Code { get; init; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = null!;

    [Required]
    public string Description { get; init; } = null!;

    public bool IsHidden { get; init; } = false;

    public ImportanceLevel Importance { get; init; } = ImportanceLevel.Medium;
}

public sealed record UpdateScenarioEventRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = null!;

    [Required]
    public string Description { get; init; } = null!;

    public ScenarioEventTriggerType TriggerType { get; init; } = ScenarioEventTriggerType.AfterSprintEnds;

    public string? TriggerConditionJson { get; init; }

    [Required]
    public string EventContent { get; init; } = null!;

    public int DisplayOrder { get; init; } = 1;

    public bool IsActive { get; init; } = true;
}

public sealed record UpdateScenarioDefinitionRequest
{
    public string? Summary { get; init; }

    [Required]
    public string BusinessContext { get; init; } = null!;

    [Required]
    public string BusinessProblem { get; init; } = null!;

    [Required]
    public string BusinessObjectives { get; init; } = null!;

    [Required]
    [MaxLength(500)]
    public string BARole { get; init; } = "Business Analyst";

    /// <summary>Deprecated — bỏ mô tả Assignment trên scenario; lộ trình dùng Sprints. Luôn lưu rỗng.</summary>
    public string? Assignment { get; init; }

    public string? InScope { get; init; }

    public string? OutOfScope { get; init; }

    public string? KnownConstraints { get; init; }

    public ScenarioDifficulty Difficulty { get; init; } = ScenarioDifficulty.Intermediate;

    /// <summary>Deprecated — server tự gắn theo role creator (Admin = library, Instructor = riêng).</summary>
    public bool IsFreeAccess { get; init; }

    public List<UpdatePersonaRequest> Personas { get; init; } = [];

    public List<UpdateKnowledgeItemRequest> KnowledgeItems { get; init; } = [];

    public List<UpdateBusinessRuleRequest> BusinessRules { get; init; } = [];

    public List<UpdateScenarioEventRequest> Events { get; init; } = [];

    /// <summary>Lộ trình sprint (bảng ScenarioSprints). Null = không đụng sprints khi cập nhật.</summary>
    public List<UpsertScenarioSprintRequest>? Sprints { get; init; }
}
