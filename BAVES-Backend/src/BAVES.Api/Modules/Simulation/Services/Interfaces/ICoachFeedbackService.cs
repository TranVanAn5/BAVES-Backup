using BAVES.Api.Modules.Simulation.Models.DTOs;

namespace BAVES.Api.Modules.Simulation.Services.Interfaces;

/// <summary>
/// Nghiệp vụ AI Coach cho một Simulation Session: xin nhận xét về kỹ năng khơi gợi
/// yêu cầu (elicitation) của learner và đọc lại nhận xét đã lưu.
///
/// Tách riêng khỏi <see cref="ISimulationSessionService"/> vì cần dùng BA Plan
/// (module Planning) — nếu gộp vào SimulationSessionService sẽ tạo vòng phụ thuộc
/// DI (Planning đã phụ thuộc Simulation).
/// </summary>
public interface ICoachFeedbackService
{
    /// <summary>
    /// Xin AI Coach nhận xét cho cửa sổ hội thoại hiện tại của session và LƯU kết quả.
    ///
    /// Quy tắc:
    ///   - Session phải tồn tại và thuộc learner (nếu không → 404).
    ///   - Session phải còn Active (nếu không → 409).
    ///   - Session phải có ít nhất một lượt trao đổi (nếu không → 409), vì không có
    ///     nội dung thì coach không thể nhận xét kỹ năng đặt câu hỏi.
    ///   - BA Plan được đính kèm nếu learner đã lập, ngược lại gửi null.
    ///   - Mỗi lần gọi tạo một bản ghi mới (bất biến theo thời điểm).
    /// </summary>
    /// <param name="learnerId">User id lấy từ JWT (sub claim).</param>
    /// <param name="sessionId">Session cần nhận xét.</param>
    /// <param name="aiConnection">Kết nối AI theo BYOK — không lưu DB.</param>
    /// <param name="cancellationToken">Token huỷ.</param>
    /// <returns>Nhận xét vừa sinh và đã lưu.</returns>
    /// <exception cref="Shared.Exceptions.NotFoundException">Session không tồn tại hoặc không thuộc learner.</exception>
    /// <exception cref="Shared.Exceptions.ConflictException">Session không còn Active hoặc chưa có hội thoại.</exception>
    /// <exception cref="Shared.Exceptions.AiRuntimeException">AI Runtime lỗi (key sai, rate limit, timeout).</exception>
    Task<CoachFeedbackDto> RequestCoachFeedbackAsync(
        int learnerId,
        int sessionId,
        AgentAiConnection aiConnection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Đọc bản nhận xét MỚI NHẤT đã lưu của session. KHÔNG gọi LLM.
    /// Trả null nếu session không tồn tại/không thuộc learner, hoặc chưa từng có
    /// nhận xét nào (controller map 404).
    /// </summary>
    Task<CoachFeedbackDto?> GetLatestCoachFeedbackAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default);
}
