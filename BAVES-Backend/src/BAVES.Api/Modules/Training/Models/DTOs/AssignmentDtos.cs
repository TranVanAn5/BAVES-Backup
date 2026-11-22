using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Models.DTOs;

/// <summary>
/// Thông tin tóm tắt của một Bài tập (Assignment) dùng trong danh sách quản lý của Giảng viên/Admin.
/// </summary>
public sealed record AssignmentSummaryDto(
    int Id,
    string Title,
    int ScenarioVersionId,
    int? CohortId,
    string? CohortCode,
    string? CohortName,
    int? LearnerId,
    int? RubricId,
    int AssignedByUserId,
    DateTimeOffset? StartAt,
    DateTimeOffset? DueAt,
    int? MaxAttempts,
    AssignmentStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

/// <summary>
/// Thông tin chi tiết của một Bài tập (Assignment).
/// </summary>
public sealed record AssignmentDetailDto(
    int Id,
    string Title,
    string? Instructions,
    int ScenarioVersionId,
    int? CohortId,
    string? CohortCode,
    string? CohortName,
    int? LearnerId,
    int? RubricId,
    int AssignedByUserId,
    DateTimeOffset? StartAt,
    DateTimeOffset? DueAt,
    int? MaxAttempts,
    AssignmentStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

/// <summary>
/// Thông tin Bài tập hiển thị cho Học viên kèm tiến độ thực hiện và số lần làm bài còn lại.
/// </summary>
public sealed record LearnerAssignmentDto(
    int Id,
    string Title,
    string? Instructions,
    int ScenarioVersionId,
    int? CohortId,
    string? CohortCode,
    string? CohortName,
    DateTimeOffset? StartAt,
    DateTimeOffset? DueAt,
    int? MaxAttempts,
    AssignmentStatus AssignmentStatus,
    string ProgressStatus,
    int AttemptsUsed,
    int? RemainingAttempts,
    int? LatestSessionId,
    bool IsOverdue
);
