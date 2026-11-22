namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Payload gửi sang AI Runtime (Python) endpoint POST /v1/analysis/confirm.
/// Mirror đúng schema của agent-runtime/app/schemas/confirmation.py.
/// </summary>
public sealed record AnalysisConfirmRuntimeRequest(
    StakeholderPersonaDto Persona,
    AnalysisConfirmItemPayloadDto Item,
    string EvidenceContent,
    IReadOnlyList<StakeholderKnowledgeItemDto> AllowedKnowledge,
    StakeholderAiConnectionDto AiConnection
);

/// <summary>
/// Thông tin mục phân tích gửi cho AI Stakeholder thẩm định.
/// </summary>
public sealed record AnalysisConfirmItemPayloadDto(
    int? ItemId,
    string ItemType,
    string Title,
    string Description,
    string? Importance = "Medium"
);

/// <summary>
/// Kết quả thẩm định từ AI Runtime endpoint POST /v1/analysis/confirm.
/// Outcome nhận một trong ba giá trị: "Confirmed", "Disputed", "InsufficientInformation".
/// </summary>
public sealed record AnalysisConfirmRuntimeResponse(
    string Outcome,
    string Reasoning,
    string? SuggestedClarification,
    string? PromptVersion,
    StakeholderUsageDto? Usage,
    string? Error
);
