// DTO cho contract của AI Runtime ở endpoint POST /v1/coach/feedback.
// Tách khỏi CoachFeedbackDtos.cs của module Simulation: file này là "hình dạng
// dữ liệu trao đổi với AI runtime", còn bên Simulation là DTO trả về cho client.

namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Payload gửi sang AI Runtime (Python) endpoint POST /v1/coach/feedback.
/// Mirror đúng schema của agent-runtime/app/schemas/coach.py.
/// </summary>
public sealed record CoachFeedbackRequest(
    int SessionId,
    CoachBaPlanContextDto? BaPlan,
    IReadOnlyList<CoachChatMessageDto> RecentMessages,
    CoachAiConnectionDto AiConnection);

/// <summary>
/// Ngữ cảnh kế hoạch BA của learner. null nếu learner chưa lập kế hoạch.
/// </summary>
public sealed record CoachBaPlanContextDto(
    string? Objective,
    string? Approach,
    int PlannedQuestionsCount = 0);

/// <summary>
/// Một lượt hội thoại trong cửa sổ hội thoại gửi cho coach.
/// <c>SenderType</c> chỉ nhận "Learner" hoặc "Stakeholder" theo Literal của Python.
/// </summary>
public sealed record CoachChatMessageDto(
    string SenderType,
    string Content,
    string? CreatedAt);

/// <summary>
/// Kết nối AI dùng cho lần gọi coach này (BYOK — không lưu vào DB).
/// </summary>
public sealed record CoachAiConnectionDto(
    string Provider,
    string? Model,
    string? ApiKey);

/// <summary>
/// Response từ AI Runtime endpoint POST /v1/coach/feedback.
/// Mirror đúng schema của agent-runtime/app/schemas/coach.py.
/// </summary>
public sealed record CoachFeedbackResponse(
    int SessionId,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Gaps,
    IReadOnlyList<string> UnclearPoints,
    IReadOnlyList<string> ImprovementHints,
    string? PromptVersion,
    StakeholderUsageDto? Usage,
    string? Error);
