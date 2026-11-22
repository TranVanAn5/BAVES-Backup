namespace BAVES.Api.Modules.Training.Models.DTOs;

/// <summary>
/// Một dòng trong Sổ điểm (Gradebook) tương ứng với một Học viên được giao bài tập.
/// </summary>
public sealed record GradebookEntryDto(
    int LearnerId,
    string FullName,
    string? Email,
    int AttemptsUsed,
    int? LatestSessionId,
    int? LatestSubmissionId,
    string ProgressStatus,
    bool IsLate,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? FinalizedAt,
    decimal? AiSuggestedScore,
    decimal? ExpertFinalScore,
    int? ReviewerId
);

/// <summary>
/// Toàn bộ Sổ điểm và Tiến độ thực hiện của một Bài tập (Assignment Gradebook).
/// </summary>
public sealed record AssignmentGradebookDto(
    int AssignmentId,
    string Title,
    int ScenarioVersionId,
    int? CohortId,
    string? CohortCode,
    string? CohortName,
    DateTimeOffset? DueAt,
    int? MaxAttempts,
    int TotalRecipients,
    int NotStartedCount,
    int InProgressCount,
    int SubmittedCount,
    int GradedCount,
    int LateSubmissionCount,
    decimal? AverageAiScore,
    decimal? AverageFinalScore,
    IReadOnlyList<GradebookEntryDto> Entries
);

/// <summary>
/// Thống kê điểm chuẩn theo từng tiêu chí Rubric BABOK của cả Lớp học.
/// </summary>
public sealed record CohortCriterionBenchmarkDto(
    string CriterionCode,
    string CriterionName,
    decimal MaxScore,
    int EvaluatedCount,
    decimal AverageScore,
    decimal AverageAchievementPercent,
    int BelowThresholdCount
);

/// <summary>
/// Tổng hợp kết quả học tập của từng Học viên trong Lớp học.
/// </summary>
public sealed record CohortLearnerAnalyticsDto(
    int LearnerId,
    string FullName,
    string? Email,
    int SessionsStartedCount,
    int SubmissionsCount,
    int FinalizedReviewsCount,
    decimal? AverageAiScore,
    decimal? AverageFinalScore
);

/// <summary>
/// Báo cáo phân tích năng lực toàn diện của một Lớp học (Cohort Analytics).
/// </summary>
public sealed record CohortAnalyticsDto(
    int CohortId,
    string Code,
    string Name,
    int ActiveMemberCount,
    int TotalAssignmentCount,
    int TotalSessionsStarted,
    int TotalSubmissionsCount,
    int TotalFinalizedReviewsCount,
    decimal? ClassAverageAiScore,
    decimal? ClassAverageExpertScore,
    decimal CompletionRatePercent,
    IReadOnlyList<CohortCriterionBenchmarkDto> CriterionBenchmarks,
    IReadOnlyList<CohortLearnerAnalyticsDto> LearnerSummaries
);
