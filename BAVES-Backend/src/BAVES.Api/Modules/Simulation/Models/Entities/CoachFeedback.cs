namespace BAVES.Api.Modules.Simulation.Models.Entities;

/// <summary>
/// Một lần AI Coach trả nhận xét về kỹ năng khơi gợi yêu cầu (elicitation) của learner
/// trong một Simulation Session.
///
/// Đây là bản ghi BẤT BIẾN theo thời điểm: mỗi lần learner yêu cầu coach, một dòng mới
/// được thêm vào. GET /coach-feedback đọc dòng mới nhất mà KHÔNG gọi lại LLM, nhờ đó
/// nội dung learner đã thấy không bị thay đổi về sau.
///
/// Bốn nhóm nhận xét được lưu dưới dạng JSON array chuỗi để giữ đúng contract của
/// AI Runtime (app/schemas/coach.py). Đây là dữ liệu hiển thị nguyên khối, không bị
/// truy vấn/lọc theo từng phần tử, nên không cần tách bảng con.
/// </summary>
public class CoachFeedback
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    /// <summary>
    /// User id của learner đã yêu cầu feedback (lấy từ JWT claim "sub").
    /// Lưu lại để truy vết khi cần đối chiếu.
    /// </summary>
    public int CreatedByUserId { get; set; }

    /// <summary>JSON array: nhận xét điểm mạnh trong kỹ năng đặt câu hỏi.</summary>
    public string StrengthsJson { get; set; } = "[]";

    /// <summary>JSON array: khoảng trống thông tin nghiệp vụ chưa khai thác.</summary>
    public string GapsJson { get; set; } = "[]";

    /// <summary>JSON array: khái niệm mập mờ stakeholder đã nhắc nhưng learner chưa hỏi rõ.</summary>
    public string UnclearPointsJson { get; set; } = "[]";

    /// <summary>JSON array: gợi ý phương pháp/kỹ thuật BA cho các lượt phỏng vấn tiếp theo.</summary>
    public string ImprovementHintsJson { get; set; } = "[]";

    /// <summary>
    /// Phiên bản prompt của agent coach đã sinh ra feedback này
    /// (ví dụ "coach@1.0.0"). Cần để truy vết khi prompt thay đổi.
    /// </summary>
    public string PromptVersion { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public SimulationSession Session { get; set; } = null!;
}
