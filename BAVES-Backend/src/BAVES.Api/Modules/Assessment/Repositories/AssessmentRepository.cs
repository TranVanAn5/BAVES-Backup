using System.Text.Json;
using BAVES.Api.Modules.Assessment.Data;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Models.Entities;
using BAVES.Api.Modules.Assessment.Models.Enums;
using BAVES.Api.Modules.Assessment.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Assessment.Repositories;

public sealed class AssessmentRepository(AssessmentDbContext db) : IAssessmentRepository
{
    // ─────────── RUBRIC ───────────
    public async Task<Rubric?> GetRubricByScenarioVersionAsync(
        int scenarioVersionId,
        CancellationToken cancellationToken = default)
    {
        return await db.Rubrics
            .AsNoTracking()
            .Include(r => r.RubricCriteria)
            .FirstOrDefaultAsync(r => r.ScenarioVersionId == scenarioVersionId, cancellationToken);
    }

    public async Task<Rubric?> GetDefaultRubricAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Rubrics
            .AsNoTracking()
            .Include(r => r.RubricCriteria)
            .FirstOrDefaultAsync(r => r.Status == RubricStatus.Published, cancellationToken);
    }

    public async Task<IReadOnlyList<BabokTaskDto>> GetBabokTasksAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.BabokTasks
            .AsNoTracking()
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Id)
            .Select(t => new BabokTaskDto(
                t.Id,
                t.KnowledgeAreaCode,
                t.KnowledgeAreaName,
                t.Code,
                t.Name,
                t.Purpose,
                t.Description,
                t.BabokVersion,
                t.DisplayOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> AllBabokTasksExistAsync(
        IReadOnlyCollection<int> babokTaskIds,
        CancellationToken cancellationToken = default)
    {
        if (babokTaskIds.Count == 0)
        {
            return false;
        }

        var distinctIds = babokTaskIds.Distinct().ToList();
        var count = await db.BabokTasks
            .AsNoTracking()
            .CountAsync(t => distinctIds.Contains(t.Id), cancellationToken);

        return count == distinctIds.Count;
    }

    public async Task<IReadOnlyList<RubricDetailDto>> GetRubricsAsync(
        int? scenarioVersionId,
        RubricStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.Rubrics
            .AsNoTracking()
            .Include(r => r.RubricCriteria)
                .ThenInclude(c => c.BabokTask)
            .AsQueryable();

        if (scenarioVersionId.HasValue)
        {
            query = query.Where(r => r.ScenarioVersionId == scenarioVersionId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var entities = await query
            .OrderByDescending(r => r.ScenarioVersionId)
            .ThenByDescending(r => r.Version)
            .ToListAsync(cancellationToken);

        return entities.Select(MapRubricDetailToDto).ToList();
    }

    public async Task<RubricDetailDto?> GetRubricDetailByIdAsync(
        int rubricId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.Rubrics
            .AsNoTracking()
            .Include(r => r.RubricCriteria)
                .ThenInclude(c => c.BabokTask)
            .FirstOrDefaultAsync(r => r.Id == rubricId, cancellationToken);

        return entity is null ? null : MapRubricDetailToDto(entity);
    }

    public async Task<Rubric?> GetTrackedRubricByIdAsync(
        int rubricId,
        CancellationToken cancellationToken = default)
    {
        return await db.Rubrics
            .Include(r => r.RubricCriteria)
            .FirstOrDefaultAsync(r => r.Id == rubricId, cancellationToken);
    }

    public async Task<int> GetNextRubricVersionAsync(
        int scenarioVersionId,
        CancellationToken cancellationToken = default)
    {
        var maxVersion = await db.Rubrics
            .AsNoTracking()
            .Where(r => r.ScenarioVersionId == scenarioVersionId)
            .MaxAsync(r => (int?)r.Version, cancellationToken);

        return (maxVersion ?? 0) + 1;
    }

    public async Task<RubricDetailDto> CreateRubricAsync(
        Rubric rubric,
        CancellationToken cancellationToken = default)
    {
        db.Rubrics.Add(rubric);
        await db.SaveChangesAsync(cancellationToken);

        return (await GetRubricDetailByIdAsync(rubric.Id, cancellationToken))
               ?? MapRubricDetailToDto(rubric);
    }

    public async Task<RubricDetailDto> UpdateRubricAsync(
        Rubric rubric,
        List<RubricCriterion> newCriteria,
        CancellationToken cancellationToken = default)
    {
        if (rubric.RubricCriteria.Count > 0)
        {
            db.RubricCriteria.RemoveRange(rubric.RubricCriteria);
        }

        rubric.RubricCriteria.Clear();
        foreach (var criterion in newCriteria)
        {
            criterion.RubricId = rubric.Id;
            rubric.RubricCriteria.Add(criterion);
        }

        await db.SaveChangesAsync(cancellationToken);

        return (await GetRubricDetailByIdAsync(rubric.Id, cancellationToken))
               ?? MapRubricDetailToDto(rubric);
    }

    public async Task<RubricDetailDto?> PublishRubricAsync(
        int rubricId,
        CancellationToken cancellationToken = default)
    {
        var rubric = await db.Rubrics
            .FirstOrDefaultAsync(r => r.Id == rubricId, cancellationToken);

        if (rubric is null)
        {
            return null;
        }

        rubric.Status = RubricStatus.Published;
        rubric.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return await GetRubricDetailByIdAsync(rubricId, cancellationToken);
    }

    private static RubricDetailDto MapRubricDetailToDto(Rubric r) =>
        new(
            r.Id,
            r.ScenarioVersionId,
            r.CreatedByUserId,
            r.Name,
            r.Version,
            r.Description,
            r.Status,
            r.CreatedAt,
            r.UpdatedAt,
            r.RubricCriteria
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .Select(c => new RubricCriterionDetailDto(
                    c.Id,
                    c.RubricId,
                    c.BabokTaskId,
                    c.BabokTask?.Code,
                    c.BabokTask?.Name,
                    c.Code,
                    c.Name,
                    c.Description,
                    c.Weight,
                    c.MaxScore,
                    c.EvidenceGuidance,
                    c.DisplayOrder,
                    c.IsRequired))
                .ToList()
        );

    public async Task<Rubric?> GetRubricByIdAsync(
        int rubricId,
        CancellationToken cancellationToken = default)
    {
        return await db.Rubrics
            .AsNoTracking()
            .Include(r => r.RubricCriteria)
            .FirstOrDefaultAsync(r => r.Id == rubricId, cancellationToken);
    }

    // ─────────── AI FEEDBACK ───────────
    public async Task<AIPracticeFeedbackDto?> GetFeedbackDtoBySubmissionIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.AIPracticeFeedbacks
            .AsNoTracking()
            .Include(f => f.CriterionScores)
                .ThenInclude(cs => cs.RubricCriterion)
                    .ThenInclude(c => c!.BabokTask)
            .OrderByDescending(f => f.StartedAt)
            .FirstOrDefaultAsync(f => f.SubmissionId == submissionId, cancellationToken);

        return entity is null ? null : MapToDto(entity);
    }

    public async Task<AIPracticeFeedback?> GetFeedbackEntityBySubmissionIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        return await db.AIPracticeFeedbacks
            .Include(f => f.CriterionScores)
            .OrderByDescending(f => f.StartedAt)
            .FirstOrDefaultAsync(f => f.SubmissionId == submissionId, cancellationToken);
    }

    public async Task<AIPracticeFeedbackDto> SavePracticeFeedbackAsync(
        AIPracticeFeedback feedback,
        List<AIFeedbackCriterionScore> criterionScores,
        CancellationToken cancellationToken = default)
    {
        var existing = await db.AIPracticeFeedbacks
            .Include(f => f.CriterionScores)
            .Where(f => f.SubmissionId == feedback.SubmissionId)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
        {
            db.AIPracticeFeedbacks.RemoveRange(existing);
            await db.SaveChangesAsync(cancellationToken);
        }

        db.AIPracticeFeedbacks.Add(feedback);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var cs in criterionScores)
        {
            cs.AIFeedbackId = feedback.Id;
            db.AIFeedbackCriterionScores.Add(cs);
        }

        await db.SaveChangesAsync(cancellationToken);

        return await GetFeedbackDtoBySubmissionIdAsync(feedback.SubmissionId, cancellationToken)
               ?? MapToDto(feedback);
    }

    // ─────────── REVIEW REQUESTS ───────────
    public async Task<ReviewRequest?> GetReviewRequestByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.ReviewRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<ReviewRequest?> GetOpenReviewRequestBySubmissionIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        return await db.ReviewRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.SubmissionId == submissionId &&
                                      (r.Status == ReviewRequestStatus.Pending || r.Status == ReviewRequestStatus.Accepted),
                cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewRequestDto>> GetReviewRequestsAsync(
        int? learnerId,
        int? reviewerId,
        bool queueOnly,
        CancellationToken cancellationToken = default)
    {
        var query = db.ReviewRequests.AsNoTracking().AsQueryable();

        if (learnerId.HasValue)
        {
            query = query.Where(r => r.LearnerId == learnerId.Value);
        }

        if (queueOnly)
        {
            query = query.Where(r => r.Status == ReviewRequestStatus.Pending && r.AssignedReviewerId == null);
        }
        else if (reviewerId.HasValue)
        {
            query = query.Where(r => r.AssignedReviewerId == reviewerId.Value);
        }

        var list = await query
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new ReviewRequestDto(
                r.Id,
                r.SubmissionId,
                r.LearnerId,
                r.AssignedReviewerId,
                r.RequestNote,
                r.Status.ToString(),
                r.RequestedAt,
                r.AcceptedAt,
                r.CompletedAt))
            .ToListAsync(cancellationToken);

        return list;
    }

    public async Task<ReviewRequestDto> CreateReviewRequestAsync(
        ReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        db.ReviewRequests.Add(request);
        await db.SaveChangesAsync(cancellationToken);

        return new ReviewRequestDto(
            request.Id,
            request.SubmissionId,
            request.LearnerId,
            request.AssignedReviewerId,
            request.RequestNote,
            request.Status.ToString(),
            request.RequestedAt,
            request.AcceptedAt,
            request.CompletedAt);
    }

    public async Task<bool> TryAcceptReviewRequestAsync(
        int requestId,
        int reviewerId,
        CancellationToken cancellationToken = default)
    {
        // Khóa đồng thời tối ưu (Atomic Conditional Update)
        var affected = await db.ReviewRequests
            .Where(r => r.Id == requestId && r.Status == ReviewRequestStatus.Pending && r.AssignedReviewerId == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, ReviewRequestStatus.Accepted)
                .SetProperty(r => r.AssignedReviewerId, reviewerId)
                .SetProperty(r => r.AcceptedAt, DateTimeOffset.UtcNow),
                cancellationToken);

        return affected > 0;
    }

    public async Task<bool> CompleteReviewRequestAsync(
        int requestId,
        CancellationToken cancellationToken = default)
    {
        var affected = await db.ReviewRequests
            .Where(r => r.Id == requestId && r.Status == ReviewRequestStatus.Accepted)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, ReviewRequestStatus.Completed)
                .SetProperty(r => r.CompletedAt, DateTimeOffset.UtcNow),
                cancellationToken);

        return affected > 0;
    }

    public async Task<bool> RequestRevisionAsync(
        int requestId,
        int reviewerId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var reviewReq = await db.ReviewRequests
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (reviewReq is null)
        {
            return false;
        }

        if (reviewReq.AssignedReviewerId != reviewerId)
        {
            return false;
        }

        if (reviewReq.Status is not (ReviewRequestStatus.Pending or ReviewRequestStatus.Accepted))
        {
            return false;
        }

        var notePrefix = string.IsNullOrWhiteSpace(reviewReq.RequestNote)
            ? string.Empty
            : $"{reviewReq.RequestNote.Trim()}\n\n";
        var revisionNote = $"{notePrefix}[Yêu cầu làm lại] {reason.Trim()}";
        if (revisionNote.Length > 1000)
        {
            revisionNote = revisionNote[^1000..];
        }

        reviewReq.Status = ReviewRequestStatus.RevisionRequested;
        reviewReq.RequestNote = revisionNote;
        reviewReq.CompletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ─────────── HUMAN REVIEWS ───────────
    public async Task<IReadOnlyList<AIPracticeFeedbackDto>> GetFeedbackDtosBySubmissionIdsAsync(
        IReadOnlyCollection<int> submissionIds,
        CancellationToken cancellationToken = default)
    {
        if (submissionIds.Count == 0)
        {
            return Array.Empty<AIPracticeFeedbackDto>();
        }

        var entities = await db.AIPracticeFeedbacks
            .AsNoTracking()
            .Include(f => f.CriterionScores)
                .ThenInclude(cs => cs.RubricCriterion)
                    .ThenInclude(c => c!.BabokTask)
            .Where(f => submissionIds.Contains(f.SubmissionId))
            .OrderByDescending(f => f.CompletedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<HumanReviewDto>> GetHumanReviewDtosBySubmissionIdsAsync(
        IReadOnlyCollection<int> submissionIds,
        CancellationToken cancellationToken = default)
    {
        if (submissionIds.Count == 0)
        {
            return Array.Empty<HumanReviewDto>();
        }

        var entities = await db.HumanReviews
            .AsNoTracking()
            .Include(hr => hr.CriterionScores)
                .ThenInclude(cs => cs.RubricCriterion)
                    .ThenInclude(c => c!.BabokTask)
            .Where(hr => submissionIds.Contains(hr.SubmissionId))
            .OrderByDescending(hr => hr.ReviewedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapHumanReviewToDto).ToList();
    }

    public async Task<HumanReviewDto?> GetHumanReviewDtoBySubmissionIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.HumanReviews
            .AsNoTracking()
            .Include(hr => hr.CriterionScores)
                .ThenInclude(cs => cs.RubricCriterion)
                    .ThenInclude(c => c!.BabokTask)
            .OrderByDescending(hr => hr.ReviewedAt)
            .FirstOrDefaultAsync(hr => hr.SubmissionId == submissionId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return MapHumanReviewToDto(entity);
    }

    public async Task<HumanReviewDto> SaveHumanReviewAsync(
        HumanReview review,
        List<HumanReviewCriterionScore> criterionScores,
        CancellationToken cancellationToken = default)
    {
        db.HumanReviews.Add(review);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var cs in criterionScores)
        {
            cs.HumanReviewId = review.Id;
            db.HumanReviewCriterionScores.Add(cs);
        }

        await db.SaveChangesAsync(cancellationToken);

        return await GetHumanReviewDtoBySubmissionIdAsync(review.SubmissionId, cancellationToken)
               ?? MapHumanReviewToDto(review);
    }

    // ─────────── MAPPERS ───────────
    private static HumanReviewDto MapHumanReviewToDto(HumanReview hr) =>
        new(
            hr.Id,
            hr.SubmissionId,
            hr.RubricId,
            hr.ReviewerId,
            hr.FinalScore,
            hr.OverallFeedback,
            hr.CriterionScores
                .Select(cs => new HumanReviewCriterionScoreDto(
                    cs.RubricCriterionId,
                    cs.RubricCriterion?.Code ?? string.Empty,
                    cs.RubricCriterion?.Name ?? string.Empty,
                    cs.Score,
                    cs.RubricCriterion?.MaxScore ?? 100m,
                    cs.Comment,
                    cs.RubricCriterion?.BabokTaskId,
                    cs.RubricCriterion?.BabokTask?.Code,
                    cs.RubricCriterion?.BabokTask?.KnowledgeAreaCode,
                    cs.RubricCriterion?.BabokTask?.KnowledgeAreaName))
                .ToList(),
            hr.ReviewedAt
        );

    private static AIPracticeFeedbackDto MapToDto(AIPracticeFeedback f)
    {
        var strengths = new List<string>();
        var weaknesses = new List<string>();
        var criticalErrors = new List<string>();

        try
        {
            if (!string.IsNullOrWhiteSpace(f.FeedbackItemsJson))
            {
                using var doc = JsonDocument.Parse(f.FeedbackItemsJson);
                if (doc.RootElement.TryGetProperty("strengths", out var sProp) && sProp.ValueKind == JsonValueKind.Array)
                {
                    strengths.AddRange(sProp.EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)));
                }
                if (doc.RootElement.TryGetProperty("weaknesses", out var wProp) && wProp.ValueKind == JsonValueKind.Array)
                {
                    weaknesses.AddRange(wProp.EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)));
                }
                if (doc.RootElement.TryGetProperty("criticalErrors", out var cProp) && cProp.ValueKind == JsonValueKind.Array)
                {
                    criticalErrors.AddRange(cProp.EnumerateArray().Select(e => e.GetString() ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
        catch (JsonException)
        {
        }

        return new AIPracticeFeedbackDto(
            f.Id,
            f.SubmissionId,
            f.Status.ToString(),
            f.SuggestedScore,
            f.Confidence,
            f.OverallFeedback,
            strengths,
            weaknesses,
            criticalErrors,
            f.CriterionScores
                .Select(cs => new AIFeedbackCriterionScoreDto(
                    cs.RubricCriterionId,
                    cs.RubricCriterion?.Code ?? string.Empty,
                    cs.RubricCriterion?.Name ?? string.Empty,
                    cs.Score,
                    cs.RubricCriterion?.MaxScore ?? 100m,
                    cs.Comment,
                    cs.EvidenceSnippet,
                    cs.RubricCriterion?.BabokTaskId,
                    cs.RubricCriterion?.BabokTask?.Code,
                    cs.RubricCriterion?.BabokTask?.KnowledgeAreaCode,
                    cs.RubricCriterion?.BabokTask?.KnowledgeAreaName))
                .ToList(),
            f.StartedAt,
            f.CompletedAt
        );
    }
}
