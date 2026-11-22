using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Enums;
using BAVES.Api.Modules.Training.Models.Requests;

namespace BAVES.Api.Modules.Training.Services.Interfaces;

/// <summary>
/// Dịch vụ xử lý nghiệp vụ Quản lý Lớp học (Cohorts), Học viên trong lớp (CohortMembers) và Báo cáo phân tích lớp học.
/// </summary>
public interface ICohortService
{
    Task<CohortDetailDto> CreateCohortAsync(
        int instructorId,
        CreateCohortRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CohortSummaryDto>> GetCohortsAsync(
        int userId,
        string userRole,
        CohortStatus? status,
        string? search,
        CancellationToken cancellationToken = default);

    Task<CohortDetailDto?> GetCohortByIdAsync(
        int userId,
        string userRole,
        int cohortId,
        CancellationToken cancellationToken = default);

    Task<CohortDetailDto?> UpdateCohortAsync(
        int userId,
        string userRole,
        int cohortId,
        UpdateCohortRequest request,
        CancellationToken cancellationToken = default);

    Task<CohortDetailDto?> UpdateCohortStatusAsync(
        int userId,
        string userRole,
        int cohortId,
        CohortStatus status,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteCohortAsync(
        int userId,
        string userRole,
        int cohortId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LearnerCohortDto>> GetMyCohortsAsync(
        int learnerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AvailableCohortDto>> GetAvailableCohortsAsync(
        int learnerId,
        string? search,
        CancellationToken cancellationToken = default);

    Task<LearnerCohortDto> RequestJoinCohortAsync(
        int learnerId,
        int? cohortId,
        string? code,
        CancellationToken cancellationToken = default);

    // Cohort Members (Nhóm 2)
    Task<IReadOnlyList<CohortMemberDto>?> GetCohortMembersAsync(
        int userId,
        string userRole,
        int cohortId,
        CohortMemberStatus? status,
        string? search,
        CancellationToken cancellationToken = default);

    Task<CohortMemberDto?> AddCohortMemberAsync(
        int userId,
        string userRole,
        int cohortId,
        AddCohortMemberRequest request,
        CancellationToken cancellationToken = default);

    Task<CohortMemberDto?> ApproveCohortMemberAsync(
        int userId,
        string userRole,
        int cohortId,
        int learnerId,
        CancellationToken cancellationToken = default);

    Task<BatchAddCohortMembersResultDto?> BatchAddCohortMembersAsync(
        int userId,
        string userRole,
        int cohortId,
        BatchAddCohortMembersRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveCohortMemberAsync(
        int userId,
        string userRole,
        int cohortId,
        int learnerId,
        CancellationToken cancellationToken = default);

    // Cohort Analytics (Nhóm 5)
    Task<CohortAnalyticsDto?> GetCohortAnalyticsAsync(
        int userId,
        string userRole,
        int cohortId,
        CancellationToken cancellationToken = default);
}
