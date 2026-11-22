using BAVES.Api.Modules.Assessment.Models.Enums;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Models.Requests;

namespace BAVES.Api.Modules.Assessment.Services.Interfaces;

public interface IAssessmentService
{
    // Rubric & BABOK Tasks Authoring
    Task<IReadOnlyList<BabokTaskDto>> GetBabokTasksAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RubricDetailDto>> GetRubricsAsync(
        int? scenarioVersionId,
        RubricStatus? status,
        bool isInstructorOrAdmin,
        CancellationToken cancellationToken = default);

    Task<RubricDetailDto?> GetRubricDetailByIdAsync(
        int rubricId,
        CancellationToken cancellationToken = default);

    Task<RubricDetailDto> CreateRubricAsync(
        int creatorUserId,
        CreateRubricRequest request,
        CancellationToken cancellationToken = default);

    Task<RubricDetailDto?> UpdateRubricAsync(
        int rubricId,
        UpdateRubricRequest request,
        CancellationToken cancellationToken = default);

    Task<RubricDetailDto?> PublishRubricAsync(
        int rubricId,
        CancellationToken cancellationToken = default);

    // AI Assessment
    Task<AIPracticeFeedbackDto?> GetFeedbackBySubmissionAsync(
        int learnerId,
        int submissionId,
        CancellationToken cancellationToken = default);

    Task<AIPracticeFeedbackDto?> EvaluateSubmissionAsync(
        int learnerId,
        int submissionId,
        string? headerProvider,
        string? headerModel,
        string? headerApiKey,
        CancellationToken cancellationToken = default);

    // Human Expert Review
    Task<ReviewRequestDto?> CreateReviewRequestAsync(
        int learnerId,
        int submissionId,
        CreateReviewRequestDto request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReviewRequestDto>> GetReviewRequestsAsync(
        int userId,
        string userRole,
        string? scope,
        CancellationToken cancellationToken = default);

    Task<bool> AcceptReviewRequestAsync(
        int reviewerId,
        int requestId,
        CancellationToken cancellationToken = default);

    Task<ReviewCaseDossierDto?> GetReviewCaseDossierAsync(
        int reviewerId,
        int requestId,
        CancellationToken cancellationToken = default);

    Task<HumanReviewDto?> CompleteReviewAsync(
        int reviewerId,
        int requestId,
        CompleteReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<ReviewRequestDto?> RequestRevisionAsync(
        int reviewerId,
        int requestId,
        RequestRevisionRequest request,
        CancellationToken cancellationToken = default);

    // Final Report
    Task<FinalSubmissionReportDto?> GetFinalReportAsync(
        int userId,
        string userRole,
        int submissionId,
        CancellationToken cancellationToken = default);

    Task<CompetencyProgressDto> GetCompetencyProgressAsync(
        int learnerId,
        CancellationToken cancellationToken = default);
}
