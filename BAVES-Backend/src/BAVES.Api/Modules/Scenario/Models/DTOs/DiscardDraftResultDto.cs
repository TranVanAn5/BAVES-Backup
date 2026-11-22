namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Kết quả hủy bản nháp ScenarioVersion.
/// </summary>
public sealed record DiscardDraftResultDto(
    int DiscardedVersionId,
    int ScenarioId,
    bool ScenarioDeleted,
    int? FallbackVersionId,
    int? FallbackVersionNo,
    string? FallbackSummary
);
