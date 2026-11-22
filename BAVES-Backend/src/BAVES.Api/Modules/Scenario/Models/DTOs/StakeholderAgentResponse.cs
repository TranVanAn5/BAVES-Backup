namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Response từ AI Runtime (Python) endpoint POST /v1/stakeholder/respond.
/// Mirror đúng schema của agent-runtime/app/schemas/stakeholder.py.
/// </summary>
public sealed record StakeholderAgentResponse(
    string RequestId,
    string Reply,
    IReadOnlyList<int> UsedKnowledgeItemIds,
    StakeholderAgentStateDto StateUpdate,
    string PromptVersion,
    StakeholderUsageDto Usage,
    string? Error);

/// <summary>
/// Thống kê token và độ trễ của một lần gọi AI runtime.
/// </summary>
public sealed record StakeholderUsageDto(
    int PromptTokens,
    int CompletionTokens,
    int LatencyMs);
