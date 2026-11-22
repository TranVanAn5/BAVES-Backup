using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Entities;
using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Repositories.Interfaces;

/// <summary>
/// Truy vấn và thao tác dữ liệu Lớp học (Cohorts, CohortMembers) trong schema [training].
/// </summary>
public interface ICohortRepository
{
    Task<Cohort?> GetEntityByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Cohort?> GetEntityByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<Cohort?> GetEntityWithCountsByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByCodeAsync(
        string code,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<CohortDetailDto?> GetDetailByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CohortSummaryDto>> GetCohortsAsync(
        int? instructorId,
        CohortStatus? status,
        string? search,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LearnerCohortDto>> GetLearnerCohortsAsync(
        int learnerId,
        CancellationToken cancellationToken = default);

    Task<bool> IsLearnerInCohortAsync(
        int cohortId,
        int learnerId,
        CancellationToken cancellationToken = default);

    // Cohort Members
    Task<IReadOnlyList<CohortMember>> GetCohortMembersAsync(
        int cohortId,
        CohortMemberStatus? status,
        CancellationToken cancellationToken = default);

    Task<CohortMember?> GetCohortMemberEntityAsync(
        int cohortId,
        int learnerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CohortMember>> GetCohortMemberEntitiesByLearnerIdsAsync(
        int cohortId,
        IReadOnlyCollection<int> learnerIds,
        CancellationToken cancellationToken = default);

    void AddCohort(Cohort cohort);

    void RemoveCohort(Cohort cohort);

    void AddCohortMember(CohortMember member);

    void AddCohortMembers(IEnumerable<CohortMember> members);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
