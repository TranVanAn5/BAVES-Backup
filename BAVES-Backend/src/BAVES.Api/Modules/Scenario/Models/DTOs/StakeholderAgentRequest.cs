namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Payload gửi sang AI Runtime (Python) endpoint POST /v1/stakeholder/respond.
/// Mirror đúng schema của agent-runtime/app/schemas/stakeholder.py.
/// </summary>
public sealed record StakeholderAgentRequest(
    string RequestId,
    int SessionId,
    StakeholderScenarioContextDto Scenario,
    StakeholderPersonaDto Persona,
    IReadOnlyList<StakeholderKnowledgeItemDto> AllowedKnowledge,
    StakeholderAgentStateDto AgentState,
    IReadOnlyList<StakeholderChatMessageDto> History,
    string LearnerMessage,
    IReadOnlyList<StakeholderEventContextDto> EventContext,
    StakeholderAiConnectionDto AiConnection);

/// <summary>
/// Ngữ cảnh kinh doanh của scenario, phía AI runtime dùng để dựng prompt.
/// </summary>
public sealed record StakeholderScenarioContextDto(
    int ScenarioVersionId,
    string? CompanyName,
    string BusinessContext,
    string BusinessProblem,
    string Assignment);

/// <summary>
/// Hồ sơ persona đầy đủ — chỉ gửi cho AI runtime, không trả về cho Learner.
/// </summary>
public sealed record StakeholderPersonaDto(
    int PersonaId,
    string Name,
    string StakeholderRole,
    string? Department,
    string? Responsibilities,
    string? Goals,
    string? PainPoints,
    string? BusinessConcerns,
    string? CommunicationStyle,
    string? Personality);

/// <summary>
/// Một knowledge item mà persona được phép biết, đã lọc theo StakeholderKnowledgeAccess.
/// Bao gồm cả cờ isHidden / disclosureRule để AI biết cách phản ứng đúng.
/// </summary>
public sealed record StakeholderKnowledgeItemDto(
    int KnowledgeItemId,
    string Code,
    string Title,
    string Content,
    string Category,
    string Importance,
    string AccessType,
    bool IsHidden,
    string? DisclosureRule);

/// <summary>
/// Trạng thái hiện tại của agent trong cuộc phỏng vấn.
/// </summary>
public sealed record StakeholderAgentStateDto(
    string? MemorySummary,
    int TrustLevel,
    int PatienceLevel,
    int CooperationLevel);

/// <summary>
/// Một tin nhắn trong lịch sử phỏng vấn.
/// </summary>
public sealed record StakeholderChatMessageDto(
    string SenderType,
    string Content,
    string? CreatedAt);

/// <summary>
/// Sự kiện ngữ cảnh có thể ảnh hưởng tới câu trả lời của stakeholder.
/// </summary>
public sealed record StakeholderEventContextDto(
    string Title,
    string Description);

/// <summary>
/// Kết nối AI dùng cho lần gọi này (BYOK — không lưu vào DB).
/// </summary>
public sealed record StakeholderAiConnectionDto(
    string Provider,
    string? Model,
    string? ApiKey);
