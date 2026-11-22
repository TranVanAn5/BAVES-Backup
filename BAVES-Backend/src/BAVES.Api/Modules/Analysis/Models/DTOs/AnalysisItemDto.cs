using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Planning.Models.Enums;

namespace BAVES.Api.Modules.Analysis.Models.DTOs;

/// <summary>
/// Một mục phân tích (Analysis Item) của Learner trong một Simulation Session.
/// Gồm các loại: Finding, Requirement, BusinessRule, Constraint, Risk,
/// OpenQuestion, Conflict, Assumption.
/// </summary>
public sealed record AnalysisItemDto(
    int Id,
    int SessionId,
    int? ParentAnalysisItemId,
    AnalysisItemType ItemType,
    string Code,
    string Title,
    string Description,
    PriorityLevel? Priority,
    AnalysisItemStatus Status,
    string? MetadataJson,
    int CreatedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
