using BAVES.Api.Modules.Scenario.Models.Entities;
using ScenarioEntity = BAVES.Api.Modules.Scenario.Models.Entities.Scenario;

namespace BAVES.Api.Modules.Scenario.Repositories.Interfaces;

/// <summary>
/// Repository chịu trách nhiệm thực thi toàn bộ các câu truy vấn cơ sở dữ liệu (EF Core)
/// cho nghiệp vụ Scenario Authoring (Backend B).
/// </summary>
public interface IScenarioAuthoringRepository
{
    /// <summary>
    /// Kiểm tra Business Application có tồn tại trong hệ thống hay không.
    /// </summary>
    Task<bool> ApplicationExistsAsync(
        int applicationId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Kiểm tra mã Scenario Code đã tồn tại hay chưa.
    /// </summary>
    Task<bool> ScenarioCodeExistsAsync(
        string code,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Thêm mới một Scenario (kèm Version khởi tạo) vào cơ sở dữ liệu và lưu thay đổi.
    /// </summary>
    Task AddScenarioAsync(
        ScenarioEntity scenario,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lấy thông tin Scenario kèm danh sách các ScenarioVersion.
    /// </summary>
    Task<ScenarioEntity?> GetScenarioWithVersionsAsync(
        int scenarioId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lấy phiên bản mới nhất (VersionNo lớn nhất) của một Scenario kèm toàn bộ Aggregate
    /// (StakeholderPersonas, StakeholderKnowledgeAccess, KnowledgeItems, BusinessRules, ScenarioEvents).
    /// </summary>
    Task<ScenarioVersion?> GetLatestVersionWithAggregateAsync(
        int scenarioId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Thêm mới một ScenarioVersion vào cơ sở dữ liệu và lưu thay đổi.
    /// </summary>
    Task AddScenarioVersionAsync(
        ScenarioVersion version,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lấy toàn bộ định nghĩa ScenarioVersion đã sắp xếp thứ tự hiển thị (Personas, KnowledgeItems,
    /// StakeholderKnowledgeAccess, BusinessRules, ScenarioEvents) phục vụ xem chi tiết cấu hình kịch bản.
    /// </summary>
    Task<ScenarioVersion?> GetVersionDefinitionAsync(
        int versionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lấy ScenarioVersion kèm toàn bộ Aggregate hiện tại để phục vụ cập nhật cấu hình kịch bản.
    /// </summary>
    Task<ScenarioVersion?> GetVersionWithAggregateForUpdateAsync(
        int versionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Xóa toàn bộ dữ liệu con của một ScenarioVersion (KnowledgeAccess, Personas, KnowledgeItems,
    /// BusinessRules, ScenarioEvents) trên DbContext trước khi ghi nhận tập dữ liệu mới.
    /// </summary>
    void ClearVersionAggregate(ScenarioVersion version);

    /// <summary>
    /// Xóa con của version bằng SQL theo ScenarioVersionId (an toàn khi discard draft).
    /// </summary>
    Task DeleteVersionChildrenAsync(int versionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy ScenarioVersion kèm Scenario, StakeholderPersonas, StakeholderKnowledgeAccess và KnowledgeItems
    /// để kiểm tra điều kiện hợp lệ và thực hiện Publish.
    /// </summary>
    Task<ScenarioVersion?> GetVersionForPublishAsync(
        int versionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Xóa một ScenarioVersion (sau khi đã ClearVersionAggregate) khỏi DbContext.
    /// </summary>
    void RemoveScenarioVersion(ScenarioVersion version);

    /// <summary>
    /// Xóa Scenario (và cascade versions còn lại nếu cấu hình DB cho phép).
    /// </summary>
    void RemoveScenario(ScenarioEntity scenario);

    /// <summary>
    /// Lưu các thay đổi đang được theo dõi trong ScenarioDbContext xuống cơ sở dữ liệu.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
