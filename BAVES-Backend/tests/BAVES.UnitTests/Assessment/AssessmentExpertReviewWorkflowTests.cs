using BAVES.Api.Modules.Analysis.Models.DTOs;
using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Analysis.Repositories.Interfaces;
using BAVES.Api.Modules.Artifact.Models.DTOs;
using BAVES.Api.Modules.Artifact.Models.Entities;
using BAVES.Api.Modules.Artifact.Models.Enums;
using BAVES.Api.Modules.Artifact.Models.Requests;
using BAVES.Api.Modules.Artifact.Repositories.Interfaces;
using BAVES.Api.Modules.Artifact.Services;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Models.Entities;
using BAVES.Api.Modules.Assessment.Models.Enums;
using BAVES.Api.Modules.Assessment.Models.Requests;
using BAVES.Api.Modules.Assessment.Repositories.Interfaces;
using BAVES.Api.Modules.Assessment.Services;
using BAVES.Api.Modules.Planning.Repositories.Interfaces;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using ArtifactEntity = BAVES.Api.Modules.Artifact.Models.Entities.Artifact;

namespace BAVES.UnitTests.Assessment;

/// <summary>
/// WF-04: Đánh giá Năng lực BA Đa tầng qua AI & Chuyên gia (AI-Assisted Assessment & Expert Calibration Review)
/// Owner: Trần Văn An
/// </summary>
public class AssessmentExpertReviewWorkflowTests
{
    private readonly Mock<IAssessmentRepository> _assessmentRepoMock = new();
    private readonly Mock<IArtifactRepository> _artifactRepoMock = new();
    private readonly Mock<ISimulationSessionService> _simSessionServiceMock = new();
    private readonly Mock<IAnalysisItemRepository> _analysisRepoMock = new();
    private readonly Mock<IPlanningRepository> _planningRepoMock = new();
    private readonly Mock<IAiRuntimeClient> _aiRuntimeClientMock = new();

    private readonly AssessmentService _sutAssessment;
    private readonly ArtifactService _sutArtifact;

    public AssessmentExpertReviewWorkflowTests()
    {
        _sutAssessment = new AssessmentService(
            _assessmentRepoMock.Object,
            _artifactRepoMock.Object,
            _simSessionServiceMock.Object,
            _analysisRepoMock.Object,
            _planningRepoMock.Object,
            _aiRuntimeClientMock.Object
        );

        _sutArtifact = new ArtifactService(
            _artifactRepoMock.Object,
            _simSessionServiceMock.Object
        );
    }

    #region F-ASM-01: Đóng băng Bài nộp & Chấm điểm AI Sơ bộ

    [Fact]
    public async Task TC_ASM_01_SubmitArtifactVersionAsync_SubmitsArtifactAndFreezesSubmission()
    {
        // Arrange
        const int learnerId = 42;
        const int sessionId = 1;
        const int artifactVersionId = 10;

        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionDto(sessionId, 1, null, 1, SessionStatus.Active, DateTimeOffset.UtcNow, null, null, []));

        var artifactEntity = new ArtifactEntity { Id = 5, SessionId = sessionId };
        var versionEntity = new ArtifactVersion { Id = artifactVersionId, ArtifactId = 5, Artifact = artifactEntity, VersionNo = 1 };

