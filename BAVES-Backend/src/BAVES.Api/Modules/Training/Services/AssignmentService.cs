using System.Globalization;
using System.Text;
using BAVES.Api.Modules.Artifact.Models.DTOs;
using BAVES.Api.Modules.Artifact.Models.Enums;
using BAVES.Api.Modules.Artifact.Repositories.Interfaces;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Repositories.Interfaces;
using BAVES.Api.Modules.Identity.Services.Interfaces;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Models.Requests;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Entities;
using BAVES.Api.Modules.Training.Models.Enums;
using BAVES.Api.Modules.Training.Models.Requests;
using BAVES.Api.Modules.Training.Repositories.Interfaces;
using BAVES.Api.Modules.Training.Services.Interfaces;

namespace BAVES.Api.Modules.Training.Services;

public sealed class AssignmentService(
    IAssignmentRepository assignmentRepository,
    ICohortRepository cohortRepository,
    IScenarioCatalogService scenarioCatalogService,
    ISimulationSessionService simulationSessionService,
    IArtifactRepository artifactRepository,
    IAssessmentRepository assessmentRepository,
    IUserService userService
) : IAssignmentService
{
    public async Task<AssignmentDetailDto> CreateAssignmentAsync(
        int userId,
        string userRole,
        CreateAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CohortId.HasValue == request.LearnerId.HasValue)
        {
            throw new ArgumentException(
                "Bài tập phải được giao cho đúng 1 đối tượng: một Lớp học (CohortId) HOẶC một Học viên cá nhân (LearnerId).");
        }

        ValidateDueRange(request.StartAt, request.DueAt);

        if (request.MaxAttempts.HasValue && request.MaxAttempts.Value <= 0)
        {
            throw new ArgumentException("Số lần làm tối đa (MaxAttempts) phải lớn hơn 0.");
        }

        var scenarioVersion = await scenarioCatalogService.GetVersionInfoAsync(
            request.ScenarioVersionId,
            cancellationToken);

        if (scenarioVersion is null || !scenarioVersion.IsPublished)
        {
            throw new KeyNotFoundException(
                $"ScenarioVersion ID {request.ScenarioVersionId} không tồn tại hoặc chưa được xuất bản.");
        }

        if (request.CohortId.HasValue)
        {
            var cohort = await cohortRepository.GetEntityByIdAsync(request.CohortId.Value, cancellationToken);
            if (cohort is null)
            {
                throw new KeyNotFoundException($"Không tìm thấy lớp học ID {request.CohortId.Value}.");
            }

            if (!string.Equals(userRole, "ADMIN", StringComparison.OrdinalIgnoreCase) &&
                cohort.InstructorId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền giao bài tập cho lớp học của giảng viên khác.");
            }
        }
        else if (request.LearnerId.HasValue)
        {
            var learner = await userService.GetUserSummaryByIdAsync(request.LearnerId.Value, cancellationToken);
            if (learner is null)
            {
                throw new KeyNotFoundException($"Không tìm thấy tài khoản học viên ID {request.LearnerId.Value}.");
            }
        }

        var entity = new Assignment
        {
            Title = request.Title.Trim(),
            Instructions = string.IsNullOrWhiteSpace(request.Instructions) ? null : request.Instructions.Trim(),
            ScenarioVersionId = request.ScenarioVersionId,
            CohortId = request.CohortId,
            LearnerId = request.LearnerId,
            RubricId = request.RubricId,
            AssignedByUserId = userId,
            StartAt = request.StartAt,
            DueAt = request.DueAt,
            MaxAttempts = request.MaxAttempts,
            Status = request.Status ?? AssignmentStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        assignmentRepository.AddAssignment(entity);
        await assignmentRepository.SaveChangesAsync(cancellationToken);

        return (await assignmentRepository.GetDetailByIdAsync(entity.Id, cancellationToken))!;
    }

    public async Task<IReadOnlyList<AssignmentSummaryDto>> GetAssignmentsAsync(
        int userId,
        string userRole,
        int? cohortId,
        AssignmentStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        int? instructorFilter = string.Equals(userRole, "ADMIN", StringComparison.OrdinalIgnoreCase)
            ? null
            : userId;

        return await assignmentRepository.GetAssignmentsAsync(
            instructorFilter,
            cohortId,
            status,
            search,
            cancellationToken);
    }

    public async Task<AssignmentDetailDto?> GetAssignmentByIdAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var detail = await assignmentRepository.GetDetailByIdAsync(assignmentId, cancellationToken);
        if (detail is null)
        {
            return null;
        }

        if (string.Equals(userRole, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(userRole, "EXPERT", StringComparison.OrdinalIgnoreCase))
        {
            return detail;
        }

        if (string.Equals(userRole, "INSTRUCTOR", StringComparison.OrdinalIgnoreCase))
        {
            if (detail.AssignedByUserId == userId)
            {
                return detail;
            }

            if (detail.CohortId.HasValue)
            {
                var cohort = await cohortRepository.GetEntityByIdAsync(detail.CohortId.Value, cancellationToken);
                if (cohort is not null && cohort.InstructorId == userId)
                {
                    return detail;
                }
            }

            throw new UnauthorizedAccessException("Bạn không có quyền truy cập bài tập của giảng viên khác.");
        }

        var eligible = await assignmentRepository.IsLearnerEligibleForAssignmentAsync(
            assignmentId,
            userId,
            cancellationToken);

        if (!eligible || detail.Status == AssignmentStatus.Draft || detail.Status == AssignmentStatus.Archived)
        {
            throw new UnauthorizedAccessException("Bạn không thuộc đối tượng được giao bài tập này.");
        }

        return detail;
    }

    public async Task<AssignmentDetailDto?> UpdateAssignmentAsync(
        int userId,
        string userRole,
        int assignmentId,
        UpdateAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await assignmentRepository.GetEntityByIdAsync(assignmentId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(entity, userId, userRole);
        ValidateDueRange(request.StartAt, request.DueAt);

        if (request.MaxAttempts.HasValue && request.MaxAttempts.Value <= 0)
        {
            throw new ArgumentException("Số lần làm tối đa (MaxAttempts) phải lớn hơn 0.");
        }

        entity.Title = request.Title.Trim();
        entity.Instructions = string.IsNullOrWhiteSpace(request.Instructions) ? null : request.Instructions.Trim();
        entity.RubricId = request.RubricId;
        entity.StartAt = request.StartAt;
        entity.DueAt = request.DueAt;
        entity.MaxAttempts = request.MaxAttempts;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await assignmentRepository.SaveChangesAsync(cancellationToken);
        return await assignmentRepository.GetDetailByIdAsync(assignmentId, cancellationToken);
    }

    public async Task<AssignmentDetailDto?> PublishAssignmentAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await assignmentRepository.GetEntityByIdAsync(assignmentId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(entity, userId, userRole);

        if (entity.Status == AssignmentStatus.Archived)
        {
            throw new InvalidOperationException("Không thể phát hành bài tập đã lưu trữ (Archived).");
        }

        entity.Status = AssignmentStatus.Active;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await assignmentRepository.SaveChangesAsync(cancellationToken);
        return await assignmentRepository.GetDetailByIdAsync(assignmentId, cancellationToken);
    }

    public async Task<AssignmentDetailDto?> CloseAssignmentAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await assignmentRepository.GetEntityByIdAsync(assignmentId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(entity, userId, userRole);

        entity.Status = AssignmentStatus.Closed;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await assignmentRepository.SaveChangesAsync(cancellationToken);
        return await assignmentRepository.GetDetailByIdAsync(assignmentId, cancellationToken);
    }

    // ─────────── NHÓM 4: LEARNER ASSIGNMENT FLOW ───────────

    public Task<IReadOnlyList<int>> GetVisibleScenarioVersionIdsForLearnerAsync(
        int learnerId,
        CancellationToken cancellationToken = default) =>
        assignmentRepository.GetVisibleScenarioVersionIdsForLearnerAsync(learnerId, cancellationToken);

    public Task<bool> IsLearnerAssignedToScenarioVersionAsync(
        int learnerId,
        int scenarioVersionId,
        CancellationToken cancellationToken = default) =>
        assignmentRepository.IsLearnerAssignedToScenarioVersionAsync(
            learnerId,
            scenarioVersionId,
            cancellationToken);

    public async Task<IReadOnlyList<LearnerAssignmentDto>> GetMyAssignmentsAsync(
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        var assignments = await assignmentRepository.GetLearnerVisibleAssignmentsAsync(
            learnerId,
            cancellationToken);

        if (assignments.Count == 0)
        {
            return Array.Empty<LearnerAssignmentDto>();
        }

        var assignmentIds = assignments.Select(a => a.Id).ToList();
        var sessions = await simulationSessionService.GetLearnerSessionsByAssignmentIdsAsync(
            learnerId,
            assignmentIds,
            cancellationToken);

        var sessionsByAssignment = sessions
            .GroupBy(s => s.AssignmentId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.StartedAt).ToList());

        var now = DateTimeOffset.UtcNow;
        var result = new List<LearnerAssignmentDto>(assignments.Count);

        foreach (var a in assignments)
        {
            sessionsByAssignment.TryGetValue(a.Id, out var mySessions);
            var attemptsUsed = mySessions?.Count ?? 0;
            int? remainingAttempts = a.MaxAttempts.HasValue
                ? Math.Max(0, a.MaxAttempts.Value - attemptsUsed)
                : null;

            var latestSession = mySessions?.FirstOrDefault();
            var isOverdue = a.DueAt.HasValue && now > a.DueAt.Value;

            string progressStatus;
            if (latestSession is null)
            {
                progressStatus = isOverdue ? "Overdue" : "NotStarted";
            }
            else if (latestSession.Status == SessionStatus.Completed)
            {
                progressStatus = "Completed";
            }
            else if (latestSession.Status == SessionStatus.Submitted)
            {
                progressStatus = "Submitted";
            }
            else if (latestSession.Status == SessionStatus.Active)
            {
                progressStatus = "InProgress";
            }
            else
            {
                progressStatus = isOverdue ? "Overdue" : "Abandoned";
            }

            result.Add(new LearnerAssignmentDto(
                a.Id,
                a.Title,
                a.Instructions,
                a.ScenarioVersionId,
                a.CohortId,
                a.CohortCode,
                a.CohortName,
                a.StartAt,
                a.DueAt,
                a.MaxAttempts,
                a.Status,
                progressStatus,
                attemptsUsed,
                remainingAttempts,
                latestSession?.SessionId,
                isOverdue
            ));
        }

        return result;
    }

    public async Task<SessionDto> StartAssignmentSessionAsync(
        int learnerId,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await assignmentRepository.GetEntityByIdAsync(assignmentId, cancellationToken);
        if (assignment is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài tập ID {assignmentId}.");
        }

        var eligible = await assignmentRepository.IsLearnerEligibleForAssignmentAsync(
            assignmentId,
            learnerId,
            cancellationToken);

        if (!eligible)
        {
            throw new UnauthorizedAccessException("Bạn không thuộc đối tượng được giao bài tập này.");
        }

        if (assignment.Status != AssignmentStatus.Active)
        {
            throw new InvalidOperationException(
                $"Bài tập hiện đang ở trạng thái '{assignment.Status}', chưa mở để bắt đầu làm bài.");
        }

        var now = DateTimeOffset.UtcNow;

        if (assignment.StartAt.HasValue && now < assignment.StartAt.Value)
        {
            throw new InvalidOperationException(
                $"Bài tập sẽ mở vào lúc {assignment.StartAt.Value:dd/MM/yyyy HH:mm}.");
        }

        if (assignment.DueAt.HasValue && now > assignment.DueAt.Value)
        {
            throw new InvalidOperationException(
                "Đã quá hạn nộp bài tập (DueAt), không thể bắt đầu lượt làm bài mới.");
        }

        if (assignment.MaxAttempts.HasValue)
        {
            var attemptsUsed = await simulationSessionService.CountAttemptsByAssignmentAsync(
                learnerId,
                assignmentId,
                cancellationToken);

            if (attemptsUsed >= assignment.MaxAttempts.Value)
            {
                throw new InvalidOperationException(
                    $"Bạn đã sử dụng hết số lượt làm bài tối đa ({assignment.MaxAttempts.Value} lần) cho bài tập này.");
            }
        }

        return await simulationSessionService.StartSessionAsync(
            learnerId,
            new StartSessionRequest
            {
                ScenarioVersionId = assignment.ScenarioVersionId,
                AssignmentId = assignment.Id
            },
            cancellationToken);
    }

    // ─────────── NHÓM 5: GRADEBOOK & CSV EXPORT ───────────

    public async Task<AssignmentGradebookDto?> GetAssignmentGradebookAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await assignmentRepository.GetEntityByIdAsync(assignmentId, cancellationToken);
        if (assignment is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(assignment, userId, userRole);

        var recipientLearnerIds = new HashSet<int>();
        if (assignment.CohortId.HasValue)
        {
            var members = await cohortRepository.GetCohortMembersAsync(
                assignment.CohortId.Value,
                null,
                cancellationToken);

            foreach (var m in members)
            {
                recipientLearnerIds.Add(m.LearnerId);
            }
        }
        else if (assignment.LearnerId.HasValue)
        {
            recipientLearnerIds.Add(assignment.LearnerId.Value);
        }

        var sessions = await simulationSessionService.GetSessionsByAssignmentIdAsync(
            assignmentId,
            cancellationToken);

        foreach (var s in sessions)
        {
            recipientLearnerIds.Add(s.LearnerId);
        }

        var userMap = await userService.GetUserSummariesByIdsAsync(
            recipientLearnerIds.ToList(),
            cancellationToken);

        var sessionIds = sessions.Select(s => s.SessionId).Distinct().ToList();
        var submissions = await artifactRepository.GetSubmissionsBySessionIdsAsync(
            sessionIds,
            cancellationToken);

        var submissionIds = submissions.Select(sub => sub.Id).Distinct().ToList();
        var aiFeedbacks = await assessmentRepository.GetFeedbackDtosBySubmissionIdsAsync(
            submissionIds,
            cancellationToken);
        var humanReviews = await assessmentRepository.GetHumanReviewDtosBySubmissionIdsAsync(
            submissionIds,
            cancellationToken);

        var sessionsByLearner = sessions
            .GroupBy(s => s.LearnerId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.StartedAt).ToList());

        var submissionsBySession = submissions
            .GroupBy(s => s.SessionId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SubmittedAt).ToList());

        var aiBySubmission = aiFeedbacks
            .GroupBy(f => f.SubmissionId)
            .ToDictionary(g => g.Key, g => g.First());

        var humanBySubmission = humanReviews
            .GroupBy(r => r.SubmissionId)
            .ToDictionary(g => g.Key, g => g.First());

        var now = DateTimeOffset.UtcNow;
        var entries = new List<GradebookEntryDto>(recipientLearnerIds.Count);

        int notStartedCount = 0;
        int inProgressCount = 0;
        int submittedCount = 0;
        int gradedCount = 0;
        int lateCount = 0;
        var aiScores = new List<decimal>();
        var finalScores = new List<decimal>();

        foreach (var learnerId in recipientLearnerIds.OrderBy(id => id))
        {
            userMap.TryGetValue(learnerId, out var user);
            sessionsByLearner.TryGetValue(learnerId, out var learnerSessions);

            var attemptsUsed = learnerSessions?.Count ?? 0;
            var latestSession = learnerSessions?.FirstOrDefault();

            SubmissionDto? latestSubmission = null;
            if (learnerSessions is not null)
            {
                foreach (var sess in learnerSessions)
                {
                    if (submissionsBySession.TryGetValue(sess.SessionId, out var sessSubs) && sessSubs.Count > 0)
                    {
                        latestSubmission = sessSubs.First();
                        break;
                    }
                }
            }

            AIPracticeFeedbackDto? aiFeedback = null;
            HumanReviewDto? humanReview = null;

            if (latestSubmission is not null)
            {
                aiBySubmission.TryGetValue(latestSubmission.Id, out aiFeedback);
                humanBySubmission.TryGetValue(latestSubmission.Id, out humanReview);
            }

            var isLate = latestSubmission is not null &&
                         assignment.DueAt.HasValue &&
                         latestSubmission.SubmittedAt > assignment.DueAt.Value;

            if (isLate)
            {
                lateCount++;
            }

            string progressStatus;
            if (humanReview is not null || latestSubmission?.Status == SubmissionStatus.Finalized)
            {
                progressStatus = "Graded";
                gradedCount++;
            }
            else if (latestSubmission is not null)
            {
                progressStatus = "Submitted";
                submittedCount++;
            }
            else if (latestSession is not null)
            {
                progressStatus = "InProgress";
                inProgressCount++;
            }
            else
            {
                progressStatus = assignment.DueAt.HasValue && now > assignment.DueAt.Value
                    ? "Overdue"
                    : "NotStarted";
                notStartedCount++;
            }

            if (aiFeedback?.SuggestedScore is not null)
            {
                aiScores.Add(aiFeedback.SuggestedScore.Value);
            }

            if (humanReview is not null)
            {
                finalScores.Add(humanReview.FinalScore);
            }

            entries.Add(new GradebookEntryDto(
                learnerId,
                user?.FullName ?? $"Learner #{learnerId}",
                user?.Email,
                attemptsUsed,
                latestSession?.SessionId,
                latestSubmission?.Id,
                progressStatus,
                isLate,
                latestSubmission?.SubmittedAt,
                latestSubmission?.FinalizedAt ?? humanReview?.ReviewedAt,
                aiFeedback?.SuggestedScore,
                humanReview?.FinalScore,
                humanReview?.ReviewerId
            ));
        }

        decimal? avgAi = aiScores.Count > 0 ? Math.Round(aiScores.Average(), 2) : null;
        decimal? avgFinal = finalScores.Count > 0 ? Math.Round(finalScores.Average(), 2) : null;

        return new AssignmentGradebookDto(
            assignment.Id,
            assignment.Title,
            assignment.ScenarioVersionId,
            assignment.CohortId,
            assignment.Cohort?.Code,
            assignment.Cohort?.Name,
            assignment.DueAt,
            assignment.MaxAttempts,
            recipientLearnerIds.Count,
            notStartedCount,
            inProgressCount,
            submittedCount,
            gradedCount,
            lateCount,
            avgAi,
            avgFinal,
            entries
        );
    }

    public async Task<byte[]?> ExportAssignmentGradebookCsvAsync(
        int userId,
        string userRole,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var gradebook = await GetAssignmentGradebookAsync(
            userId,
            userRole,
            assignmentId,
            cancellationToken);

        if (gradebook is null)
        {
            return null;
        }

        var sb = new StringBuilder();
        sb.AppendLine("LearnerId,FullName,Email,AttemptsUsed,ProgressStatus,IsLate,SubmittedAt,AiSuggestedScore,ExpertFinalScore,FinalizedAt");

        foreach (var row in gradebook.Entries)
        {
            sb.Append(row.LearnerId.ToString(CultureInfo.InvariantCulture)).Append(',');
            sb.Append(EscapeCsv(row.FullName)).Append(',');
            sb.Append(EscapeCsv(row.Email ?? string.Empty)).Append(',');
            sb.Append(row.AttemptsUsed.ToString(CultureInfo.InvariantCulture)).Append(',');
            sb.Append(EscapeCsv(row.ProgressStatus)).Append(',');
            sb.Append(row.IsLate ? "Late" : "OnTime").Append(',');
            sb.Append(EscapeCsv(row.SubmittedAt?.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture) ?? string.Empty)).Append(',');
            sb.Append(row.AiSuggestedScore?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty).Append(',');
            sb.Append(row.ExpertFinalScore?.ToString("F2", CultureInfo.InvariantCulture) ?? string.Empty).Append(',');
            sb.AppendLine(EscapeCsv(row.FinalizedAt?.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture) ?? string.Empty));
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var bodyBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[preamble.Length + bodyBytes.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(bodyBytes, 0, result, preamble.Length, bodyBytes.Length);
        return result;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static void ValidateDueRange(DateTimeOffset? startAt, DateTimeOffset? dueAt)
    {
        if (startAt.HasValue && dueAt.HasValue && dueAt.Value < startAt.Value)
        {
            throw new ArgumentException("Thời hạn nộp (DueAt) phải lớn hơn hoặc bằng thời gian mở đề (StartAt).");
        }
    }

    private static void EnsureOwnershipOrAdmin(Assignment assignment, int userId, string userRole)
    {
        if (string.Equals(userRole, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var isCreator = assignment.AssignedByUserId == userId;
        var isCohortOwner = assignment.Cohort is not null && assignment.Cohort.InstructorId == userId;

        if (!isCreator && !isCohortOwner)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập hoặc chỉnh sửa bài tập của giảng viên khác.");
        }
    }
}
