namespace BAVES.Api.Modules.Simulation.Models.DTOs;

public sealed record SprintDiscussionMessageDto(string Speaker, string Content);

public sealed record SprintTaskDto(
    int SprintNo,
    string Name,
    string Focus,
    string Status,
    decimal? Score,
    string? Decision,
    string? Digest,
    IReadOnlyList<string> ChallengeQuestions,
    IReadOnlyList<string> ImprovementNotes,
    string? ChairSpeakerName,
    string? ChairReply,
    IReadOnlyList<SprintDiscussionMessageDto> Discussion,
    int DurationValue = 1,
    string DurationUnit = "weeks",
    IReadOnlyList<string> RequiredTemplateCodes = null!,
    IReadOnlyList<string> SubmittedTemplateCodes = null!,
    IReadOnlyList<string> MissingTemplateCodes = null!
);

public sealed record SprintScoreLineDto(int SprintNo, decimal? Score, string? Decision, string? Digest);

public sealed record SprintFinalEvaluationDto(
    decimal? AverageScore,
    string Summary,
    IReadOnlyList<SprintScoreLineDto> Sprints
);

public sealed record PriorSprintReassessmentDto(
    int SprintNo,
    decimal? PreviousScore,
    decimal? ReassessedScore,
    bool Improved,
    string Notes
);

public sealed record SprintBoardDto(
    int SessionId,
    int CurrentSprintNo,
    int TotalSprints,
    bool ProjectFinalized,
    IReadOnlyList<SprintTaskDto> Sprints,
    SprintFinalEvaluationDto? FinalEvaluation,
    DateTimeOffset? CurrentSprintStartedAt,
    DateTimeOffset? CurrentSprintDueAt,
    int SprintDurationMinutes,
    bool IsOverdue,
    int? MinutesRemaining,
    bool ReviewRequired
);

public sealed record SprintReviewTurnDto(
    int SprintNo,
    string Status,
    string ChairSpeakerName,
    string ChairReply,
    IReadOnlyList<string> ChallengeQuestions,
    decimal? Score,
    string? Decision,
    string? Digest,
    bool ProjectFinalized,
    SprintFinalEvaluationDto? FinalEvaluation,
    string ReviewPhase,
    bool RevealNumericScore,
    IReadOnlyList<string> ImprovementNotes,
    PriorSprintReassessmentDto? PriorSprintReassessment
);
