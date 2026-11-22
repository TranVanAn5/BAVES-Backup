using System.Text.Json.Serialization;

namespace BAVES.Api.Modules.Assessment.Models.DTOs;

// ─────────── RUNTIME DTOS (Giao tiếp với AI Runtime /v1/assessment/evaluate) ───────────

public sealed record AssessmentRubricCriterionDto(
    [property: JsonPropertyName("criterionId")] int? CriterionId,
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("weight")] double Weight,
    [property: JsonPropertyName("maxScore")] double MaxScore
);

public sealed record AssessmentRubricDto(
    [property: JsonPropertyName("rubricId")] int? RubricId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("criteria")] List<AssessmentRubricCriterionDto> Criteria
);

public sealed record AssessmentArtifactSectionDto(
    [property: JsonPropertyName("sectionKey")] string SectionKey,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("content")] string Content
);

public sealed record AssessmentArtifactSubmissionDto(
    [property: JsonPropertyName("artifactId")] int? ArtifactId,
    [property: JsonPropertyName("artifactType")] string ArtifactType,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("versionNo")] int VersionNo,
    [property: JsonPropertyName("sections")] List<AssessmentArtifactSectionDto> Sections
);

public sealed record AssessmentEvidenceDto(
    [property: JsonPropertyName("evidenceId")] int? EvidenceId,
    [property: JsonPropertyName("messageContent")] string MessageContent,
    [property: JsonPropertyName("stakeholderName")] string? StakeholderName,
    [property: JsonPropertyName("linkedItemTitle")] string? LinkedItemTitle
);

public sealed record AssessmentAiConnectionDto(
    [property: JsonPropertyName("provider")] string Provider,
    [property: JsonPropertyName("model")] string? Model,
    [property: JsonPropertyName("apiKey")] string? ApiKey
);

public sealed record AssessmentRuntimeRequest(
    [property: JsonPropertyName("submissionId")] int SubmissionId,
    [property: JsonPropertyName("artifact")] AssessmentArtifactSubmissionDto? Artifact,
    [property: JsonPropertyName("artifacts")] List<AssessmentArtifactSubmissionDto> Artifacts,
    [property: JsonPropertyName("rubric")] AssessmentRubricDto Rubric,
    [property: JsonPropertyName("evidenceList")] List<AssessmentEvidenceDto> EvidenceList,
    [property: JsonPropertyName("aiConnection")] AssessmentAiConnectionDto AiConnection
);

public sealed record AssessmentCriterionScoreDto(
    [property: JsonPropertyName("criterionCode")] string CriterionCode,
    [property: JsonPropertyName("score")] double Score,
    [property: JsonPropertyName("maxScore")] double MaxScore,
    [property: JsonPropertyName("comment")] string Comment
);

public sealed record AssessmentRuntimeResponse(
    [property: JsonPropertyName("submissionId")] int SubmissionId,
    [property: JsonPropertyName("overallScore")] double OverallScore,
    [property: JsonPropertyName("passed")] bool Passed,
    [property: JsonPropertyName("criterionScores")] List<AssessmentCriterionScoreDto> CriterionScores,
    [property: JsonPropertyName("strengths")] List<string> Strengths,
    [property: JsonPropertyName("weaknesses")] List<string> Weaknesses,
    [property: JsonPropertyName("criticalErrors")] List<string> CriticalErrors,
    [property: JsonPropertyName("expertReviewAvailable")] bool ExpertReviewAvailable
);

// ─────────── CLIENT / API RESPONSE DTOS ───────────

public sealed record AIFeedbackCriterionScoreDto(
    int RubricCriterionId,
    string CriterionCode,
    string CriterionName,
    decimal Score,
    decimal MaxScore,
    string? Comment,
    string? EvidenceSnippet,
    int? BabokTaskId = null,
    string? BabokTaskCode = null,
    string? KnowledgeAreaCode = null,
    string? KnowledgeAreaName = null
);

public sealed record AIPracticeFeedbackDto(
    int Id,
    int SubmissionId,
    string Status,
    decimal? SuggestedScore,
    decimal? Confidence,
    string? OverallFeedback,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Weaknesses,
    IReadOnlyList<string> CriticalErrors,
    IReadOnlyList<AIFeedbackCriterionScoreDto> CriterionScores,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt
);
