using BAVES.Api.Modules.Training.Data;
using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Entities;
using BAVES.Api.Modules.Training.Models.Enums;
using BAVES.Api.Modules.Training.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Training.Repositories;

public sealed class AssignmentRepository(TrainingDbContext db) : IAssignmentRepository
{
    public async Task<Assignment?> GetEntityByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.Assignments
            .Include(a => a.Cohort)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<AssignmentDetailDto?> GetDetailByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.Assignments
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AssignmentDetailDto(
                a.Id,
                a.Title,
                a.Instructions,
                a.ScenarioVersionId,
                a.CohortId,
                a.Cohort != null ? a.Cohort.Code : null,
                a.Cohort != null ? a.Cohort.Name : null,
                a.LearnerId,
                a.RubricId,
                a.AssignedByUserId,
                a.StartAt,
                a.DueAt,
                a.MaxAttempts,
                a.Status,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssignmentSummaryDto>> GetAssignmentsAsync(
        int? instructorId,
        int? cohortId,
        AssignmentStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var query = db.Assignments.AsNoTracking();

        if (instructorId.HasValue)
        {
            query = query.Where(a =>
                a.AssignedByUserId == instructorId.Value ||
                (a.Cohort != null && a.Cohort.InstructorId == instructorId.Value));
        }

        if (cohortId.HasValue)
        {
            query = query.Where(a => a.CohortId == cohortId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a => a.Title.Contains(term));
        }

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssignmentSummaryDto(
                a.Id,
                a.Title,
                a.ScenarioVersionId,
                a.CohortId,
                a.Cohort != null ? a.Cohort.Code : null,
                a.Cohort != null ? a.Cohort.Name : null,
                a.LearnerId,
                a.RubricId,
                a.AssignedByUserId,
                a.StartAt,
                a.DueAt,
                a.MaxAttempts,
                a.Status,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AssignmentDetailDto>> GetLearnerVisibleAssignmentsAsync(
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        var activeCohortIds = await db.CohortMembers
            .AsNoTracking()
            .Where(m => m.LearnerId == learnerId && m.Status != CohortMemberStatus.Removed)
            .Select(m => m.CohortId)
            .ToListAsync(cancellationToken);

        return await db.Assignments
            .AsNoTracking()
            .Where(a =>
                a.Status != AssignmentStatus.Draft &&
                a.Status != AssignmentStatus.Archived &&
                (a.LearnerId == learnerId ||
                 (a.CohortId.HasValue && activeCohortIds.Contains(a.CohortId.Value))))
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AssignmentDetailDto(
                a.Id,
                a.Title,
                a.Instructions,
                a.ScenarioVersionId,
                a.CohortId,
                a.Cohort != null ? a.Cohort.Code : null,
                a.Cohort != null ? a.Cohort.Name : null,
                a.LearnerId,
                a.RubricId,
                a.AssignedByUserId,
                a.StartAt,
                a.DueAt,
                a.MaxAttempts,
                a.Status,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsLearnerEligibleForAssignmentAsync(
        int assignmentId,
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        return await db.Assignments
            .AsNoTracking()
            .AnyAsync(a =>
                a.Id == assignmentId &&
                (a.LearnerId == learnerId ||
                 (a.CohortId.HasValue &&
                  a.Cohort!.CohortMembers.Any(m =>
                      m.LearnerId == learnerId && m.Status != CohortMemberStatus.Removed))),
                cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetVisibleScenarioVersionIdsForLearnerAsync(
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        var activeCohortIds = await db.CohortMembers
            .AsNoTracking()
            .Where(m => m.LearnerId == learnerId && m.Status != CohortMemberStatus.Removed)
            .Select(m => m.CohortId)
            .ToListAsync(cancellationToken);

        return await db.Assignments
            .AsNoTracking()
            .Where(a =>
                a.Status != AssignmentStatus.Draft &&
                a.Status != AssignmentStatus.Archived &&
                (a.LearnerId == learnerId ||
                 (a.CohortId.HasValue && activeCohortIds.Contains(a.CohortId.Value))))
            .Select(a => a.ScenarioVersionId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsLearnerAssignedToScenarioVersionAsync(
        int learnerId,
        int scenarioVersionId,
        CancellationToken cancellationToken = default)
    {
        var activeCohortIds = await db.CohortMembers
            .AsNoTracking()
            .Where(m => m.LearnerId == learnerId && m.Status != CohortMemberStatus.Removed)
            .Select(m => m.CohortId)
            .ToListAsync(cancellationToken);

        return await db.Assignments
            .AsNoTracking()
            .AnyAsync(a =>
                a.ScenarioVersionId == scenarioVersionId &&
                a.Status != AssignmentStatus.Draft &&
                a.Status != AssignmentStatus.Archived &&
                (a.LearnerId == learnerId ||
                 (a.CohortId.HasValue && activeCohortIds.Contains(a.CohortId.Value))),
                cancellationToken);
    }

    public void AddAssignment(Assignment assignment) => db.Assignments.Add(assignment);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        db.SaveChangesAsync(cancellationToken);
}
