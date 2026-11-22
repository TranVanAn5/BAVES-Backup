using BAVES.Api.Modules.Simulation.Models.Enums;

namespace BAVES.Api.Modules.Simulation.Models.DTOs;

/// <summary>
/// Thông tin tóm tắt của một Simulation Session trả về cho Learner.
/// </summary>
public sealed record SessionDto(
    int Id,
    int ScenarioVersionId,
    int? AssignmentId,
    int AttemptNo,
    SessionStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<SessionAgentDto> Agents
);

/// <summary>
/// Thông tin lịch sử phiên thực hành của Learner trên Dashboard (hỗ trợ lọc theo trạng thái, kịch bản, bài tập).
/// </summary>
public sealed record SessionHistoryItemDto(
    int Id,
    int ScenarioVersionId,
    int? AssignmentId,
    int AttemptNo,
    SessionStatus Status,
    int AgentCount,
    int MessageCount,
    DateTimeOffset StartedAt,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? CompletedAt
);

/// <summary>
/// Một stakeholder persona đã được gắn vào session, kèm trạng thái ban đầu.
/// </summary>
public sealed record SessionAgentDto(
    int Id,
    int PersonaId,
    string Name,
    string StakeholderRole,
    int TrustLevel,
    int PatienceLevel,
    int CooperationLevel
);

/// <summary>
/// Thông tin tóm tắt phiên làm bài của Learner gắn với Assignment.
/// </summary>
public sealed record AssignmentSessionSnapshotDto(
    int SessionId,
    int AssignmentId,
    int AttemptNo,
    SessionStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? CompletedAt
);

/// <summary>
/// Thông tin phiên mô phỏng dùng cho tổng hợp Sổ điểm (Gradebook) và Phân tích Lớp học (Cohort Analytics).
/// </summary>
public sealed record LearnerSessionSummaryDto(
    int SessionId,
    int LearnerId,
    int ScenarioVersionId,
    int? AssignmentId,
    int AttemptNo,
    SessionStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? SubmittedAt,
    DateTimeOffset? CompletedAt
);
