using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Analysis.Models.Requests;

/// <summary>
/// Request gửi một mục phân tích (AnalysisItem) tới một Stakeholder Agent trong phiên
/// để AI Stakeholder đối chiếu bằng chứng và xác nhận (Confirmed / Disputed / InsufficientInformation).
/// </summary>
public sealed class ConfirmAnalysisItemRequest
{
    /// <summary>
    /// ID của SessionAgent trong phiên mô phỏng cần gửi xác nhận.
    /// Có thể truyền AgentId hoặc PersonaId (ít nhất 1 trong 2 phải lớn hơn 0).
    /// </summary>
    public int? AgentId { get; init; }

    /// <summary>
    /// ID của StakeholderPersona (tùy chọn nếu client truyền PersonaId thay vì AgentId).
    /// </summary>
    public int? PersonaId { get; init; }

    /// <summary>
    /// Ghi chú hoặc tóm tắt bổ sung của học viên khi gửi xác nhận (tùy chọn).
    /// </summary>
    [MaxLength(2000)]
    public string? EvidenceNote { get; init; }
}
