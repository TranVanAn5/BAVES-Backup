using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Enums;
using BAVES.Api.Modules.Training.Models.Requests;

namespace BAVES.Api.Modules.Training.Services.Interfaces;

/// <summary>
/// Dịch vụ xử lý nghiệp vụ Giao bài tập (Assignments), Học viên thực hiện bài tập và Sổ điểm (Gradebook).
/// </summary>
public interface IAssignmentService
{
    Task<AssignmentDetailDto> CreateAssignmentAsync(
        int userId,
        string userRole,
        CreateAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssignmentSummaryDto>> GetAssignmentsAsync(
        int userId,
        string userRole,
        int? cohortId,
        AssignmentStatus? status,
        string? search,
        CancellationToken cancellationToken = default);

    Task<AssignmentDetailDto?> GetAssignmentByIdAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default);

    Task<AssignmentDetailDto?> UpdateAssignmentAsync(
        int userId,
        string userRole,
        int assignmentId,
        UpdateAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<AssignmentDetailDto?> PublishAssignmentAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default);

    Task<AssignmentDetailDto?> CloseAssignmentAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default);

    // Nhóm 4: Learner Assignment Flow
    Task<IReadOnlyList<LearnerAssignmentDto>> GetMyAssignmentsAsync(
        int learnerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetVisibleScenarioVersionIdsForLearnerAsync(
        int learnerId,
        CancellationToken cancellationToken = default);

    Task<bool> IsLearnerAssignedToScenarioVersionAsync(
        int learnerId,
        int scenarioVersionId,
        CancellationToken cancellationToken = default);

    Task<SessionDto> StartAssignmentSessionAsync(
        int learnerId,
        int assignmentId,
        CancellationToken cancellationToken = default);

    // Nhóm 5: Gradebook & Export
    Task<AssignmentGradebookDto?> GetAssignmentGradebookAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default);

    Task<byte[]?> ExportAssignmentGradebookCsvAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default);
}
