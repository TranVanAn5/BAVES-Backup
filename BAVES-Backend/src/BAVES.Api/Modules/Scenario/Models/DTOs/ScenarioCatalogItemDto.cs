using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Một scenario trong danh sách catalogue, do module Scenario sở hữu và cung cấp
/// cho các module khác (ví dụ Content) để dựng màn hình danh sách kịch bản.
/// </summary>
public sealed record ScenarioCatalogItemDto(
    int Id,
    int ApplicationId,
    string Code,
    string Title,
    string? Description,
    ScenarioStatus Status,
    bool HasPublishedVersion,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    int? PublishedVersionId,
    string? Difficulty = null,
    bool IsFreeAccess = false
);