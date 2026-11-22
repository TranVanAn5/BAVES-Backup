using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Requests;

namespace BAVES.Api.Modules.Scenario.Services.Interfaces;

public interface IAiRuntimeClient
{
    Task<ValidateAiConnectionDto> ValidateConnectionAsync(
        ValidateAiConnectionRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Hỏi AI Runtime trả lời thay cho stakeholder.
    /// Gọi POST /v1/stakeholder/respond với header X-Internal-Token.
    /// </summary>
    /// <param name="request">Payload đầy đủ theo contract của AI Runtime.</param>
    /// <param name="cancellationToken">Token huỷ.</param>
    /// <returns>Câu trả lời + các knowledge item đã dùng + cập nhật trạng thái agent.</returns>
    /// <exception cref="Shared.Exceptions.AiRuntimeException">
    /// Khi runtime lỗi (API key sai, bị giới hạn, timeout, hoặc trả về không hợp lệ).
    /// </exception>
    Task<StakeholderAgentResponse> RespondAsync(
        StakeholderAgentRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Hỏi AI Runtime nhận xét kỹ năng khơi gợi yêu cầu của learner.
    /// Gọi POST /v1/coach/feedback với header X-Internal-Token.
    /// </summary>
    Task<CoachFeedbackResponse> GetCoachFeedbackAsync(
        CoachFeedbackRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gửi bài nộp sang AI Assessor Runtime để chấm điểm theo Rubric BABOK.
    /// Gọi POST /v1/assessment/evaluate với header X-Internal-Token.
    /// </summary>
    Task<AssessmentRuntimeResponse> EvaluateAssessmentAsync(
        AssessmentRuntimeRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gửi một mục phân tích (AnalysisItem) kèm bằng chứng sang AI Stakeholder để xác nhận.
    /// Gọi POST /v1/analysis/confirm với header X-Internal-Token.
    /// </summary>
    Task<AnalysisConfirmRuntimeResponse> ConfirmAnalysisItemAsync(
        AnalysisConfirmRuntimeRequest request,
        CancellationToken cancellationToken = default
    );

    Task<SprintReviewRuntimeResponse> ReviewSprintAsync(
        object request,
        CancellationToken cancellationToken = default
    );
}
