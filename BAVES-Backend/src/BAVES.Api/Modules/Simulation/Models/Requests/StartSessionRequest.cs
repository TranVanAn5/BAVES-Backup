namespace BAVES.Api.Modules.Simulation.Models.Requests;

/// <summary>
/// Request body cho POST /api/sessions.
/// Learner gửi kèm ScenarioVersionId muốn bắt đầu luyện tập (và tùy chọn AssignmentId nếu làm theo bài tập).
/// </summary>
public sealed record StartSessionRequest
{
    /// <summary>
    /// ID của ScenarioVersion đã được publish mà Learner muốn mở phiên luyện tập.
    /// </summary>
    public int ScenarioVersionId { get; init; }

    /// <summary>
    /// ID của bài tập (Assignment) nếu phiên mô phỏng này được khởi tạo từ một bài tập được giao.
    /// </summary>
    public int? AssignmentId { get; init; }
}
