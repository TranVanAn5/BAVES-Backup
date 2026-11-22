using BAVES.Api.Modules.Planning.Data;
using BAVES.Api.Modules.Planning.Models.DTOs;
using BAVES.Api.Modules.Planning.Models.Entities;
using BAVES.Api.Modules.Planning.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Planning.Repositories;

/// <summary>
/// Hiện thực truy vấn Planning trên PlanningDbContext.
/// </summary>
public sealed class PlanningRepository(PlanningDbContext db) : IPlanningRepository
{
    public async Task<BAPlanDto?> GetPlanBySessionAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // Projection thẳng ra DTO trong SQL: chỉ lấy đúng cột cần,
        // không tải entity thừa. Include dùng để lấy kèm PlannedQuestions.
        return await db.BAPlans
            .AsNoTracking()
            .Include(p => p.PlannedQuestions)
            .Where(p => p.SessionId == sessionId)
            .Select(p => new BAPlanDto(
                p.Id,
                p.SessionId,
                p.Objective,
                p.Approach,
                p.Status,
                p.CreatedAt,
                p.UpdatedAt,
                p.PlannedQuestions
                    .OrderBy(q => q.DisplayOrder)
                    .ThenBy(q => q.Id)
                    .Select(q => new PlannedQuestionDto(
                        q.Id,
                        q.StakeholderPersonaId,
                        q.Topic,
                        q.InformationNeed,
                        q.Question,
                        q.Priority,
                        q.DisplayOrder,
                        q.Status,
                        q.CreatedAt))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BAPlan?> GetPlanEntityForUpdateAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // Không AsNoTracking: theo dõi thay đổi để SaveChanges sinh UPDATE.
        return await db.BAPlans
            .Include(p => p.PlannedQuestions)
            .FirstOrDefaultAsync(p => p.SessionId == sessionId, cancellationToken);
    }

    public void AddPlan(BAPlan plan)
    {
        db.BAPlans.Add(plan);
    }

    public void RemoveQuestions(IEnumerable<PlannedQuestion> questions)
    {
        db.PlannedQuestions.RemoveRange(questions);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}