        _artifactRepoMock.Setup(r => r.GetVersionEntityByIdAsync(artifactVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(versionEntity);
        _artifactRepoMock.Setup(r => r.GetNextSubmissionAttemptNoAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var expectedDto = new SubmissionDto(
            Id: 100,
            SessionId: sessionId,
            ArtifactVersionId: artifactVersionId,
            AttemptNo: 1,
            Status: SubmissionStatus.Submitted,
            SubmittedAt: DateTimeOffset.UtcNow,
            FinalizedAt: null
        );

        _artifactRepoMock.Setup(r => r.CreateSubmissionAsync(It.IsAny<Submission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var request = new CreateSubmissionRequest { ArtifactVersionId = artifactVersionId };

        // Act
        var result = await _sutArtifact.SubmitArtifactVersionAsync(learnerId, sessionId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(SubmissionStatus.Submitted);
        result.AttemptNo.Should().Be(1);

        _artifactRepoMock.Verify(r => r.CreateSubmissionAsync(It.Is<Submission>(s =>
            s.SessionId == sessionId &&
            s.ArtifactVersionId == artifactVersionId &&
            s.Status == SubmissionStatus.Submitted
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_ASM_02_ArtifactFreeze_UpdateDraftOrFreezeAfterSubmitted_ThrowsInvalidOperationException()
    {
        // Arrange
        const int learnerId = 42;
        const int artifactId = 5;

        var submittedArtifact = new ArtifactEntity
        {
            Id = artifactId,
            SessionId = 1,
            Status = ArtifactStatus.Submitted // Submitted!
        };

        _artifactRepoMock.Setup(r => r.GetArtifactEntityByIdAsync(artifactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submittedArtifact);
        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(learnerId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionDto(1, 1, null, 1, SessionStatus.Active, DateTimeOffset.UtcNow, null, null, []));

        // Act & Assert 1: Update draft on submitted artifact
        var actUpdate = () => _sutArtifact.UpdateArtifactDraftAsync(learnerId, artifactId, new UpdateArtifactDraftRequest());
        await actUpdate.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Không thể chỉnh sửa tài liệu đã nộp*");

        // Act & Assert 2: Freeze new version on submitted artifact
        var actFreeze = () => _sutArtifact.FreezeArtifactVersionAsync(learnerId, artifactId, new CreateArtifactVersionRequest());
        await actFreeze.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Không thể đóng băng phiên bản cho tài liệu đã nộp*");
    }

    [Fact]
    public async Task TC_ASM_03_EvaluateSubmissionAsync_CallsAiAssessor_CalculatesSuggestedScoreAndCriterionScores()
    {
        // Arrange
        const int learnerId = 42;
        const int submissionId = 100;
        const int versionId = 10;
        const int sessionId = 1;

        var submissionDto = new SubmissionDto(submissionId, sessionId, versionId, 1, SubmissionStatus.Submitted, DateTimeOffset.UtcNow, null);
        _artifactRepoMock.Setup(r => r.GetSubmissionDtoByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submissionDto);
        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionDto(sessionId, 1, null, 1, SessionStatus.Active, DateTimeOffset.UtcNow, null, null, []));

        var artifactVersion = new ArtifactVersion
        {
            Id = versionId,
            ArtifactId = 5,
            VersionNo = 1,
            PlainTextContent = "BRD Document Content",
            StructuredContentJson = "{}",
            Artifact = new ArtifactEntity { Id = 5, Title = "BRD OmniMart" }
        };
        _artifactRepoMock.Setup(r => r.GetVersionEntityByIdAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(artifactVersion);

        var rubric = new Rubric
        {
            Id = 1,
            Name = "BABOK Standard Rubric",
            RubricCriteria = new List<RubricCriterion>
            {
                new() { Id = 11, Code = "BABOK-01", Name = "Completeness", Weight = 0.5m, MaxScore = 100 },
                new() { Id = 12, Code = "BABOK-02", Name = "Traceability", Weight = 0.5m, MaxScore = 100 }
            }
        };
        _assessmentRepoMock.Setup(r => r.GetRubricByScenarioVersionAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rubric);
        _analysisRepoMock.Setup(r => r.GetItemsBySessionAsync(sessionId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnalysisItemDto>());

        var runtimeResponse = new AssessmentRuntimeResponse(
            SubmissionId: submissionId,
            OverallScore: 85.0,
            Passed: true,
            CriterionScores:
            [
                new AssessmentCriterionScoreDto("BABOK-01", 80.0, 100.0, "Good scope"),
                new AssessmentCriterionScoreDto("BABOK-02", 90.0, 100.0, "Solid evidence")
            ],
            Strengths: ["Clear business context"],
            Weaknesses: ["Missing non-functional requirements"],
            CriticalErrors: [],
            ExpertReviewAvailable: true
        );
        _aiRuntimeClientMock.Setup(a => a.EvaluateAssessmentAsync(It.IsAny<AssessmentRuntimeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(runtimeResponse);

        var expectedFeedbackDto = new AIPracticeFeedbackDto(
            Id: 1,
            SubmissionId: submissionId,
            Status: "Completed",
            SuggestedScore: 85.0m,
            Confidence: 0.9m,
            OverallFeedback: "Đạt yêu cầu sơ bộ (Điểm: 85.0/100)",
            Strengths: ["Clear business context"],
            Weaknesses: ["Missing non-functional requirements"],
            CriticalErrors: [],
            CriterionScores: [],
            StartedAt: DateTimeOffset.UtcNow,
            CompletedAt: DateTimeOffset.UtcNow
        );

        _assessmentRepoMock.Setup(r => r.SavePracticeFeedbackAsync(
            It.IsAny<AIPracticeFeedback>(), It.IsAny<List<AIFeedbackCriterionScore>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFeedbackDto);

        // Act
        var result = await _sutAssessment.EvaluateSubmissionAsync(learnerId, submissionId, "OpenAI", "gpt-4o-mini", "key");

        // Assert
        result.Should().NotBeNull();
        result!.SuggestedScore.Should().Be(85.0m);
        result.OverallFeedback.Should().Contain("85.0");

        _assessmentRepoMock.Verify(r => r.SavePracticeFeedbackAsync(
            It.Is<AIPracticeFeedback>(f => f.SuggestedScore == 85.0m && f.Status == AIFeedbackStatus.Completed),
            It.Is<List<AIFeedbackCriterionScore>>(list => list.Count == 2),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    #endregion

    #region F-ASM-02: Hàng đợi & Tiếp nhận Hồ sơ Chuyên gia (Atomic Lock)

    [Fact]
    public async Task TC_ASM_04_CreateReviewRequestAsync_LearnerRequestsExpertReview_CreatesPendingRequest()
    {
        // Arrange
        const int learnerId = 42;
        const int submissionId = 100;
        const int sessionId = 1;

        _artifactRepoMock.Setup(r => r.GetSubmissionDtoByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmissionDto(submissionId, sessionId, 10, 1, SubmissionStatus.Submitted, DateTimeOffset.UtcNow, null));
        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionDto(sessionId, 1, null, 1, SessionStatus.Active, DateTimeOffset.UtcNow, null, null, []));
        _assessmentRepoMock.Setup(r => r.GetOpenReviewRequestBySubmissionIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReviewRequest?)null);
        _assessmentRepoMock.Setup(r => r.GetFeedbackDtoBySubmissionIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIPracticeFeedbackDto(1, submissionId, "Completed", 80m, 0.9m, "AI Good", [], [], [], [], DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

        var expectedDto = new ReviewRequestDto(
            Id: 50,
            SubmissionId: submissionId,
            LearnerId: learnerId,
            AssignedReviewerId: null,
            RequestNote: "Please review my inventory requirements",
            Status: "Pending",
            RequestedAt: DateTimeOffset.UtcNow,
            AcceptedAt: null,
            CompletedAt: null
        );

        _assessmentRepoMock.Setup(r => r.CreateReviewRequestAsync(It.IsAny<ReviewRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        // Act
        var result = await _sutAssessment.CreateReviewRequestAsync(learnerId, submissionId, new CreateReviewRequestDto { RequestNote = "Please review my inventory requirements" });

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Pending");
        result.LearnerId.Should().Be(learnerId);

        _assessmentRepoMock.Verify(r => r.CreateReviewRequestAsync(It.Is<ReviewRequest>(r =>
            r.SubmissionId == submissionId &&
            r.LearnerId == learnerId &&
            r.Status == ReviewRequestStatus.Pending
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_ASM_05_AcceptReviewRequestAsync_AtomicConcurrencyLock_OnlyOneExpertCanAccept()
    {
        // Arrange: Pending review request without reviewer
        const int requestId = 50;
        const int expert1Id = 101;
        const int expert2Id = 102;

        var openRequest = new ReviewRequest
        {
            Id = requestId,
            Status = ReviewRequestStatus.Pending,
            AssignedReviewerId = null
        };

        _assessmentRepoMock.Setup(r => r.GetReviewRequestByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openRequest);

        // First expert succeeds
        _assessmentRepoMock.Setup(r => r.TryAcceptReviewRequestAsync(requestId, expert1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Second expert fails (locked)
        _assessmentRepoMock.Setup(r => r.TryAcceptReviewRequestAsync(requestId, expert2Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var firstResult = await _sutAssessment.AcceptReviewRequestAsync(expert1Id, requestId);
        var secondResult = await _sutAssessment.AcceptReviewRequestAsync(expert2Id, requestId);

        // Assert
        firstResult.Should().BeTrue();
        secondResult.Should().BeFalse(); // Conflict / lock prevented double assignment!
    }

    [Fact]
    public async Task TC_ASM_06_DossierPrivacy_UnassignedReviewerAccessesDossier_ThrowsUnauthorizedAccessException()
    {
        // Arrange: Request is assigned to Reviewer 101
        const int requestId = 50;
        const int assignedReviewerId = 101;
        const int intruderReviewerId = 999;

        var assignedRequest = new ReviewRequest
        {
            Id = requestId,
            SubmissionId = 100,
            AssignedReviewerId = assignedReviewerId,
            Status = ReviewRequestStatus.Accepted
        };

        _assessmentRepoMock.Setup(r => r.GetReviewRequestByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(assignedRequest);

        // Act
        var act = () => _sutAssessment.GetReviewCaseDossierAsync(intruderReviewerId, requestId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*chưa được phân công hoặc chưa tiếp nhận*");
    }

    #endregion

    #region F-ASM-03: Chấm điểm Rubric & Hiệu chỉnh Điểm cuối cùng (FinalScore)

    [Fact]
    public async Task TC_ASM_07_CompleteReviewAsync_CalculatesFinalWeightedScore_DoesNotOverwriteWithAiSuggestedScore()
    {
        // Arrange
        const int reviewerId = 101;
        const int requestId = 50;
        const int submissionId = 100;

        var reviewReq = new ReviewRequest
        {
            Id = requestId,
            SubmissionId = submissionId,
            LearnerId = 42,
            AssignedReviewerId = reviewerId,
            Status = ReviewRequestStatus.Accepted
        };
        _assessmentRepoMock.Setup(r => r.GetReviewRequestByIdAsync(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviewReq);

        _artifactRepoMock.Setup(r => r.GetSubmissionDtoByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmissionDto(submissionId, 1, 10, 1, SubmissionStatus.Submitted, DateTimeOffset.UtcNow, null));
        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(42, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionDto(1, 1, null, 1, SessionStatus.Active, DateTimeOffset.UtcNow, null, null, []));

        // Rubric: Criterion 1 (weight 0.4, max 100), Criterion 2 (weight 0.6, max 100)
        var rubric = new Rubric
        {
            Id = 1,
            RubricCriteria = new List<RubricCriterion>
            {
                new() { Id = 1, Name = "Completeness", Weight = 0.4m, MaxScore = 100 },
                new() { Id = 2, Name = "Traceability", Weight = 0.6m, MaxScore = 100 }
            }
        };
        _assessmentRepoMock.Setup(r => r.GetRubricByScenarioVersionAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rubric);

        var completeReq = new CompleteReviewRequest
        {
            OverallFeedback = "Strong analysis, well-traced requirements.",
            CriterionScores = new List<CriterionScoreInputRequest>
            {
                new() { RubricCriterionId = 1, Score = 90m, Comment = "Comprehensive" },
                new() { RubricCriterionId = 2, Score = 80m, Comment = "Good traceability" }
            }
        };

        var expectedHumanReviewDto = new HumanReviewDto(
            Id: 10,
            SubmissionId: submissionId,
            RubricId: 1,
            ReviewerId: reviewerId,
            FinalScore: 84.00m,
            OverallFeedback: completeReq.OverallFeedback,
            CriterionScores: [],
            ReviewedAt: DateTimeOffset.UtcNow
        );

        _assessmentRepoMock.Setup(r => r.SaveHumanReviewAsync(
            It.IsAny<HumanReview>(), It.IsAny<List<HumanReviewCriterionScore>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHumanReviewDto);

        // Act
        var result = await _sutAssessment.CompleteReviewAsync(reviewerId, requestId, completeReq);

        // Assert
        result.Should().NotBeNull();
        result!.FinalScore.Should().Be(84.00m);

        _assessmentRepoMock.Verify(r => r.SaveHumanReviewAsync(
            It.Is<HumanReview>(hr => hr.FinalScore == 84.00m && hr.ReviewerId == reviewerId),
            It.Is<List<HumanReviewCriterionScore>>(scores => scores.Count == 2),
            It.IsAny<CancellationToken>()
        ), Times.Once);

        _artifactRepoMock.Verify(r => r.FinalizeSubmissionAsync(submissionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_ASM_08_CompleteReviewAsync_ScoreOutOfBoundsOrInvalidCriterion_ThrowsArgumentException()
    {
        // Arrange
        const int reviewerId = 101;
        const int requestId = 50;

        var reviewReq = new ReviewRequest { Id = requestId, SubmissionId = 100, AssignedReviewerId = reviewerId, Status = ReviewRequestStatus.Accepted };
        _assessmentRepoMock.Setup(r => r.GetReviewRequestByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(reviewReq);
        _artifactRepoMock.Setup(r => r.GetSubmissionDtoByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(new SubmissionDto(100, 1, 10, 1, SubmissionStatus.Submitted, DateTimeOffset.UtcNow, null));
        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(It.IsAny<int>(), 1, It.IsAny<CancellationToken>())).ReturnsAsync(new SessionDto(1, 1, null, 1, SessionStatus.Active, DateTimeOffset.UtcNow, null, null, []));

        var rubric = new Rubric
        {
            Id = 1,
            RubricCriteria = new List<RubricCriterion>
            {
                new() { Id = 1, Name = "Scope", MaxScore = 100, Weight = 1.0m }
            }
        };
        _assessmentRepoMock.Setup(r => r.GetRubricByScenarioVersionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rubric);

        // Act & Assert: Score > MaxScore (150 > 100)
        var invalidReq = new CompleteReviewRequest
        {
            OverallFeedback = "Invalid",
            CriterionScores = [new() { RubricCriterionId = 1, Score = 150m, Comment = "Too high" }]
        };

        var act = () => _sutAssessment.CompleteReviewAsync(reviewerId, requestId, invalidReq);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*phải từ 0 đến 100*");
    }

    [Fact]
    public async Task TC_ASM_09_GetFinalReportAsync_AggregatesAiFeedbackAndHumanExpertEvaluation()
    {
        // Arrange
        const int submissionId = 100;
        const int learnerId = 42;

        _artifactRepoMock.Setup(r => r.GetSubmissionDtoByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmissionDto(submissionId, 1, 10, 1, SubmissionStatus.Finalized, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));
        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(learnerId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionDto(1, 1, null, 1, SessionStatus.Completed, DateTimeOffset.UtcNow, null, null, []));

        var aiFeedback = new AIPracticeFeedbackDto(1, submissionId, "Completed", 80m, 0.9m, "AI Good", [], [], [], [], DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        var humanReview = new HumanReviewDto(2, submissionId, 1, 101, 88m, "Expert: Excellent depth", [], DateTimeOffset.UtcNow);

        _assessmentRepoMock.Setup(r => r.GetFeedbackDtoBySubmissionIdAsync(submissionId, It.IsAny<CancellationToken>())).ReturnsAsync(aiFeedback);
        _assessmentRepoMock.Setup(r => r.GetHumanReviewDtoBySubmissionIdAsync(submissionId, It.IsAny<CancellationToken>())).ReturnsAsync(humanReview);

        // Act
        var report = await _sutAssessment.GetFinalReportAsync(learnerId, "LEARNER", submissionId);

        // Assert
        report.Should().NotBeNull();
        report!.AIFeedback.Should().NotBeNull();
        report.AIFeedback!.SuggestedScore.Should().Be(80m);
        report.ExpertReview.Should().NotBeNull();
        report.ExpertReview!.FinalScore.Should().Be(88m);
        report.ExpertReview.OverallFeedback.Should().Contain("Excellent depth");
    }

    #endregion

    #region End-to-End Workflow Tests

    [Fact]
    public async Task TC_ASM_E2E_01_FullAssessmentWorkflow_Submit_AiEvaluate_RequestReview_ExpertAccepts_ScoresRubric_Finalize()
    {
        // Arrange
        const int learnerId = 42;
        const int expertId = 101;
        const int sessionId = 1;
        const int submissionId = 500;
        const int versionId = 10;
        const int requestId = 200;

        // Step 1: Submit
        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionDto(sessionId, 1, null, 1, SessionStatus.Active, DateTimeOffset.UtcNow, null, null, []));
        _artifactRepoMock.Setup(r => r.GetVersionEntityByIdAsync(versionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ArtifactVersion { Id = versionId, ArtifactId = 1, Artifact = new ArtifactEntity { Id = 1, SessionId = sessionId } });
        _artifactRepoMock.Setup(r => r.GetNextSubmissionAttemptNoAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _artifactRepoMock.Setup(r => r.CreateSubmissionAsync(It.IsAny<Submission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmissionDto(submissionId, sessionId, versionId, 1, SubmissionStatus.Submitted, DateTimeOffset.UtcNow, null));

        var subResult = await _sutArtifact.SubmitArtifactVersionAsync(learnerId, sessionId, new CreateSubmissionRequest { ArtifactVersionId = versionId });
        subResult!.Status.Should().Be(SubmissionStatus.Submitted);

        // Step 2: AI Evaluation
        var rubric = new Rubric { Id = 1, RubricCriteria = [new() { Id = 1, Code = "C1", Weight = 1.0m, MaxScore = 100 }] };
        _artifactRepoMock.Setup(r => r.GetSubmissionDtoByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(subResult);
        _assessmentRepoMock.Setup(r => r.GetRubricByScenarioVersionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rubric);
        _analysisRepoMock.Setup(r => r.GetItemsBySessionAsync(sessionId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AnalysisItemDto>());
        _aiRuntimeClientMock.Setup(a => a.EvaluateAssessmentAsync(It.IsAny<AssessmentRuntimeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AssessmentRuntimeResponse(submissionId, 82.0, true, [new AssessmentCriterionScoreDto("C1", 82.0, 100.0, "Good")], [], [], [], true));
        _assessmentRepoMock.Setup(r => r.SavePracticeFeedbackAsync(It.IsAny<AIPracticeFeedback>(), It.IsAny<List<AIFeedbackCriterionScore>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AIPracticeFeedbackDto(1, submissionId, "Completed", 82m, 0.9m, "Good", [], [], [], [], DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

        var aiFeedback = await _sutAssessment.EvaluateSubmissionAsync(learnerId, submissionId, "OpenAI", null, null);
        aiFeedback!.SuggestedScore.Should().Be(82m);

        // Step 3: Request Expert Review
        _assessmentRepoMock.Setup(r => r.GetOpenReviewRequestBySubmissionIdAsync(submissionId, It.IsAny<CancellationToken>())).ReturnsAsync((ReviewRequest?)null);
        _assessmentRepoMock.Setup(r => r.GetFeedbackDtoBySubmissionIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(aiFeedback);
        _assessmentRepoMock.Setup(r => r.CreateReviewRequestAsync(It.IsAny<ReviewRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReviewRequestDto(requestId, submissionId, learnerId, null, "Please review", "Pending", DateTimeOffset.UtcNow, null, null));

        var reviewRequest = await _sutAssessment.CreateReviewRequestAsync(learnerId, submissionId, new CreateReviewRequestDto { RequestNote = "Please review" });
        reviewRequest!.Status.Should().Be("Pending");

        // Step 4: Expert Accepts
        var pendingEntity = new ReviewRequest { Id = requestId, SubmissionId = submissionId, LearnerId = learnerId, Status = ReviewRequestStatus.Pending };
        _assessmentRepoMock.Setup(r => r.GetReviewRequestByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(pendingEntity);
        _assessmentRepoMock.Setup(r => r.TryAcceptReviewRequestAsync(requestId, expertId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var accepted = await _sutAssessment.AcceptReviewRequestAsync(expertId, requestId);
        accepted.Should().BeTrue();

        // Step 5: Expert Scores & Completes
        pendingEntity.Status = ReviewRequestStatus.Accepted;
        pendingEntity.AssignedReviewerId = expertId;
        _assessmentRepoMock.Setup(r => r.SaveHumanReviewAsync(It.IsAny<HumanReview>(), It.IsAny<List<HumanReviewCriterionScore>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HumanReviewDto(1, submissionId, 1, expertId, 92m, "Expert Final Feedback", [], DateTimeOffset.UtcNow));

        var completeResult = await _sutAssessment.CompleteReviewAsync(expertId, requestId, new CompleteReviewRequest
        {
            OverallFeedback = "Expert Final Feedback",
            CriterionScores = [new() { RubricCriterionId = 1, Score = 92m, Comment = "Superb" }]
        });
        completeResult!.FinalScore.Should().Be(92m);
    }

    [Fact]
    public async Task TC_ASM_E2E_02_SecurityAndConcurrency_ConcurrentAcceptConflict_DossierIsolationEnforced()
    {
        // Concurrency test: Expert A accepts first, Expert B gets false
        const int requestId = 10;
        var req = new ReviewRequest { Id = requestId, Status = ReviewRequestStatus.Pending, AssignedReviewerId = null };
        _assessmentRepoMock.Setup(r => r.GetReviewRequestByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(req);

        _assessmentRepoMock.Setup(r => r.TryAcceptReviewRequestAsync(requestId, 101, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _assessmentRepoMock.Setup(r => r.TryAcceptReviewRequestAsync(requestId, 102, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var expert1Accepted = await _sutAssessment.AcceptReviewRequestAsync(101, requestId);
        var expert2Accepted = await _sutAssessment.AcceptReviewRequestAsync(102, requestId);

        expert1Accepted.Should().BeTrue();
        expert2Accepted.Should().BeFalse();

        // Privacy: Expert 102 tries to inspect dossier of assigned review 101
        req.AssignedReviewerId = 101;
        var actUnauthorized = () => _sutAssessment.GetReviewCaseDossierAsync(102, requestId);
        await actUnauthorized.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public async Task TC_ASM_ST_01_SubmissionStateTransition_DraftToSubmitted()
    {
        // Arrange
        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionDto(1, 1, null, 1, SessionStatus.Active, DateTimeOffset.UtcNow, null, null, []));
        _artifactRepoMock.Setup(r => r.GetVersionEntityByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ArtifactVersion { Id = 1, Artifact = new ArtifactEntity { SessionId = 1 } });
        _artifactRepoMock.Setup(r => r.CreateSubmissionAsync(It.IsAny<Submission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmissionDto(1, 1, 1, 1, SubmissionStatus.Submitted, DateTimeOffset.UtcNow, null));

        // Act
        var submission = await _sutArtifact.SubmitArtifactVersionAsync(1, 1, new CreateSubmissionRequest { ArtifactVersionId = 1 });

        // Assert
        submission!.Status.Should().Be(SubmissionStatus.Submitted);
    }

    [Fact]
    public async Task TC_ASM_ST_02_ReviewRequestStateTransition_PendingToAccepted()
    {
        // Arrange
        const int requestId = 10;
        const int expertId = 99;
        var req = new ReviewRequest { Id = requestId, Status = ReviewRequestStatus.Pending };
        _assessmentRepoMock.Setup(r => r.GetReviewRequestByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(req);
        _assessmentRepoMock.Setup(r => r.TryAcceptReviewRequestAsync(requestId, expertId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _sutAssessment.AcceptReviewRequestAsync(expertId, requestId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TC_ASM_ST_03_ReviewRequestStateTransition_AcceptedToCompleted_AndSubmissionToFinalized()
    {
        // Arrange
        const int requestId = 10;
        const int reviewerId = 99;
        const int submissionId = 5;

        var req = new ReviewRequest { Id = requestId, SubmissionId = submissionId, AssignedReviewerId = reviewerId, Status = ReviewRequestStatus.Accepted };
        _assessmentRepoMock.Setup(r => r.GetReviewRequestByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(req);
        _artifactRepoMock.Setup(r => r.GetSubmissionDtoByIdAsync(submissionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmissionDto(submissionId, 1, 1, 1, SubmissionStatus.Submitted, DateTimeOffset.UtcNow, null));
        _simSessionServiceMock.Setup(s => s.GetSessionByIdAsync(It.IsAny<int>(), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SessionDto(1, 1, null, 1, SessionStatus.Active, DateTimeOffset.UtcNow, null, null, []));

        var rubric = new Rubric { Id = 1, RubricCriteria = [new() { Id = 1, MaxScore = 100, Weight = 1.0m }] };
        _assessmentRepoMock.Setup(r => r.GetRubricByScenarioVersionAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rubric);
        _assessmentRepoMock.Setup(r => r.SaveHumanReviewAsync(It.IsAny<HumanReview>(), It.IsAny<List<HumanReviewCriterionScore>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HumanReviewDto(1, submissionId, 1, reviewerId, 85m, "Done", [], DateTimeOffset.UtcNow));

        // Act
        await _sutAssessment.CompleteReviewAsync(reviewerId, requestId, new CompleteReviewRequest
        {
            OverallFeedback = "Done",
            CriterionScores = [new() { RubricCriterionId = 1, Score = 85m, Comment = "Good" }]
        });

        // Assert
        _assessmentRepoMock.Verify(r => r.CompleteReviewRequestAsync(requestId, It.IsAny<CancellationToken>()), Times.Once);
        _artifactRepoMock.Verify(r => r.FinalizeSubmissionAsync(submissionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_ASM_ST_04_InvalidStateTransition_CompleteReviewWhenNotAccepted_ThrowsInvalidOperationException()
    {
        // Arrange: Request is still Pending
        const int requestId = 10;
        const int reviewerId = 99;

        var req = new ReviewRequest { Id = requestId, Status = ReviewRequestStatus.Pending, AssignedReviewerId = reviewerId };
        _assessmentRepoMock.Setup(r => r.GetReviewRequestByIdAsync(requestId, It.IsAny<CancellationToken>())).ReturnsAsync(req);

        // Act
        var act = () => _sutAssessment.CompleteReviewAsync(reviewerId, requestId, new CompleteReviewRequest { OverallFeedback = "Done", CriterionScores = [] });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*không thể hoàn thành*");
    }

    #endregion
}
