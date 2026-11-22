using System.Text.Json;
using BAVES.Api.Modules.Analysis.Repositories.Interfaces;
using BAVES.Api.Modules.Artifact.Repositories.Interfaces;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Models.Entities;
using BAVES.Api.Modules.Assessment.Models.Enums;
using BAVES.Api.Modules.Assessment.Models.Requests;
using BAVES.Api.Modules.Assessment.Repositories.Interfaces;
using BAVES.Api.Modules.Assessment.Services.Interfaces;
using BAVES.Api.Modules.Commerce.Services.Interfaces;
using BAVES.Api.Modules.Planning.Repositories.Interfaces;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Data;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Assessment.Services;

public sealed class AssessmentService(
    IAssessmentRepository assessmentRepository,
    IArtifactRepository artifactRepository,
    ISimulationSessionService simulationSessionService,
    IAnalysisItemRepository analysisItemRepository,
    IPlanningRepository planningRepository,
    IAiRuntimeClient aiRuntimeClient,
    IEntitlementService? entitlementService = null,
    SimulationDbContext? simulationDb = null
) : IAssessmentService
{
    // ─────────── RUBRIC & BABOK TASKS AUTHORING ───────────

    public async Task<IReadOnlyList<BabokTaskDto>> GetBabokTasksAsync(
        CancellationToken cancellationToken = default)
    {
        return await assessmentRepository.GetBabokTasksAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RubricDetailDto>> GetRubricsAsync(
        int? scenarioVersionId,
        RubricStatus? status,
        bool isInstructorOrAdmin,
        CancellationToken cancellationToken = default)
    {
        var effectiveStatus = isInstructorOrAdmin
            ? status
            : RubricStatus.Published;

        return await assessmentRepository.GetRubricsAsync(scenarioVersionId, effectiveStatus, cancellationToken);
    }

    public async Task<RubricDetailDto?> GetRubricDetailByIdAsync(
        int rubricId,
        CancellationToken cancellationToken = default)
    {
        return await assessmentRepository.GetRubricDetailByIdAsync(rubricId, cancellationToken);
    }

    public async Task<RubricDetailDto> CreateRubricAsync(
        int creatorUserId,
        CreateRubricRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateRubricCriteriaAsync(request.Criteria, cancellationToken);

        var nextVersion = await assessmentRepository.GetNextRubricVersionAsync(
            request.ScenarioVersionId, cancellationToken);

        var rubric = new Rubric
        {
            ScenarioVersionId = request.ScenarioVersionId,
            CreatedByUserId = creatorUserId,
            Name = request.Name.Trim(),
            Version = nextVersion,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = request.PublishImmediately ? RubricStatus.Published : RubricStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            RubricCriteria = request.Criteria.Select((c, idx) => new RubricCriterion
            {
                BabokTaskId = c.BabokTaskId,
                Code = c.Code.Trim().ToUpperInvariant(),
                Name = c.Name.Trim(),
                Description = c.Description.Trim(),
                Weight = NormalizeCriterionWeight(c.Weight, request.Criteria),
                MaxScore = c.MaxScore,
                EvidenceGuidance = string.IsNullOrWhiteSpace(c.EvidenceGuidance) ? null : c.EvidenceGuidance.Trim(),
                DisplayOrder = c.DisplayOrder > 0 ? c.DisplayOrder : idx + 1,
                IsRequired = c.IsRequired
            }).ToList()
        };

        return await assessmentRepository.CreateRubricAsync(rubric, cancellationToken);
    }

    public async Task<RubricDetailDto?> UpdateRubricAsync(
        int rubricId,
        UpdateRubricRequest request,
        CancellationToken cancellationToken = default)
    {
        var rubric = await assessmentRepository.GetTrackedRubricByIdAsync(rubricId, cancellationToken);
        if (rubric is null)
        {
            return null;
        }

        if (rubric.Status != RubricStatus.Draft)
        {
            throw new ConflictException(
                $"Chỉ có thể chỉnh sửa Rubric khi còn ở trạng thái Draft (hiện tại: {rubric.Status}).");
        }

        await ValidateRubricCriteriaAsync(request.Criteria, cancellationToken);

        rubric.Name = request.Name.Trim();
        rubric.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        rubric.UpdatedAt = DateTimeOffset.UtcNow;

        var newCriteria = request.Criteria.Select((c, idx) => new RubricCriterion
        {
            RubricId = rubric.Id,
            BabokTaskId = c.BabokTaskId,
            Code = c.Code.Trim().ToUpperInvariant(),
            Name = c.Name.Trim(),
            Description = c.Description.Trim(),
            Weight = NormalizeCriterionWeight(c.Weight, request.Criteria),
            MaxScore = c.MaxScore,
            EvidenceGuidance = string.IsNullOrWhiteSpace(c.EvidenceGuidance) ? null : c.EvidenceGuidance.Trim(),
            DisplayOrder = c.DisplayOrder > 0 ? c.DisplayOrder : idx + 1,
            IsRequired = c.IsRequired
        }).ToList();

        return await assessmentRepository.UpdateRubricAsync(rubric, newCriteria, cancellationToken);
    }

    public async Task<RubricDetailDto?> PublishRubricAsync(
        int rubricId,
        CancellationToken cancellationToken = default)
    {
        var rubric = await assessmentRepository.GetTrackedRubricByIdAsync(rubricId, cancellationToken);
        if (rubric is null)
        {
            return null;
        }

        if (rubric.Status == RubricStatus.Archived)
        {
            throw new ConflictException("Không thể phát hành (Publish) Rubric đã bị lưu trữ (Archived).");
        }

        if (rubric.RubricCriteria.Count == 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Criteria"] = ["Rubric phải có ít nhất 1 tiêu chí chấm điểm trước khi phát hành."]
            });
        }

        return await assessmentRepository.PublishRubricAsync(rubricId, cancellationToken);
    }

    private async Task ValidateRubricCriteriaAsync(
        IReadOnlyCollection<RubricCriterionInputRequest>? criteria,
        CancellationToken cancellationToken)
    {
        if (criteria is null || criteria.Count == 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Criteria"] = ["Rubric phải có ít nhất 1 tiêu chí chấm điểm."]
            });
        }

        var duplicateCodes = criteria
            .GroupBy(c => c.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateCodes.Count > 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Criteria"] = [$"Mã tiêu chí (Code) bị trùng lặp trong Rubric: {string.Join(", ", duplicateCodes)}."]
            });
        }

        var totalWeight = criteria.Sum(c => c.Weight);
        // Cho phép tổng trọng số bằng 1.0 (hệ thập phân) hoặc 100.0 (hệ phần trăm)
        var isValidSum = Math.Abs(totalWeight - 1.0m) <= 0.01m || Math.Abs(totalWeight - 100.0m) <= 0.1m;
        if (!isValidSum)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Criteria"] = [$"Tổng trọng số (Weight) của các tiêu chí phải bằng 100 (hoặc 1.0). Tổng hiện tại: {totalWeight}."]
            });
        }

        var babokTaskIds = criteria.Select(c => c.BabokTaskId).Distinct().ToList();
        var allTasksExist = await assessmentRepository.AllBabokTasksExistAsync(babokTaskIds, cancellationToken);
        if (!allTasksExist)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Criteria"] = ["Một hoặc nhiều BabokTaskId không tồn tại trong hệ thống."]
            });
        }
    }

    private static decimal NormalizeCriterionWeight(
        decimal weight,
        IReadOnlyCollection<RubricCriterionInputRequest> allCriteria)
    {
        var total = allCriteria.Sum(c => c.Weight);
        // Nếu người dùng nhập theo thang 100 (vd: 20, 30, 50) thì quy đổi về thang 0..1 (0.20, 0.30, 0.50) để đồng nhất với DB
        return total > 1.5m ? Math.Round(weight / 100m, 4) : Math.Round(weight, 4);
    }

    // ─────────── AI PRACTICE FEEDBACK ───────────

    public async Task<AIPracticeFeedbackDto?> GetFeedbackBySubmissionAsync(
        int learnerId,
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        var submission = await artifactRepository.GetSubmissionDtoByIdAsync(submissionId, cancellationToken);
        if (submission is null)
        {
            return null;
        }

        // Kiểm tra quyền sở hữu session
        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, submission.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        return await assessmentRepository.GetFeedbackDtoBySubmissionIdAsync(submissionId, cancellationToken);
    }

    public async Task<AIPracticeFeedbackDto?> EvaluateSubmissionAsync(
        int learnerId,
        int submissionId,
        string? headerProvider,
        string? headerModel,
        string? headerApiKey,
        CancellationToken cancellationToken = default)
    {
        var submission = await artifactRepository.GetSubmissionDtoByIdAsync(submissionId, cancellationToken);
        if (submission is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, submission.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        var version = await artifactRepository.GetVersionEntityByIdAsync(submission.ArtifactVersionId, cancellationToken);
        if (version is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy ArtifactVersion ID {submission.ArtifactVersionId}.");
        }

        var startedAt = DateTimeOffset.UtcNow;

        // 1. Lấy Rubric cho ScenarioVersion này (hoặc default Rubric)
        var rubric = await assessmentRepository.GetRubricByScenarioVersionAsync(session.ScenarioVersionId, cancellationToken)
                     ?? await assessmentRepository.GetDefaultRubricAsync(cancellationToken);

        if (rubric is null || rubric.RubricCriteria.Count == 0)
        {
            throw new InvalidOperationException("Không tìm thấy Rubric tiêu chuẩn để đánh giá bài nộp này.");
        }

        var rubricDto = new AssessmentRubricDto(
            rubric.Id,
            rubric.Name,
            rubric.RubricCriteria.Select(c => new AssessmentRubricCriterionDto(
                c.Id,
                c.Code,
                c.Name,
                c.Description,
                (double)c.Weight,
                (double)c.MaxScore)).ToList()
        );

        // 2. Tách các đề mục (Sections) từ nội dung của Artifact
        var sections = ParseSections(version.StructuredContentJson, version.PlainTextContent);
        var artifactSubmissionDto = new AssessmentArtifactSubmissionDto(
            version.ArtifactId,
            version.Artifact.ArtifactTemplate?.ArtifactType.ToString() ?? "Generic",
            version.Artifact.Title,
            version.VersionNo,
            sections
        );

        // 3. Lấy danh sách Analysis Items & Evidence từ session để hỗ trợ đối chiếu truy vết (Traceability)
        var analysisItems = await analysisItemRepository.GetItemsBySessionAsync(session.Id, null, null, cancellationToken);
        var evidenceList = analysisItems
            .Select(item => new AssessmentEvidenceDto(
                item.Id,
                item.Description ?? string.Empty,
                item.ItemType.ToString(),
                item.Title))
            .ToList();

        // 4. Chuẩn bị kết nối AI BYOK
        var provider = !string.IsNullOrWhiteSpace(headerProvider) ? headerProvider.Trim() : "OpenAI";
        var model = !string.IsNullOrWhiteSpace(headerModel) ? headerModel.Trim() : null;
        var apiKey = !string.IsNullOrWhiteSpace(headerApiKey) ? headerApiKey.Trim() : null;

        var runtimeRequest = new AssessmentRuntimeRequest(
            submissionId,
            artifactSubmissionDto,
            [artifactSubmissionDto],
            rubricDto,
            evidenceList,
            new AssessmentAiConnectionDto(provider, model, apiKey)
        );

        // 5. Gọi sang AI Assessor Runtime (FastAPI /v1/assessment/evaluate)
        var runtimeResponse = await aiRuntimeClient.EvaluateAssessmentAsync(runtimeRequest, cancellationToken);

        // 6. Lưu kết quả phản hồi vào Database
        var feedbackItemsJson = JsonSerializer.Serialize(new
        {
            strengths = runtimeResponse.Strengths,
            weaknesses = runtimeResponse.Weaknesses,
            criticalErrors = runtimeResponse.CriticalErrors,
            passed = runtimeResponse.Passed
        });

        var feedback = new AIPracticeFeedback
        {
            SubmissionId = submissionId,
            Provider = provider,
            Model = model ?? (provider == "Gemini" ? "gemini-1.5-flash" : "gpt-4o-mini"),
            Status = AIFeedbackStatus.Completed,
            OverallFeedback = runtimeResponse.Passed
                ? $"Đạt yêu cầu sơ bộ (Điểm: {runtimeResponse.OverallScore:F1}/100)"
                : $"Chưa đạt yêu cầu (Điểm: {runtimeResponse.OverallScore:F1}/100)",
            SuggestedScore = (decimal)runtimeResponse.OverallScore,
            Confidence = 0.9m,
            FeedbackItemsJson = feedbackItemsJson,
            StartedAt = startedAt,
            CompletedAt = DateTimeOffset.UtcNow
        };

        // Ghép điểm chi tiết theo từng Criterion
        var criterionScores = new List<AIFeedbackCriterionScore>();
        foreach (var cs in runtimeResponse.CriterionScores)
        {
            var matchedCriterion = rubric.RubricCriteria.FirstOrDefault(c =>
                string.Equals(c.Code, cs.CriterionCode, StringComparison.OrdinalIgnoreCase));

            if (matchedCriterion is not null)
            {
                criterionScores.Add(new AIFeedbackCriterionScore
                {
                    RubricCriterionId = matchedCriterion.Id,
                    Score = (decimal)cs.Score,
                    Comment = cs.Comment,
                    EvidenceSnippet = null
                });
            }
        }

        return await assessmentRepository.SavePracticeFeedbackAsync(feedback, criterionScores, cancellationToken);
    }

    // ─────────── HUMAN EXPERT REVIEW ───────────

    public async Task<ReviewRequestDto?> CreateReviewRequestAsync(
        int learnerId,
        int submissionId,
        CreateReviewRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var submission = await artifactRepository.GetSubmissionDtoByIdAsync(submissionId, cancellationToken);
        if (submission is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, submission.SessionId, cancellationToken);
        if (session is null)
        {
            throw new UnauthorizedAccessException("Bạn không có quyền gửi yêu cầu đánh giá cho bài nộp của phiên mô phỏng này.");
        }

        var existingOpen = await assessmentRepository.GetOpenReviewRequestBySubmissionIdAsync(submissionId, cancellationToken);
        if (existingOpen is not null)
        {
            throw new InvalidOperationException("Đã tồn tại yêu cầu đánh giá đang chờ xử lý cho bài nộp này.");
        }

        var aiFeedback = await assessmentRepository.GetFeedbackDtoBySubmissionIdAsync(submissionId, cancellationToken);
        var sprintReviews = await LoadSprintReviewsAsync(submission.SessionId, cancellationToken);
        if (aiFeedback is null && sprintReviews.All(sprint => sprint.Status != "Finalized"))
        {
            throw new InvalidOperationException(
                "Hãy hoàn tất ít nhất một đánh giá AI (chấm bài nộp hoặc chốt sprint) trước khi gửi chuyên gia.");
        }

        if (entitlementService is not null)
        {
            var credits = await entitlementService.GetReviewCreditsAsync(learnerId, cancellationToken);
            if (credits < 1)
            {
                throw new InvalidOperationException(
                    "Bạn không đủ lượt chấm bài chuyên gia. Vui lòng mua gói Review Credit trước khi gửi yêu cầu.");
            }
        }

        var entity = new ReviewRequest
        {
            SubmissionId = submissionId,
            LearnerId = learnerId,
            RequestNote = request.RequestNote,
            Status = ReviewRequestStatus.Pending,
            RequestedAt = DateTimeOffset.UtcNow
        };

        var created = await assessmentRepository.CreateReviewRequestAsync(entity, cancellationToken);
        if (entitlementService is not null && created is not null)
        {
            var consumed = await entitlementService.ConsumeReviewCreditAsync(learnerId, created.Id, cancellationToken);
            if (!consumed)
            {
                throw new InvalidOperationException(
                    "Không trừ được lượt chấm bài chuyên gia. Vui lòng kiểm tra số dư credit và thử lại.");
            }
        }

        return created;
    }

    public async Task<IReadOnlyList<ReviewRequestDto>> GetReviewRequestsAsync(
        int userId,
        string userRole,
        string? scope,
        CancellationToken cancellationToken = default)
    {
        if (string.Equals(userRole, "LEARNER", StringComparison.OrdinalIgnoreCase))
        {
            return await assessmentRepository.GetReviewRequestsAsync(learnerId: userId, reviewerId: null, queueOnly: false, cancellationToken);
        }

        if (IsExpertReviewerRole(userRole) ||
            string.Equals(userRole, "INSTRUCTOR", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(userRole, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(scope, "mine", StringComparison.OrdinalIgnoreCase))
            {
                return await assessmentRepository.GetReviewRequestsAsync(learnerId: null, reviewerId: userId, queueOnly: false, cancellationToken);
            }

            return await assessmentRepository.GetReviewRequestsAsync(learnerId: null, reviewerId: null, queueOnly: true, cancellationToken);
        }

        return Array.Empty<ReviewRequestDto>();
    }

    public async Task<bool> AcceptReviewRequestAsync(
        int reviewerId,
        int requestId,
        CancellationToken cancellationToken = default)
    {
        var reviewReq = await assessmentRepository.GetReviewRequestByIdAsync(requestId, cancellationToken);
        if (reviewReq is null)
        {
            return false;
        }

        if (reviewReq.Status != ReviewRequestStatus.Pending || reviewReq.AssignedReviewerId.HasValue)
        {
            return false;
        }

        return await assessmentRepository.TryAcceptReviewRequestAsync(requestId, reviewerId, cancellationToken);
    }

    public async Task<ReviewCaseDossierDto?> GetReviewCaseDossierAsync(
        int reviewerId,
        int requestId,
        CancellationToken cancellationToken = default)
    {
        var reviewReq = await assessmentRepository.GetReviewRequestByIdAsync(requestId, cancellationToken);
        if (reviewReq is null)
        {
            return null;
        }

        if (reviewReq.AssignedReviewerId != reviewerId)
        {
            throw new UnauthorizedAccessException("Bạn chưa được phân công hoặc chưa tiếp nhận yêu cầu đánh giá này.");
        }

        var submission = await artifactRepository.GetSubmissionDtoByIdAsync(reviewReq.SubmissionId, cancellationToken);
        if (submission is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài nộp ID {reviewReq.SubmissionId}.");
        }

        var version = await artifactRepository.GetVersionEntityByIdAsync(submission.ArtifactVersionId, cancellationToken);
        if (version is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy ArtifactVersion ID {submission.ArtifactVersionId}.");
        }

        var artifactSnapshot = new ArtifactSnapshotDto(
            version.ArtifactId,
            version.Artifact.Title,
            version.Artifact.ArtifactTemplate?.ArtifactType.ToString() ?? "Generic",
            version.VersionNo,
            version.PlainTextContent,
            version.StructuredContentJson
        );

        var plan = await planningRepository.GetPlanBySessionAsync(submission.SessionId, cancellationToken);
        var planSnapshot = plan is null ? null : new PlanSnapshotDto(
            plan.Objective,
            plan.Approach,
            null
        );

        var analysisItems = await analysisItemRepository.GetItemsBySessionAsync(submission.SessionId, null, null, cancellationToken);
        var analysisSnapshots = analysisItems.Select(item => new AnalysisItemSnapshotDto(
            item.Id,
            item.Code,
            item.Title,
            item.ItemType.ToString(),
            item.Status.ToString(),
            item.Description
        )).ToList();

        var aiFeedback = await assessmentRepository.GetFeedbackDtoBySubmissionIdAsync(reviewReq.SubmissionId, cancellationToken);
        var sprintReviews = await LoadSprintReviewsAsync(submission.SessionId, cancellationToken);

        var session = await simulationSessionService.GetSessionByIdAsync(reviewReq.LearnerId, submission.SessionId, cancellationToken);
        var scenarioVersionId = session?.ScenarioVersionId ?? 0;

        var rubric = await assessmentRepository.GetRubricByScenarioVersionAsync(scenarioVersionId, cancellationToken)
                     ?? await assessmentRepository.GetDefaultRubricAsync(cancellationToken);

        var rubricCriteria = rubric?.RubricCriteria.Select(c => new ReviewRubricCriterionDto(
            c.Id,
            c.Code,
            c.Name,
            c.Description,
            c.Weight,
            c.MaxScore
        )).ToList() ?? new List<ReviewRubricCriterionDto>();

        return new ReviewCaseDossierDto(
            reviewReq.Id,
            reviewReq.SubmissionId,
            submission.SessionId,
            reviewReq.LearnerId,
            reviewReq.Status.ToString(),
            reviewReq.RequestNote,
            reviewReq.RequestedAt,
            artifactSnapshot,
            planSnapshot,
            analysisSnapshots,
            aiFeedback,
            sprintReviews,
            rubricCriteria
        );
    }

    public async Task<HumanReviewDto?> CompleteReviewAsync(
        int reviewerId,
        int requestId,
        CompleteReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var reviewReq = await assessmentRepository.GetReviewRequestByIdAsync(requestId, cancellationToken);
        if (reviewReq is null)
        {
            return null;
        }

        if (reviewReq.AssignedReviewerId != reviewerId)
        {
            throw new UnauthorizedAccessException("Bạn không phải chuyên gia được phân công cho yêu cầu đánh giá này.");
        }

        if (reviewReq.Status != ReviewRequestStatus.Accepted)
        {
            throw new InvalidOperationException($"Yêu cầu đánh giá đang ở trạng thái {reviewReq.Status}, không thể hoàn thành.");
        }

        var submission = await artifactRepository.GetSubmissionDtoByIdAsync(reviewReq.SubmissionId, cancellationToken);
        if (submission is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy bài nộp ID {reviewReq.SubmissionId}.");
        }

        var session = await simulationSessionService.GetSessionByIdAsync(reviewReq.LearnerId, submission.SessionId, cancellationToken);
        var scenarioVersionId = session?.ScenarioVersionId ?? 0;

        var rubric = await assessmentRepository.GetRubricByScenarioVersionAsync(scenarioVersionId, cancellationToken)
                     ?? await assessmentRepository.GetDefaultRubricAsync(cancellationToken);

        if (rubric is null || rubric.RubricCriteria.Count == 0)
        {
            throw new InvalidOperationException("Không tìm thấy Rubric tiêu chuẩn để chấm bài.");
        }

        decimal totalWeightedScore = 0m;
        decimal totalWeight = 0m;
        var criterionScores = new List<HumanReviewCriterionScore>();

        foreach (var inputScore in request.CriterionScores)
        {
            var matched = rubric.RubricCriteria.FirstOrDefault(c => c.Id == inputScore.RubricCriterionId);
            if (matched is null)
            {
                throw new ArgumentException($"Tiêu chí Rubric ID {inputScore.RubricCriterionId} không thuộc Rubric này.");
            }

            if (inputScore.Score < 0 || inputScore.Score > matched.MaxScore)
            {
                throw new ArgumentException($"Điểm tiêu chí '{matched.Name}' phải từ 0 đến {matched.MaxScore}.");
            }

            var normalizedScore = matched.MaxScore > 0 ? (inputScore.Score / matched.MaxScore) * 100m : inputScore.Score;
            totalWeightedScore += normalizedScore * matched.Weight;
            totalWeight += matched.Weight;

            criterionScores.Add(new HumanReviewCriterionScore
            {
                RubricCriterionId = matched.Id,
                Score = inputScore.Score,
                Comment = inputScore.Comment
            });
        }

        var finalScore = totalWeight > 0 ? Math.Round(totalWeightedScore / totalWeight, 2) : 0m;

        var humanReview = new HumanReview
        {
            SubmissionId = reviewReq.SubmissionId,
            RubricId = rubric.Id,
            ReviewerId = reviewerId,
            FinalScore = finalScore,
            OverallFeedback = request.OverallFeedback,
            ReviewedAt = DateTimeOffset.UtcNow
        };

        var savedReview = await assessmentRepository.SaveHumanReviewAsync(humanReview, criterionScores, cancellationToken);

        await assessmentRepository.CompleteReviewRequestAsync(requestId, cancellationToken);
        await artifactRepository.FinalizeSubmissionAsync(reviewReq.SubmissionId, cancellationToken);

        return savedReview;
    }

    public async Task<ReviewRequestDto?> RequestRevisionAsync(
        int reviewerId,
        int requestId,
        RequestRevisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var reviewReq = await assessmentRepository.GetReviewRequestByIdAsync(requestId, cancellationToken);
        if (reviewReq is null)
        {
            return null;
        }

        if (reviewReq.AssignedReviewerId.HasValue && reviewReq.AssignedReviewerId != reviewerId)
        {
            throw new UnauthorizedAccessException("Bạn không phải chuyên gia được phân công cho yêu cầu đánh giá này.");
        }

        if (reviewReq.Status is not (ReviewRequestStatus.Pending or ReviewRequestStatus.Accepted))
        {
            throw new InvalidOperationException(
                $"Yêu cầu đánh giá đang ở trạng thái {reviewReq.Status}, không thể yêu cầu làm lại.");
        }

        // Pending: tiếp nhận trước rồi mới gửi yêu cầu làm lại (gắn reviewer).
        if (reviewReq.Status == ReviewRequestStatus.Pending && !reviewReq.AssignedReviewerId.HasValue)
        {
            var accepted = await assessmentRepository.TryAcceptReviewRequestAsync(
                requestId,
                reviewerId,
                cancellationToken);
            if (!accepted)
            {
                throw new InvalidOperationException(
                    "Không tiếp nhận được yêu cầu đánh giá để gửi yêu cầu làm lại.");
            }
        }

        var ok = await assessmentRepository.RequestRevisionAsync(
            requestId,
            reviewerId,
            request.Reason,
            cancellationToken);

        if (!ok)
        {
            throw new InvalidOperationException("Không gửi được yêu cầu làm lại cho bài nộp này.");
        }

        var updated = await assessmentRepository.GetReviewRequestByIdAsync(requestId, cancellationToken);
        if (updated is null)
        {
            return null;
        }

        return new ReviewRequestDto(
            updated.Id,
            updated.SubmissionId,
            updated.LearnerId,
            updated.AssignedReviewerId,
            updated.RequestNote,
            updated.Status.ToString(),
            updated.RequestedAt,
            updated.AcceptedAt,
            updated.CompletedAt);
    }

    // ─────────── FINAL SUBMISSION REPORT ───────────

    public async Task<FinalSubmissionReportDto?> GetFinalReportAsync(
        int userId,
        string userRole,
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        var submission = await artifactRepository.GetSubmissionDtoByIdAsync(submissionId, cancellationToken);
        if (submission is null)
        {
            return null;
        }

        if (string.Equals(userRole, "LEARNER", StringComparison.OrdinalIgnoreCase))
        {
            var session = await simulationSessionService.GetSessionByIdAsync(userId, submission.SessionId, cancellationToken);
            if (session is null)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập báo cáo của bài nộp này.");
            }
        }

        var aiFeedback = await assessmentRepository.GetFeedbackDtoBySubmissionIdAsync(submissionId, cancellationToken);
        var humanReview = await assessmentRepository.GetHumanReviewDtoBySubmissionIdAsync(submissionId, cancellationToken);
        var sprintReviews = await LoadSprintReviewsAsync(submission.SessionId, cancellationToken);
        var finalScore = humanReview?.FinalScore ?? aiFeedback?.SuggestedScore;

        return new FinalSubmissionReportDto(
            submission.Id,
            submission.SessionId,
            submission.Status.ToString(),
            aiFeedback,
            humanReview,
            finalScore,
            submission.SubmittedAt,
            submission.FinalizedAt,
            sprintReviews
        );
    }

    public async Task<CompetencyProgressDto> GetCompetencyProgressAsync(
        int learnerId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await simulationSessionService.GetLearnerSessionsHistoryAsync(
            learnerId, null, null, null, cancellationToken);

        if (sessions.Count == 0)
        {
            var emptyAreas = BuildKnowledgeAreaProgress(
                await assessmentRepository.GetBabokTasksAsync(cancellationToken),
                [],
                []);

            return new CompetencyProgressDto(0, 0, null, [], emptyAreas);
        }

        var sessionIds = sessions.Select(session => session.Id).ToList();
        var submissions = await artifactRepository.GetSubmissionsBySessionIdsAsync(sessionIds, cancellationToken);
        var latestBySession = submissions
            .GroupBy(submission => submission.SessionId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderByDescending(item => item.SubmittedAt).ThenByDescending(item => item.Id).First());

        var submissionIds = latestBySession.Values.Select(item => item.Id).ToList();
        var aiFeedbacks = submissionIds.Count == 0
            ? []
            : await assessmentRepository.GetFeedbackDtosBySubmissionIdsAsync(submissionIds, cancellationToken);
        var humanReviews = submissionIds.Count == 0
            ? []
            : await assessmentRepository.GetHumanReviewDtosBySubmissionIdsAsync(submissionIds, cancellationToken);

        var aiBySubmission = aiFeedbacks.ToDictionary(item => item.SubmissionId);
        var expertBySubmission = humanReviews.ToDictionary(item => item.SubmissionId);

        var sprintAverages = await LoadSprintAveragesAsync(sessionIds, cancellationToken);

        var rows = sessions.Select(session =>
        {
            latestBySession.TryGetValue(session.Id, out var submission);
            decimal? aiScore = null;
            decimal? expertScore = null;
            if (submission is not null)
            {
                if (aiBySubmission.TryGetValue(submission.Id, out var ai))
                {
                    aiScore = ai.SuggestedScore;
                }

                if (expertBySubmission.TryGetValue(submission.Id, out var expert))
                {
                    expertScore = expert.FinalScore;
                }
            }

            sprintAverages.TryGetValue(session.Id, out var sprintAverage);
            var display = expertScore ?? aiScore ?? sprintAverage;

            return new CompetencySessionScoreDto(
                session.Id,
                session.ScenarioVersionId,
                session.AttemptNo,
                session.Status.ToString(),
                session.StartedAt,
                session.CompletedAt,
                submission?.Id,
                aiScore,
                expertScore,
                sprintAverage,
                display);
        }).ToList();

        var scored = rows.Where(row => row.DisplayScore.HasValue).Select(row => row.DisplayScore!.Value).ToList();
        var average = scored.Count == 0 ? (decimal?)null : Math.Round(scored.Average(), 2);
        var completed = rows.Count(row =>
            string.Equals(row.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
            row.CompletedAt.HasValue);

        var knowledgeAreas = BuildKnowledgeAreaProgress(
            await assessmentRepository.GetBabokTasksAsync(cancellationToken),
            aiFeedbacks,
            humanReviews);

        return new CompetencyProgressDto(rows.Count, completed, average, rows, knowledgeAreas);
    }

    // ─────────── HELPERS ───────────

    private static List<AssessmentArtifactSectionDto> ParseSections(string? structuredJson, string? plainText)
    {
        var sections = new List<AssessmentArtifactSectionDto>();

        if (!string.IsNullOrWhiteSpace(structuredJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(structuredJson);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        var content = prop.Value.ValueKind == JsonValueKind.String
                            ? prop.Value.GetString() ?? string.Empty
                            : prop.Value.GetRawText();

                        sections.Add(new AssessmentArtifactSectionDto(
                            prop.Name,
                            prop.Name,
                            content));
                    }
                }
            }
            catch (JsonException)
            {
                // Fallback nếu JSON bị lỗi format
            }
        }

        if (sections.Count == 0 && !string.IsNullOrWhiteSpace(plainText))
        {
            sections.Add(new AssessmentArtifactSectionDto(
                "BODY",
                "Nội dung tài liệu",
                plainText));
        }

        return sections;
    }

    private async Task<IReadOnlyList<SprintReviewEvidenceDto>> LoadSprintReviewsAsync(
        int sessionId,
        CancellationToken cancellationToken)
    {
        if (simulationDb is null)
        {
            return [];
        }

        var reviews = await simulationDb.SessionSprintReviews
            .AsNoTracking()
            .Where(review => review.SessionId == sessionId)
            .OrderBy(review => review.SprintNo)
            .ToListAsync(cancellationToken);

        return reviews.Select(review => new SprintReviewEvidenceDto(
            review.SprintNo,
            review.Status,
            review.SprintScore,
            review.Decision,
            review.CompressedDigest,
            DeserializeStringList(review.ChallengeQuestionsJson)
        )).ToList();
    }

    private async Task<Dictionary<int, decimal?>> LoadSprintAveragesAsync(
        IReadOnlyCollection<int> sessionIds,
        CancellationToken cancellationToken)
    {
        if (simulationDb is null || sessionIds.Count == 0)
        {
            return new Dictionary<int, decimal?>();
        }

        var reviews = await simulationDb.SessionSprintReviews
            .AsNoTracking()
            .Where(review => sessionIds.Contains(review.SessionId) && review.Status == "Finalized" && review.SprintScore != null)
            .Select(review => new { review.SessionId, review.SprintScore })
            .ToListAsync(cancellationToken);

        return reviews
            .GroupBy(item => item.SessionId)
            .ToDictionary(
                group => group.Key,
                group => (decimal?)Math.Round(group.Average(item => item.SprintScore!.Value), 2));
    }

    private static readonly string[] BabokKaOrder =
    [
        "BAPM", "EC", "RLCM", "SA", "RADD", "SE"
    ];

    private static IReadOnlyList<CompetencyKnowledgeAreaDto> BuildKnowledgeAreaProgress(
        IReadOnlyList<BabokTaskDto> babokTasks,
        IReadOnlyList<AIPracticeFeedbackDto> aiFeedbacks,
        IReadOnlyList<HumanReviewDto> humanReviews)
    {
        var areas = babokTasks
            .GroupBy(task => task.KnowledgeAreaCode)
            .Select(group => new
            {
                Code = group.Key,
                Name = group.First().KnowledgeAreaName,
                Description = group
                    .Select(t => t.Purpose)
                    .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p)),
                TaskCodes = group.OrderBy(t => t.DisplayOrder).Select(t => t.Code).ToList(),
                Acc = new List<decimal>()
            })
            .ToList();

        var byCode = areas.ToDictionary(a => a.Code, StringComparer.OrdinalIgnoreCase);

        void Accrue(string? knowledgeAreaCode, decimal score, decimal maxScore)
        {
            if (maxScore <= 0 || string.IsNullOrWhiteSpace(knowledgeAreaCode))
            {
                return;
            }

            if (!byCode.TryGetValue(knowledgeAreaCode, out var area))
            {
                return;
            }

            area.Acc.Add(Math.Round((score / maxScore) * 100m, 2));
        }

        // Ưu tiên điểm chuyên gia; AI chỉ bổ sung khi cùng criterion chưa có expert.
        var expertCriterionIds = new HashSet<int>();
        foreach (var review in humanReviews)
        {
            foreach (var score in review.CriterionScores)
            {
                expertCriterionIds.Add(score.RubricCriterionId);
                Accrue(score.KnowledgeAreaCode, score.Score, score.MaxScore);
            }
        }

        foreach (var feedback in aiFeedbacks)
        {
            foreach (var score in feedback.CriterionScores)
            {
                if (expertCriterionIds.Contains(score.RubricCriterionId))
                {
                    continue;
                }

                Accrue(score.KnowledgeAreaCode, score.Score, score.MaxScore);
            }
        }

        return areas
            .Select(area => new CompetencyKnowledgeAreaDto(
                area.Code,
                area.Name,
                area.Acc.Count == 0 ? null : Math.Round(area.Acc.Average(), 1),
                area.Acc.Count,
                area.Description,
                area.TaskCodes))
            .OrderBy(area =>
            {
                var idx = Array.FindIndex(
                    BabokKaOrder,
                    code => string.Equals(code, area.Code, StringComparison.OrdinalIgnoreCase));
                return idx < 0 ? 99 : idx;
            })
            .ThenBy(area => area.Code)
            .ToList();
    }

    private static IReadOnlyList<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static bool IsExpertReviewerRole(string? userRole) =>
        string.Equals(userRole, "EXPERT", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(userRole, "BAExpert", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(userRole, "BAEXPERT", StringComparison.OrdinalIgnoreCase);
}
