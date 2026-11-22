using BAVES.Api.Modules.Scenario.Models.DTOs;

namespace BAVES.Api.Modules.Scenario.Repositories.Interfaces;

/// <summary>
/// Truy vấn danh sách scenario (catalogue) từ bảng [scenario].[Scenarios].
/// Module Scenario là chủ sở hữu duy nhất của bảng này.
/// </summary>
public interface IScenarioCatalogRepository
{
    /// <summary>
    /// Lấy danh sách scenario published. Có thể lọc theo application và visibility theo role.
    /// </summary>
    Task<IReadOnlyList<ScenarioCatalogItemDto>> GetScenariosAsync(
        int? applicationId,
        ScenarioCatalogVisibilityFilter? visibility = null,
        CancellationToken cancellationToken = default);
}
