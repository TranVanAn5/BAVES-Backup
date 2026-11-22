using BAVES.Api.Modules.Assessment.Models.Enums;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Models.Entities;

namespace BAVES.Api.Modules.Assessment.Repositories.Interfaces;

public interface IAssessmentRepository
{
    // Rubric
    Task<Rubric?> GetRubricByScenarioVersionAsync(
        int scenarioVersionId,
        CancellationToken cancellationToken = default);

    Task<Rubric?> GetDefaultRubricAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BabokTaskDto>> GetBabokTasksAsync(
        CancellationToken cancellationToken = default);

    Task<bool> AllBabokTasksExistAsync(
        IReadOnlyCollection<int> babokTaskIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RubricDetailDto>> GetRubricsAsync(
        int? scenarioVersionId,
        RubricStatus? status,
        CancellationToken cancellationToken = default);

    Task<RubricDetailDto?> GetRubricDetailByIdAsync(
        int rubricId,
        CancellationToken cancellationToken = default);

    Task<Rubric?> GetTrackedRubricByIdAsync(
        int rubricId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextRubricVersionAsync(
        int scenarioVersionId,
        CancellationToken cancellationToken = default);

    Task<RubricDetailDto> CreateRubricAsync(
        Rubric rubric,
        CancellationToken cancellationToken = default);

    Task<RubricDetailDto> UpdateRubricAsync(
        Rubric rubric,
        List<RubricCriterion> newCriteria,
        CancellationToken cancellationToken = default);

    Task<RubricDetailDto?> PublishRubricAsync(
        int rubricId,
        CancellationToken cancellationToken = default);

    Task<Rubric?> GetRubricByIdAsync(
        int rubricId,
        CancellationToken cancellationToken = default);

    // AI Feedback
    Task<AIPracticeFeedbackDto?> GetFeedbackDtoBySubmissionIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default);

    Task<AIPracticeFeedback?> GetFeedbackEntityBySubmissionIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default);

    Task<AIPracticeFeedbackDto> SavePracticeFeedbackAsync(
        AIPracticeFeedback feedback,
        List<AIFeedbackCriterionScore> criterionScores,
        CancellationToken cancellationToken = default);

    // Review Requests
    Task<ReviewRequest?> GetReviewRequestByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ReviewRequest?> GetOpenReviewRequestBySubmissionIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReviewRequestDto>> GetReviewRequestsAsync(
        int? learnerId,
        int? reviewerId,
        bool queueOnly,
        CancellationToken cancellationToken = default);

    Task<ReviewRequestDto> CreateReviewRequestAsync(
        ReviewRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> TryAcceptReviewRequestAsync(
        int requestId,
        int reviewerId,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteReviewRequestAsync(
        int requestId,
        CancellationToken cancellationToken = default);

    Task<bool> RequestRevisionAsync(
        int requestId,
        int reviewerId,
        string reason,
        CancellationToken cancellationToken = default);

    // Human Reviews
    Task<IReadOnlyList<AIPracticeFeedbackDto>> GetFeedbackDtosBySubmissionIdsAsync(
        IReadOnlyCollection<int> submissionIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HumanReviewDto>> GetHumanReviewDtosBySubmissionIdsAsync(
        IReadOnlyCollection<int> submissionIds,
        CancellationToken cancellationToken = default);

    Task<HumanReviewDto?> GetHumanReviewDtoBySubmissionIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default);

    Task<HumanReviewDto> SaveHumanReviewAsync(
        HumanReview review,
        List<HumanReviewCriterionScore> criterionScores,
        CancellationToken cancellationToken = default);
}
