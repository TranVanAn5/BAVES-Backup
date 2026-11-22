using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Entities;
using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Repositories.Interfaces;

/// <summary>
/// Truy vấn và thao tác dữ liệu Bài tập (Assignments) trong schema [training].
/// </summary>
public interface IAssignmentRepository
{
    Task<Assignment?> GetEntityByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<AssignmentDetailDto?> GetDetailByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssignmentSummaryDto>> GetAssignmentsAsync(
        int? instructorId,
        int? cohortId,
        AssignmentStatus? status,
        string? search,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssignmentDetailDto>> GetLearnerVisibleAssignmentsAsync(
        int learnerId,
        CancellationToken cancellationToken = default);

    Task<bool> IsLearnerEligibleForAssignmentAsync(
        int assignmentId,
        int learnerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ScenarioVersionId mà learner được thấy qua bài giao (lớp đang thuộc hoặc giao cá nhân).
    /// </summary>
    Task<IReadOnlyList<int>> GetVisibleScenarioVersionIdsForLearnerAsync(
        int learnerId,
        CancellationToken cancellationToken = default);

    Task<bool> IsLearnerAssignedToScenarioVersionAsync(
        int learnerId,
        int scenarioVersionId,
        CancellationToken cancellationToken = default);

    void AddAssignment(Assignment assignment);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
