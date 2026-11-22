using BAVES.Api.Modules.Scenario.Models.DTOs;

namespace BAVES.Api.Modules.Scenario.Services.Interfaces;

/// <summary>
/// Cung cấp dữ liệu read-only của module Scenario cho các module khác.
/// Đây là "cửa" để module khác đọc dữ liệu scenario mà không cần biết
/// cấu trúc bảng [scenario].[Scenarios], [scenario].[ScenarioVersions],
/// [scenario].[StakeholderPersonas] hay entity EF của module này.
/// </summary>
public interface IScenarioCatalogService
{
    /// <summary>
    /// Lấy danh sách scenario. Nếu truyền applicationId thì lọc theo application.
    /// visibility = null giữ hành vi cũ (tất cả published) — caller Content nên luôn truyền filter theo role.
    /// </summary>
    Task<IReadOnlyList<ScenarioCatalogItemDto>> GetScenariosAsync(
        int? applicationId,
        ScenarioCatalogVisibilityFilter? visibility = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin tối thiểu của một ScenarioVersion (dùng khi mở Simulation Session).
    /// Trả null nếu version không tồn tại.
    /// </summary>
    Task<ScenarioVersionInfoDto?> GetVersionInfoAsync(
        int versionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách stakeholder persona đang active của một ScenarioVersion,
    /// sắp theo DisplayOrder. Trả danh sách rỗng nếu version không có persona active.
    /// </summary>
    Task<IReadOnlyList<ScenarioPersonaInfoDto>> GetActivePersonasAsync(
        int versionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy thông tin persona theo danh sách Id, dùng để bổ sung Name/StakeholderRole
    /// cho các SessionAgent đã tồn tại. Persona không tồn tại sẽ không có trong kết quả.
    /// </summary>
    Task<IReadOnlyDictionary<int, ScenarioPersonaInfoDto>> GetPersonasByIdsAsync(
        IReadOnlyCollection<int> personaIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy ngữ cảnh kinh doanh của một ScenarioVersion để dựng payload gửi AI Runtime
    /// (business context, business problem, assignment).
    /// Trả null nếu version không tồn tại.
    /// </summary>
    Task<StakeholderScenarioContextDto?> GetScenarioInterviewContextAsync(
        int versionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy hồ sơ persona đầy đủ (goals, pain points, communication style, personality...)
    /// để dựng payload gửi AI Runtime. Trả null nếu persona không tồn tại.
    /// </summary>
    Task<StakeholderPersonaDto?> GetPersonaInterviewContextAsync(
        int personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách knowledge item mà persona được phép biết, đã lọc theo
    /// [scenario].[StakeholderKnowledgeAccess]. Trả danh sách rỗng nếu persona
    /// không được gán knowledge nào.
    /// </summary>
    Task<IReadOnlyList<StakeholderKnowledgeItemDto>> GetAllowedKnowledgeAsync(
        int personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách event đang active của một ScenarioVersion (sắp theo DisplayOrder)
    /// để module Simulation hiển thị danh mục event của session.
    /// </summary>
    Task<IReadOnlyList<ScenarioEventInfoDto>> GetActiveEventsAsync(
        int versionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy một ScenarioEvent theo Id để validate khi learner trigger event.
    /// Trả null nếu không tồn tại.
    /// </summary>
    Task<ScenarioEventInfoDto?> GetEventByIdAsync(
        int scenarioEventId,
        CancellationToken cancellationToken = default);
}
