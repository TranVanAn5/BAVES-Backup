using BAVES.Api.Modules.Scenario.Models.DTOs;

namespace BAVES.Api.Modules.Scenario.Repositories.Interfaces;

/// <summary>
/// Truy vấn read-only phục vụ việc khởi tạo Simulation Session:
/// lấy thông tin ScenarioVersion và danh sách StakeholderPersona.
/// Module Scenario là chủ sở hữu duy nhất của các bảng
/// [scenario].[ScenarioVersions] và [scenario].[StakeholderPersonas].
/// </summary>
public interface IScenarioSessionContextRepository
{
    /// <summary>
    /// Lấy thông tin tối thiểu của một ScenarioVersion.
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
    /// Lấy thông tin persona theo danh sách Id (dùng để bổ sung Name/StakeholderRole
    /// cho các SessionAgent đã tồn tại). Persona không tồn tại sẽ không có trong kết quả.
    /// </summary>
    Task<IReadOnlyDictionary<int, ScenarioPersonaInfoDto>> GetPersonasByIdsAsync(
        IReadOnlyCollection<int> personaIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy ngữ cảnh kinh doanh của một ScenarioVersion (business context, business problem,
    /// assignment) để dựng payload gửi AI Runtime.
    /// Trả null nếu version không tồn tại.
    /// </summary>
    Task<StakeholderScenarioContextDto?> GetScenarioInterviewContextAsync(
        int versionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy hồ sơ persona đầy đủ (goals, pain points, communication style, personality...)
    /// để dựng payload gửi AI Runtime.
    /// Trả null nếu persona không tồn tại.
    /// </summary>
    Task<StakeholderPersonaDto?> GetPersonaInterviewContextAsync(
        int personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách knowledge item mà persona được phép biết,
    /// đã lọc theo [scenario].[StakeholderKnowledgeAccess].
    /// Kèm cờ isHidden / disclosureRule để AI biết cách phản ứng đúng.
    /// Trả danh sách rỗng nếu persona không được gán knowledge nào.
    /// </summary>
    Task<IReadOnlyList<StakeholderKnowledgeItemDto>> GetAllowedKnowledgeAsync(
        int personaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách event đang active của một ScenarioVersion, sắp theo DisplayOrder.
    /// Dùng bởi module Simulation để hiển thị danh mục event và validate khi trigger.
    /// Trả danh sách rỗng nếu version không có event nào.
    /// </summary>
    Task<IReadOnlyList<ScenarioEventInfoDto>> GetActiveEventsAsync(
        int versionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy một ScenarioEvent theo Id.
    /// Trả null nếu event không tồn tại.
    /// </summary>
    Task<ScenarioEventInfoDto?> GetEventByIdAsync(
        int scenarioEventId,
        CancellationToken cancellationToken = default);
}
