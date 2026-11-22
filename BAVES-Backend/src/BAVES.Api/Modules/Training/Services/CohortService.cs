using System.Globalization;
using BAVES.Api.Modules.Artifact.Repositories.Interfaces;
using BAVES.Api.Modules.Assessment.Repositories.Interfaces;
using BAVES.Api.Modules.Identity.Models.DTOs;
using BAVES.Api.Modules.Identity.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Entities;
using BAVES.Api.Modules.Training.Models.Enums;
using BAVES.Api.Modules.Training.Models.Requests;
using BAVES.Api.Modules.Training.Repositories.Interfaces;
using BAVES.Api.Modules.Training.Services.Interfaces;

namespace BAVES.Api.Modules.Training.Services;

public sealed class CohortService(
    ICohortRepository cohortRepository,
    IUserService userService,
    ISimulationSessionService simulationSessionService,
    IArtifactRepository artifactRepository,
    IAssessmentRepository assessmentRepository
) : ICohortService
{
    public async Task<CohortDetailDto> CreateCohortAsync(
        int instructorId,
        CreateCohortRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        ValidateDateRange(request.StartDate, request.EndDate);

        if (await cohortRepository.ExistsByCodeAsync(code, null, cancellationToken))
        {
            throw new InvalidOperationException($"Mã lớp học '{code}' đã tồn tại trong hệ thống.");
        }

        var entity = new Cohort
        {
            InstructorId = instructorId,
            Code = code,
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = request.Status ?? CohortStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow
        };

        cohortRepository.AddCohort(entity);
        await cohortRepository.SaveChangesAsync(cancellationToken);

        return new CohortDetailDto(
            entity.Id,
            entity.InstructorId,
            entity.Code,
            entity.Name,
            entity.Description,
            entity.StartDate,
            entity.EndDate,
            entity.Status,
            0,
            0,
            0,
            entity.CreatedAt,
            entity.UpdatedAt
        );
    }

    public async Task<IReadOnlyList<CohortSummaryDto>> GetCohortsAsync(
        int userId,
        string userRole,
        CohortStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        int? instructorFilter = string.Equals(userRole, "ADMIN", StringComparison.OrdinalIgnoreCase)
            ? null
            : userId;

        return await cohortRepository.GetCohortsAsync(instructorFilter, status, search, cancellationToken);
    }

    public async Task<CohortDetailDto?> GetCohortByIdAsync(
        int userId,
        string userRole,
        int cohortId,
        CancellationToken cancellationToken = default)
    {
        var detail = await cohortRepository.GetDetailByIdAsync(cohortId, cancellationToken);
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
            if (detail.InstructorId != userId)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập lớp học của giảng viên khác.");
            }
            return detail;
        }

        var isMember = await cohortRepository.IsLearnerInCohortAsync(cohortId, userId, cancellationToken);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("Bạn không phải là học viên thuộc lớp học này.");
        }

        return detail;
    }

    public async Task<CohortDetailDto?> UpdateCohortAsync(
        int userId,
        string userRole,
        int cohortId,
        UpdateCohortRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await cohortRepository.GetEntityByIdAsync(cohortId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(entity, userId, userRole);

        var code = request.Code.Trim().ToUpperInvariant();
        var name = request.Name.Trim();

        ValidateDateRange(request.StartDate, request.EndDate);

        if (!string.Equals(entity.Code, code, StringComparison.OrdinalIgnoreCase) &&
            await cohortRepository.ExistsByCodeAsync(code, cohortId, cancellationToken))
        {
            throw new InvalidOperationException($"Mã lớp học '{code}' đã tồn tại trong hệ thống.");
        }

        entity.Code = code;
        entity.Name = name;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await cohortRepository.SaveChangesAsync(cancellationToken);

        return await cohortRepository.GetDetailByIdAsync(cohortId, cancellationToken);
    }

    public async Task<CohortDetailDto?> UpdateCohortStatusAsync(
        int userId,
        string userRole,
        int cohortId,
        CohortStatus status,
        CancellationToken cancellationToken = default)
    {
        var entity = await cohortRepository.GetEntityByIdAsync(cohortId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(entity, userId, userRole);

        entity.Status = status;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await cohortRepository.SaveChangesAsync(cancellationToken);

        return await cohortRepository.GetDetailByIdAsync(cohortId, cancellationToken);
    }

    public async Task<bool> DeleteCohortAsync(
        int userId,
        string userRole,
        int cohortId,
        CancellationToken cancellationToken = default)
    {
        var entity = await cohortRepository.GetEntityWithCountsByIdAsync(cohortId, cancellationToken);
        if (entity is null)
        {
            return false;
        }

        EnsureOwnershipOrAdmin(entity, userId, userRole);

        if (entity.Status == CohortStatus.Draft &&
            entity.CohortMembers.Count == 0 &&
            entity.Assignments.Count == 0)
        {
            cohortRepository.RemoveCohort(entity);
        }
        else
        {
            entity.Status = CohortStatus.Archived;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await cohortRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<LearnerCohortDto>> GetMyCohortsAsync(
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        var items = await cohortRepository.GetLearnerCohortsAsync(learnerId, cancellationToken);
        if (items.Count == 0)
        {
            return items;
        }

        var instructorIds = items.Select(c => c.InstructorId).Distinct().ToList();
        var instructorMap = await userService.GetUserSummariesByIdsAsync(instructorIds, cancellationToken);

        return items.Select(c =>
        {
            instructorMap.TryGetValue(c.InstructorId, out var inst);
            return c with
            {
                InstructorName = inst?.FullName ?? $"Giảng viên #{c.InstructorId}",
                InstructorEmail = inst?.Email
            };
        }).ToList();
    }

    public async Task<IReadOnlyList<AvailableCohortDto>> GetAvailableCohortsAsync(
        int learnerId,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var allCohorts = await cohortRepository.GetCohortsAsync(null, null, search, cancellationToken);
        var activeCohorts = allCohorts
            .Where(c => c.Status == CohortStatus.Active || c.Status == CohortStatus.Draft)
            .ToList();

        if (activeCohorts.Count == 0)
        {
            return Array.Empty<AvailableCohortDto>();
        }

        var instructorIds = activeCohorts.Select(c => c.InstructorId).Distinct().ToList();
        var instructorMap = await userService.GetUserSummariesByIdsAsync(instructorIds, cancellationToken);

        var myCohorts = await cohortRepository.GetLearnerCohortsAsync(learnerId, cancellationToken);
        var myStatusMap = myCohorts.ToDictionary(m => m.CohortId, m => m.MemberStatus);

        return activeCohorts.Select(c =>
        {
            instructorMap.TryGetValue(c.InstructorId, out var inst);
            myStatusMap.TryGetValue(c.Id, out var currentStatus);
            CohortMemberStatus? memberStatus = myStatusMap.ContainsKey(c.Id) ? currentStatus : null;

            return new AvailableCohortDto(
                c.Id,
                c.InstructorId,
                inst?.FullName ?? $"Giảng viên #{c.InstructorId}",
                inst?.Email,
                c.Code,
                c.Name,
                c.Description,
                c.StartDate,
                c.EndDate,
                c.Status,
                c.ActiveMemberCount,
                c.AssignmentCount,
                memberStatus
            );
        }).ToList();
    }

    public async Task<LearnerCohortDto> RequestJoinCohortAsync(
        int learnerId,
        int? cohortId,
        string? code,
        CancellationToken cancellationToken = default)
    {
        Cohort? cohort = null;
        if (cohortId.HasValue)
        {
            cohort = await cohortRepository.GetEntityByIdAsync(cohortId.Value, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(code))
        {
            cohort = await cohortRepository.GetEntityByCodeAsync(code.Trim(), cancellationToken);
        }

        if (cohort is null)
        {
            throw new KeyNotFoundException("Không tìm thấy lớp học với mã hoặc ID đã cung cấp.");
        }

        var existing = await cohortRepository.GetCohortMemberEntityAsync(cohort.Id, learnerId, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        if (existing is not null)
        {
            if (existing.Status == CohortMemberStatus.Active || existing.Status == CohortMemberStatus.Completed)
            {
                throw new InvalidOperationException($"Bạn đã là thành viên chính thức của lớp '{cohort.Code} - {cohort.Name}'.");
            }
            if (existing.Status == CohortMemberStatus.Pending)
            {
                throw new InvalidOperationException($"Yêu cầu tham gia lớp '{cohort.Code}' của bạn đang chờ giảng viên phê duyệt.");
            }

            existing.Status = CohortMemberStatus.Pending;
            existing.JoinedAt = now;
            await cohortRepository.SaveChangesAsync(cancellationToken);
        }
        else
        {
            var newMember = new CohortMember
            {
                CohortId = cohort.Id,
                LearnerId = learnerId,
                Status = CohortMemberStatus.Pending,
                JoinedAt = now
            };
            cohortRepository.AddCohortMember(newMember);
            await cohortRepository.SaveChangesAsync(cancellationToken);
        }

        var inst = await userService.GetUserSummaryByIdAsync(cohort.InstructorId, cancellationToken);
        var detail = await cohortRepository.GetDetailByIdAsync(cohort.Id, cancellationToken);

        return new LearnerCohortDto(
            cohort.Id,
            cohort.InstructorId,
            cohort.Code,
            cohort.Name,
            cohort.Description,
            cohort.StartDate,
            cohort.EndDate,
            cohort.Status,
            CohortMemberStatus.Pending,
            now,
            detail?.ActiveAssignmentCount ?? 0,
            inst?.FullName ?? $"Giảng viên #{cohort.InstructorId}",
            inst?.Email
        );
    }

    // ─────────── COHORT MEMBERS (NHÓM 2) ───────────

    public async Task<IReadOnlyList<CohortMemberDto>?> GetCohortMembersAsync(
        int userId,
        string userRole,
        int cohortId,
        CohortMemberStatus? status,
        string? search,
        CancellationToken cancellationToken = default)
    {
        var cohort = await cohortRepository.GetEntityByIdAsync(cohortId, cancellationToken);
        if (cohort is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(cohort, userId, userRole);

        var members = await cohortRepository.GetCohortMembersAsync(cohortId, status, cancellationToken);
        if (members.Count == 0)
        {
            return Array.Empty<CohortMemberDto>();
        }

        var learnerIds = members.Select(m => m.LearnerId).Distinct().ToList();
        var userMap = await userService.GetUserSummariesByIdsAsync(learnerIds, cancellationToken);

        var dtos = members.Select(m =>
        {
            userMap.TryGetValue(m.LearnerId, out var user);
            return new CohortMemberDto(
                m.Id,
                m.CohortId,
                m.LearnerId,
                user?.FullName ?? $"Learner #{m.LearnerId}",
                user?.Email,
                user?.AvatarUrl,
                m.Status,
                m.JoinedAt
            );
        });

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            dtos = dtos.Where(d =>
                d.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (d.Email is not null && d.Email.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        return dtos.ToList();
    }

    public async Task<CohortMemberDto?> AddCohortMemberAsync(
        int userId,
        string userRole,
        int cohortId,
        AddCohortMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var cohort = await cohortRepository.GetEntityByIdAsync(cohortId, cancellationToken);
        if (cohort is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(cohort, userId, userRole);

        if (!request.LearnerId.HasValue && string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException("Vui lòng cung cấp LearnerId hoặc Email của học viên.");
        }

        UserSummaryDto? targetUser = request.LearnerId.HasValue
            ? await userService.GetUserSummaryByIdAsync(request.LearnerId.Value, cancellationToken)
            : await userService.GetUserSummaryByEmailAsync(request.Email!, cancellationToken);

        if (targetUser is null)
        {
            var identifier = request.LearnerId.HasValue
                ? $"ID {request.LearnerId.Value}"
                : $"Email '{request.Email}'";
            throw new KeyNotFoundException($"Không tìm thấy tài khoản học viên với {identifier}.");
        }

        var existing = await cohortRepository.GetCohortMemberEntityAsync(cohortId, targetUser.Id, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status == CohortMemberStatus.Active || existing.Status == CohortMemberStatus.Completed)
            {
                throw new InvalidOperationException($"Học viên '{targetUser.FullName}' ({targetUser.Email}) đã có trong lớp học này.");
            }

            existing.Status = CohortMemberStatus.Active;
            existing.JoinedAt = DateTimeOffset.UtcNow;
            await cohortRepository.SaveChangesAsync(cancellationToken);

            return new CohortMemberDto(
                existing.Id,
                existing.CohortId,
                existing.LearnerId,
                targetUser.FullName,
                targetUser.Email,
                targetUser.AvatarUrl,
                existing.Status,
                existing.JoinedAt
            );
        }

        var member = new CohortMember
        {
            CohortId = cohortId,
            LearnerId = targetUser.Id,
            Status = CohortMemberStatus.Active,
            JoinedAt = DateTimeOffset.UtcNow
        };

        cohortRepository.AddCohortMember(member);
        await cohortRepository.SaveChangesAsync(cancellationToken);

        return new CohortMemberDto(
            member.Id,
            member.CohortId,
            member.LearnerId,
            targetUser.FullName,
            targetUser.Email,
            targetUser.AvatarUrl,
            member.Status,
            member.JoinedAt
        );
    }

    public async Task<CohortMemberDto?> ApproveCohortMemberAsync(
        int userId,
        string userRole,
        int cohortId,
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        var cohort = await cohortRepository.GetEntityByIdAsync(cohortId, cancellationToken);
        if (cohort is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(cohort, userId, userRole);

        var existing = await cohortRepository.GetCohortMemberEntityAsync(cohortId, learnerId, cancellationToken);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy yêu cầu tham gia của học viên ID {learnerId} trong lớp học ID {cohortId}.");
        }

        existing.Status = CohortMemberStatus.Active;
        existing.JoinedAt = DateTimeOffset.UtcNow;
        await cohortRepository.SaveChangesAsync(cancellationToken);

        var targetUser = await userService.GetUserSummaryByIdAsync(learnerId, cancellationToken);

        return new CohortMemberDto(
            existing.Id,
            existing.CohortId,
            existing.LearnerId,
            targetUser?.FullName ?? $"Học viên #{learnerId}",
            targetUser?.Email,
            targetUser?.AvatarUrl,
            existing.Status,
            existing.JoinedAt
        );
    }

    public async Task<BatchAddCohortMembersResultDto?> BatchAddCohortMembersAsync(
        int userId,
        string userRole,
        int cohortId,
        BatchAddCohortMembersRequest request,
        CancellationToken cancellationToken = default)
    {
        var cohort = await cohortRepository.GetEntityByIdAsync(cohortId, cancellationToken);
        if (cohort is null)
        {
            return null;
        }

        EnsureOwnershipOrAdmin(cohort, userId, userRole);

        var requestedIds = request.LearnerIds?.Where(id => id > 0).Distinct().ToList() ?? [];
        var requestedEmails = request.Emails?
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        if (requestedIds.Count == 0 && requestedEmails.Count == 0)
        {
            throw new ArgumentException("Danh sách thêm hàng loạt phải chứa ít nhất 1 LearnerId hoặc 1 Email.");
        }

        var notFound = new List<string>();
        var resolvedUsers = new Dictionary<int, UserSummaryDto>();

        if (requestedIds.Count > 0)
        {
            var byIds = await userService.GetUserSummariesByIdsAsync(requestedIds, cancellationToken);
            foreach (var reqId in requestedIds)
            {
                if (byIds.TryGetValue(reqId, out var u))
                {
                    resolvedUsers[u.Id] = u;
                }
                else
                {
                    notFound.Add($"ID:{reqId.ToString(CultureInfo.InvariantCulture)}");
                }
            }
        }

        if (requestedEmails.Count > 0)
        {
            var byEmails = await userService.GetUserSummariesByEmailsAsync(requestedEmails, cancellationToken);
            var emailMap = byEmails
                .Where(u => !string.IsNullOrWhiteSpace(u.Email))
                .ToDictionary(u => u.Email!, StringComparer.OrdinalIgnoreCase);

            foreach (var reqEmail in requestedEmails)
            {
                if (emailMap.TryGetValue(reqEmail, out var u))
                {
                    resolvedUsers[u.Id] = u;
                }
                else
                {
                    notFound.Add(reqEmail);
                }
            }
        }

        var existingEntities = await cohortRepository.GetCohortMemberEntitiesByLearnerIdsAsync(
            cohortId,
            resolvedUsers.Keys.ToList(),
            cancellationToken);

        var existingMap = existingEntities.ToDictionary(e => e.LearnerId);

        int addedCount = 0;
        int reactivatedCount = 0;
        int alreadyActiveCount = 0;
        var now = DateTimeOffset.UtcNow;
        var newMembers = new List<CohortMember>();
        var affectedEntries = new List<(CohortMember Entity, UserSummaryDto User)>();

        foreach (var user in resolvedUsers.Values)
        {
            if (existingMap.TryGetValue(user.Id, out var existing))
            {
                if (existing.Status == CohortMemberStatus.Removed || existing.Status == CohortMemberStatus.Pending)
                {
                    existing.Status = CohortMemberStatus.Active;
                    existing.JoinedAt = now;
                    reactivatedCount++;
                    affectedEntries.Add((existing, user));
                }
                else
                {
                    alreadyActiveCount++;
                }
            }
            else
            {
                var newMember = new CohortMember
                {
                    CohortId = cohortId,
                    LearnerId = user.Id,
                    Status = CohortMemberStatus.Active,
                    JoinedAt = now
                };
                newMembers.Add(newMember);
                addedCount++;
                affectedEntries.Add((newMember, user));
            }
        }

        if (newMembers.Count > 0)
        {
            cohortRepository.AddCohortMembers(newMembers);
        }

        if (addedCount > 0 || reactivatedCount > 0)
        {
            await cohortRepository.SaveChangesAsync(cancellationToken);
        }

        var resultDtos = affectedEntries
            .Select(x => new CohortMemberDto(
                x.Entity.Id,
                x.Entity.CohortId,
                x.Entity.LearnerId,
                x.User.FullName,
                x.User.Email,
                x.User.AvatarUrl,
                x.Entity.Status,
                x.Entity.JoinedAt))
            .ToList();

        return new BatchAddCohortMembersResultDto(
            cohortId,
            addedCount,
            reactivatedCount,
            alreadyActiveCount,
            notFound,
            resultDtos
        );
    }

    public async Task<bool> RemoveCohortMemberAsync(
        int userId,
        string userRole,
        int cohortId,
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        var cohort = await cohortRepository.GetEntityByIdAsync(cohortId, cancellationToken);
        if (cohort is null)
        {
            return false;
        }

        EnsureOwnershipOrAdmin(cohort, userId, userRole);

        var member = await cohortRepository.GetCohortMemberEntityAsync(cohortId, learnerId, cancellationToken);
        if (member is null || member.Status == CohortMemberStatus.Removed)
        {
            return false;
        }

        member.Status = CohortMemberStatus.Removed;
        await cohortRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ─────────── COHORT ANALYTICS (NHÓM 5) ───────────

    public async Task<CohortAnalyticsDto?> GetCohortAnalyticsAsync(
        int userId,
        string userRole,
        int cohortId,
        CancellationToken cancellationToken = default)
    {
        var detail = await cohortRepository.GetDetailByIdAsync(cohortId, cancellationToken);
        if (detail is null)
        {
            return null;
        }

        if (!string.Equals(userRole, "ADMIN", StringComparison.OrdinalIgnoreCase) &&
            detail.InstructorId != userId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền xem báo cáo phân tích của lớp học khác.");
        }

        var members = await cohortRepository.GetCohortMembersAsync(cohortId, CohortMemberStatus.Active, cancellationToken);
        var learnerIds = members.Select(m => m.LearnerId).Distinct().ToList();

        var userMap = await userService.GetUserSummariesByIdsAsync(learnerIds, cancellationToken);
        var sessions = await simulationSessionService.GetSessionsByLearnerIdsAsync(learnerIds, cancellationToken);

        var sessionIds = sessions.Select(s => s.SessionId).Distinct().ToList();
        var submissions = await artifactRepository.GetSubmissionsBySessionIdsAsync(sessionIds, cancellationToken);

        var submissionIds = submissions.Select(sub => sub.Id).Distinct().ToList();
        var aiFeedbacks = await assessmentRepository.GetFeedbackDtosBySubmissionIdsAsync(submissionIds, cancellationToken);
        var humanReviews = await assessmentRepository.GetHumanReviewDtosBySubmissionIdsAsync(submissionIds, cancellationToken);

        var sessionsByLearner = sessions
            .GroupBy(s => s.LearnerId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var submissionsBySession = submissions
            .GroupBy(s => s.SessionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var aiBySubmission = aiFeedbacks
            .GroupBy(f => f.SubmissionId)
            .ToDictionary(g => g.Key, g => g.First());

        var humanBySubmission = humanReviews
            .GroupBy(r => r.SubmissionId)
            .ToDictionary(g => g.Key, g => g.First());

        var learnerSummaries = new List<CohortLearnerAnalyticsDto>(learnerIds.Count);
        var allAiScores = new List<decimal>();
        var allFinalScores = new List<decimal>();
        int learnersWithSubmission = 0;

        foreach (var learnerId in learnerIds.OrderBy(id => id))
        {
            userMap.TryGetValue(learnerId, out var user);
            sessionsByLearner.TryGetValue(learnerId, out var learnerSessions);

            var learnerSessionList = learnerSessions ?? [];
            var learnerSubs = learnerSessionList
                .Where(s => submissionsBySession.ContainsKey(s.SessionId))
                .SelectMany(s => submissionsBySession[s.SessionId])
                .ToList();

            if (learnerSubs.Count > 0)
            {
                learnersWithSubmission++;
            }

            var learnerAiScores = new List<decimal>();
            var learnerFinalScores = new List<decimal>();

            foreach (var sub in learnerSubs)
            {
                if (aiBySubmission.TryGetValue(sub.Id, out var ai) && ai.SuggestedScore.HasValue)
                {
                    learnerAiScores.Add(ai.SuggestedScore.Value);
                    allAiScores.Add(ai.SuggestedScore.Value);
                }

                if (humanBySubmission.TryGetValue(sub.Id, out var hr))
                {
                    learnerFinalScores.Add(hr.FinalScore);
                    allFinalScores.Add(hr.FinalScore);
                }
            }

            learnerSummaries.Add(new CohortLearnerAnalyticsDto(
                learnerId,
                user?.FullName ?? $"Learner #{learnerId}",
                user?.Email,
                learnerSessionList.Count,
                learnerSubs.Count,
                learnerFinalScores.Count,
                learnerAiScores.Count > 0 ? Math.Round(learnerAiScores.Average(), 2) : null,
                learnerFinalScores.Count > 0 ? Math.Round(learnerFinalScores.Average(), 2) : null
            ));
        }

        // Thống kê điểm chuẩn theo từng tiêu chí Rubric (ưu tiên điểm Chuyên gia, fallback sang điểm AI nếu chưa có)
        var criterionEntries = humanReviews.Count > 0
            ? humanReviews.SelectMany(hr => hr.CriterionScores.Select(cs => (
                Code: cs.CriterionCode,
                Name: cs.CriterionName,
                Score: cs.Score,
                MaxScore: cs.MaxScore > 0 ? cs.MaxScore : 100m))).ToList()
            : aiFeedbacks.SelectMany(ai => ai.CriterionScores.Select(cs => (
                Code: cs.CriterionCode,
                Name: cs.CriterionName,
                Score: cs.Score,
                MaxScore: cs.MaxScore > 0 ? cs.MaxScore : 100m))).ToList();

        var criterionBenchmarks = criterionEntries
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var first = g.First();
                var avgScore = Math.Round(g.Average(x => x.Score), 2);
                var avgPct = Math.Round(g.Average(x => (x.Score / x.MaxScore) * 100m), 2);
                var belowThreshold = g.Count(x => (x.Score / x.MaxScore) * 100m < 60m);

                return new CohortCriterionBenchmarkDto(
                    first.Code,
                    first.Name,
                    first.MaxScore,
                    g.Count(),
                    avgScore,
                    avgPct,
                    belowThreshold
                );
            })
            .OrderBy(c => c.AverageAchievementPercent)
            .ToList();

        var completionRate = learnerIds.Count > 0
            ? Math.Round((decimal)learnersWithSubmission * 100m / learnerIds.Count, 2)
            : 0m;

        return new CohortAnalyticsDto(
            detail.Id,
            detail.Code,
            detail.Name,
            learnerIds.Count,
            detail.TotalAssignmentCount,
            sessions.Count,
            submissions.Count,
            humanReviews.Count,
            allAiScores.Count > 0 ? Math.Round(allAiScores.Average(), 2) : null,
            allFinalScores.Count > 0 ? Math.Round(allFinalScores.Average(), 2) : null,
            completionRate,
            criterionBenchmarks,
            learnerSummaries
        );
    }

    private static void ValidateDateRange(DateOnly? startDate, DateOnly? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate.Value < startDate.Value)
        {
            throw new ArgumentException("Ngày kết thúc (EndDate) phải lớn hơn hoặc bằng ngày bắt đầu (StartDate).");
        }
    }

    private static void EnsureOwnershipOrAdmin(Cohort cohort, int userId, string userRole)
    {
        if (!string.Equals(userRole, "ADMIN", StringComparison.OrdinalIgnoreCase) &&
            cohort.InstructorId != userId)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa hoặc thao tác trên lớp học của giảng viên khác.");
        }
    }
}
