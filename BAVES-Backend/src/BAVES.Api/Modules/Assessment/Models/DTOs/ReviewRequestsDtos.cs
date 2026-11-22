namespace BAVES.Api.Modules.Assessment.Models.DTOs;

public sealed record ReviewRequestDto(
    int Id,
    int SubmissionId,
    int LearnerId,
    int? AssignedReviewerId,
    string? RequestNote,
    string Status,
    DateTimeOffset RequestedAt,
    DateTimeOffset? AcceptedAt,
    DateTimeOffset? CompletedAt
);

public sealed record ArtifactSnapshotDto(
    int Id,
    string Title,
    string ArtifactType,
    int VersionNo,
    string? PlainTextContent,
    string? StructuredContentJson
);

public sealed record PlanSnapshotDto(
    string? Objective,
    string? Approach,
    string? ExpectedOutputs
);

public sealed record AnalysisItemSnapshotDto(
    int Id,
    string Code,
    string Title,
    string ItemType,
    string Status,
    string? Description
);

public sealed record EvidenceSnapshotDto(
    int LinkId,
    int MessageId,
    string ItemTitle,
    string? RelationshipType
);

public sealed record HumanReviewCriterionScoreDto(
    int RubricCriterionId,
    string CriterionCode,
    string CriterionName,
    decimal Score,
    decimal MaxScore,
    string? Comment,
    int? BabokTaskId = null,
    string? BabokTaskCode = null,
    string? KnowledgeAreaCode = null,
    string? KnowledgeAreaName = null
);

public sealed record HumanReviewDto(
    int Id,
    int SubmissionId,
    int RubricId,
    int ReviewerId,
    decimal FinalScore,
    string? OverallFeedback,
    IReadOnlyList<HumanReviewCriterionScoreDto> CriterionScores,
    DateTimeOffset ReviewedAt
);

public sealed record ReviewRubricCriterionDto(
    int Id,
    string Code,
    string Name,
    string Description,
    decimal Weight,
    decimal MaxScore
);

public sealed record SprintReviewEvidenceDto(
    int SprintNo,
    string Status,
    decimal? Score,
    string? Decision,
    string? Digest,
    IReadOnlyList<string> ChallengeQuestions
);

public sealed record ReviewCaseDossierDto(
    int ReviewRequestId,
    int SubmissionId,
    int SessionId,
    int LearnerId,
    string Status,
    string? RequestNote,
    DateTimeOffset RequestedAt,
    ArtifactSnapshotDto Artifact,
    PlanSnapshotDto? Plan,
    IReadOnlyList<AnalysisItemSnapshotDto> AnalysisItems,
    AIPracticeFeedbackDto? AIFeedback,
    IReadOnlyList<SprintReviewEvidenceDto> SprintReviews,
    IReadOnlyList<ReviewRubricCriterionDto> RubricCriteria
);

public sealed record FinalSubmissionReportDto(
    int SubmissionId,
    int SessionId,
    string Status,
    AIPracticeFeedbackDto? AIFeedback,
    HumanReviewDto? ExpertReview,
    decimal? FinalScore,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? FinalizedAt,
    IReadOnlyList<SprintReviewEvidenceDto> SprintReviews
);

public sealed record CompetencySessionScoreDto(
    int SessionId,
    int ScenarioVersionId,
    int AttemptNo,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    int? LatestSubmissionId,
    decimal? AiSuggestedScore,
    decimal? ExpertFinalScore,
    decimal? SprintAverageScore,
    decimal? DisplayScore
);

public sealed record CompetencyKnowledgeAreaDto(
    string Code,
    string Name,
    decimal? AverageScore,
    int EvidenceCount,
    string? Description = null,
    IReadOnlyList<string>? TaskCodes = null
);

public sealed record CompetencyProgressDto(
    int TotalSessions,
    int CompletedSessions,
    decimal? AverageDisplayScore,
    IReadOnlyList<CompetencySessionScoreDto> Sessions,
    IReadOnlyList<CompetencyKnowledgeAreaDto> KnowledgeAreas
);
