using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Models.DTOs;

/// <summary>
/// Thông tin tóm tắt của một Lớp học (Cohort) dùng trong danh sách quản lý của Giảng viên/Admin.
/// </summary>
public sealed record CohortSummaryDto(
    int Id,
    int InstructorId,
    string Code,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    CohortStatus Status,
    int ActiveMemberCount,
    int AssignmentCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    int PendingRequestCount = 0
);

/// <summary>
/// Thông tin chi tiết của một Lớp học (Cohort) kèm thống kê sĩ số và bài tập.
/// </summary>
public sealed record CohortDetailDto(
    int Id,
    int InstructorId,
    string Code,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    CohortStatus Status,
    int ActiveMemberCount,
    int ActiveAssignmentCount,
    int TotalAssignmentCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

/// <summary>
/// Thông tin Lớp học dưới góc nhìn của Học viên (Learner) khi xem danh sách lớp mình tham gia.
/// </summary>
public sealed record LearnerCohortDto(
    int CohortId,
    int InstructorId,
    string Code,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    CohortStatus CohortStatus,
    CohortMemberStatus MemberStatus,
    DateTimeOffset JoinedAt,
    int ActiveAssignmentCount,
    string? InstructorName = null,
    string? InstructorEmail = null
);

/// <summary>
/// Thông tin Lớp học công khai để Học viên tìm kiếm và gửi yêu cầu tham gia lớp của Giảng viên.
/// </summary>
public sealed record AvailableCohortDto(
    int Id,
    int InstructorId,
    string InstructorName,
    string? InstructorEmail,
    string Code,
    string Name,
    string? Description,
    DateOnly? StartDate,
    DateOnly? EndDate,
    CohortStatus Status,
    int ActiveMemberCount,
    int AssignmentCount,
    CohortMemberStatus? CurrentMemberStatus
);
