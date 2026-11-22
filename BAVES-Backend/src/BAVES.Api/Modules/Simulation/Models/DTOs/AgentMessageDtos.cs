using BAVES.Api.Modules.Simulation.Models.Enums;

namespace BAVES.Api.Modules.Simulation.Models.DTOs;

/// <summary>
/// Kết nối AI dùng cho một lần hỏi stakeholder (BYOK).
/// Giá trị đến từ header X-AI-Provider / X-AI-Model / X-AI-Key của request và
/// chỉ được chuyển tiếp sang AI Runtime — không lưu vào DB, không ghi log.
/// </summary>
public sealed record AgentAiConnection(
    string Provider,
    string? Model,
    string? ApiKey);

/// <summary>
/// Một tin nhắn trong transcript phỏng vấn của một stakeholder agent.
/// Frontend dùng để hiển thị lịch sử hội thoại và gắn tin nhắn làm bằng chứng
/// cho các mục phân tích (evidence).
/// </summary>
public sealed record AgentMessageDto(
    int Id,
    int SessionId,
    int? SessionAgentId,
    MessageSenderType SenderType,
    string Content,
    DateTimeOffset CreatedAt);

/// <summary>
/// Một trang transcript của agent, kèm thông tin phân trang.
/// </summary>
public sealed record AgentMessagePageDto(
    IReadOnlyList<AgentMessageDto> Items,
    int Page,
    int PageSize,
    int Total);

/// <summary>
/// Kết quả gửi tin nhắn tới stakeholder: tin nhắn của learner và câu trả lời
/// của AI stakeholder, kèm các knowledge item mà AI đã dùng để trả lời.
/// </summary>
public sealed record SendAgentMessageResultDto(
    AgentMessageDto LearnerMessage,
    AgentMessageDto StakeholderMessage,
    IReadOnlyList<int> UsedKnowledgeItemIds);
