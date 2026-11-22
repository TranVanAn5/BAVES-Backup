using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Requests;

namespace BAVES.Api.Modules.Scenario.Services.Interfaces;

public interface IScenarioAuthoringService
{
    Task<ScenarioSummaryDto> CreateScenarioAsync(
        int userId,
        CreateScenarioRequest request,
        CancellationToken cancellationToken = default
    );

    Task<ScenarioVersionSummaryDto> CreateVersionAsync(
        int userId,
        int scenarioId,
        CreateScenarioVersionRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Hủy (xóa) bản nháp / bị từ chối. Không xóa bản đã publish hoặc đang chờ duyệt.
    /// </summary>
    Task<DiscardDraftResultDto> DiscardDraftVersionAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        CancellationToken cancellationToken = default
    );

    Task<ScenarioDefinitionDto> GetDefinitionAsync(
        int userId,
        int versionId,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    );

    Task<ScenarioReviewDecisionDto> SubmitForReviewAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        CancellationToken cancellationToken = default
    );

    Task<ScenarioReviewDecisionDto> ApproveVersionAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        CancellationToken cancellationToken = default
    );

    Task<ScenarioReviewDecisionDto> RejectVersionAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        string comment,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ScenarioReviewQueueItemDto>> ListPendingReviewsAsync(
        IReadOnlyCollection<string> userRoles,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Kịch bản mà Expert hiện tại đã duyệt hoặc từ chối (ReviewedByUserId = userId).
    /// </summary>
    Task<IReadOnlyList<ScenarioReviewQueueItemDto>> ListReviewedByMeAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<ScenarioReviewQueueItemDto>> ListMyVersionsAsync(
        int userId,
        CancellationToken cancellationToken = default
    );

    Task<ScenarioDefinitionDto> UpdateDefinitionAsync(
        int userId,
        int versionId,
        UpdateScenarioDefinitionRequest request,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    );

    /// <summary>
    /// Sprint CRUD trên bảng scenario.ScenarioSprints.
    /// </summary>
    Task<ScenarioSprintListDto> GetSprintsAsync(
        int userId,
        int versionId,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    );

    Task<ScenarioSprintListDto> ReplaceSprintsAsync(
        int userId,
        int versionId,
        ReplaceScenarioSprintsRequest request,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    );

    Task<ScenarioSprintListDto> AddSprintAsync(
        int userId,
        int versionId,
        UpsertScenarioSprintRequest request,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    );

    Task<ScenarioSprintListDto> UpdateSprintAsync(
        int userId,
        int versionId,
        int sprintNo,
        UpsertScenarioSprintRequest request,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    );

    Task<ScenarioSprintListDto> DeleteSprintAsync(
        int userId,
        int versionId,
        int sprintNo,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    );

    Task<ScenarioVersionSummaryDto> PublishVersionAsync(
        int userId,
        int versionId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lấy context "core" của một Scenario Version (BusinessContext, BusinessProblem,
    /// BusinessObjectives, Summary). Dùng cho Learner trước khi bắt đầu simulation.
    /// </summary>
    /// <param name="userId">ID của user đang gọi (lấy từ JWT).</param>
    /// <param name="userRoles">Danh sách role của user (phục vụ check quyền xem bản nháp).</param>
    /// <param name="versionId">ID của ScenarioVersion.</param>
    /// <param name="cancellationToken">Token hủy cho truy vấn bất đồng bộ.</param>
    Task<ScenarioContextDto> GetContextAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        CancellationToken cancellationToken = default
    );
}
