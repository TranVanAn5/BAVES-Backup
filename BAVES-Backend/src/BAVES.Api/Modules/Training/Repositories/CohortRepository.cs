using BAVES.Api.Modules.Training.Data;
using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Entities;
using BAVES.Api.Modules.Training.Models.Enums;
using BAVES.Api.Modules.Training.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Training.Repositories;

public sealed class CohortRepository(TrainingDbContext db) : ICohortRepository
{
    public async Task<Cohort?> GetEntityByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.Cohorts
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Cohort?> GetEntityByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await db.Cohorts
            .FirstOrDefaultAsync(c => c.Code == normalized, cancellationToken);
    }

    public async Task<Cohort?> GetEntityWithCountsByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.Cohorts
            .Include(c => c.CohortMembers)
            .Include(c => c.Assignments)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(
        string code,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Cohorts.AsNoTracking().Where(c => c.Code == code);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<CohortDetailDto?> GetDetailByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.Cohorts
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CohortDetailDto(
                c.Id,
                c.InstructorId,
                c.Code,
                c.Name,
                c.Description,
                c.StartDate,
                c.EndDate,
                c.Status,
                c.CohortMembers.Count(m => m.Status == CohortMemberStatus.Active),
                c.Assignments.Count(a => a.Status == AssignmentStatus.Active),
                c.Assignments.Count,
                c.CreatedAt,
                c.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CohortSummaryDto>> GetCohortsAsync(
        int? instructorId,
        CohortStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = db.Cohorts.AsNoTracking();

        if (instructorId.HasValue)
        {
            query = query.Where(c => c.InstructorId == instructorId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Code.Contains(term) || c.Name.Contains(term));
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CohortSummaryDto(
                c.Id,
                c.InstructorId,
                c.Code,
                c.Name,
                c.Description,
                c.StartDate,
                c.EndDate,
                c.Status,
                c.CohortMembers.Count(m => m.Status == CohortMemberStatus.Active),
                c.Assignments.Count,
                c.CreatedAt,
                c.UpdatedAt,
                c.CohortMembers.Count(m => m.Status == CohortMemberStatus.Pending)
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LearnerCohortDto>> GetLearnerCohortsAsync(
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        return await db.CohortMembers
            .AsNoTracking()
            .Where(m => m.LearnerId == learnerId && m.Status != CohortMemberStatus.Removed)
            .OrderByDescending(m => m.JoinedAt)
            .Select(m => new LearnerCohortDto(
                m.CohortId,
                m.Cohort.InstructorId,
                m.Cohort.Code,
                m.Cohort.Name,
                m.Cohort.Description,
                m.Cohort.StartDate,
                m.Cohort.EndDate,
                m.Cohort.Status,
                m.Status,
                m.JoinedAt,
                m.Cohort.Assignments.Count(a => a.Status == AssignmentStatus.Active),
                null,
                null
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsLearnerInCohortAsync(
        int cohortId,
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        return await db.CohortMembers
            .AsNoTracking()
            .AnyAsync(m => m.CohortId == cohortId
                        && m.LearnerId == learnerId
                        && m.Status != CohortMemberStatus.Removed,
                cancellationToken);
    }

    public async Task<IReadOnlyList<CohortMember>> GetCohortMembersAsync(
        int cohortId,
        CohortMemberStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.CohortMembers
            .AsNoTracking()
            .Where(m => m.CohortId == cohortId);

        if (status.HasValue)
        {
            query = query.Where(m => m.Status == status.Value);
        }
        else
        {
            query = query.Where(m => m.Status != CohortMemberStatus.Removed);
        }

        return await query
            .OrderByDescending(m => m.JoinedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<CohortMember?> GetCohortMemberEntityAsync(
        int cohortId,
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        return await db.CohortMembers
            .FirstOrDefaultAsync(m => m.CohortId == cohortId && m.LearnerId == learnerId, cancellationToken);
    }

    public async Task<IReadOnlyList<CohortMember>> GetCohortMemberEntitiesByLearnerIdsAsync(
        int cohortId,
        IReadOnlyCollection<int> learnerIds,
        CancellationToken cancellationToken = default)
    {
        if (learnerIds.Count == 0)
        {
            return Array.Empty<CohortMember>();
        }

        return await db.CohortMembers
            .Where(m => m.CohortId == cohortId && learnerIds.Contains(m.LearnerId))
            .ToListAsync(cancellationToken);
    }

    public void AddCohort(Cohort cohort) => db.Cohorts.Add(cohort);

    public void RemoveCohort(Cohort cohort) => db.Cohorts.Remove(cohort);

    public void AddCohortMember(CohortMember member) => db.CohortMembers.Add(member);

    public void AddCohortMembers(IEnumerable<CohortMember> members) => db.CohortMembers.AddRange(members);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
