namespace BAVES.Api.Modules.Scenario.Models.DTOs;

public sealed record ScenarioReviewDecisionDto(
    int VersionId,
    int ScenarioId,
    string ScenarioCode,
    string ScenarioTitle,
    int VersionNo,
    string Status,
    bool IsPublished,
    string? ReviewComment,
    DateTimeOffset? SubmittedForReviewAt,
    DateTimeOffset? ReviewedAt,
    int? ReviewedByUserId
);

public sealed record ScenarioReviewQueueItemDto(
    int VersionId,
    int ScenarioId,
    string Code,
    string Title,
    string? Description,
    int VersionNo,
    string Status,
    string? Summary,
    string BusinessContext,
    string BusinessProblem,
    string BusinessObjectives,
    string BARole,
    string Assignment,
    string? InScope,
    string? OutOfScope,
    string? KnownConstraints,
    int PersonaCount,
    int KnowledgeItemCount,
    int BusinessRuleCount,
    int EventCount,
    int CreatedByUserId,
    DateTimeOffset? SubmittedForReviewAt,
    string? ReviewComment,
    DateTimeOffset? ReviewedAt = null,
    int? ReviewedByUserId = null
);
