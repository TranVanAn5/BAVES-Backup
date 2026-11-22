using BAVES.Api.Modules.Scenario.Models.Enums;

namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Thông tin tối thiểu về một ScenarioVersion, do module Scenario sở hữu và cung cấp
/// cho các module khác (ví dụ Simulation) mà không cần biết cấu trúc bảng
/// [scenario].[ScenarioVersions] hay entity EF của module này.
/// </summary>
public sealed record ScenarioVersionInfoDto(
    int Id,
    int ScenarioId,
    int VersionNo,
    ScenarioDifficulty Difficulty,
    bool IsPublished
);
