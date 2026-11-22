using System.ComponentModel.DataAnnotations;
using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Planning.Models.Enums;

namespace BAVES.Api.Modules.Analysis.Models.Requests;

/// <summary>
/// Body cho POST /api/sessions/{sessionId}/analysis-items.
/// Learner tạo mới một mục phân tích trong phiên mô phỏng.
///   - `Code` do server tự sinh, không có trong request.
///   - `Status` luôn khởi tạo là Draft, do server quyết định.
///   - `CreatedByUserId` lấy từ JWT, không nhận từ client.
///   - Enum gửi dạng số (0..N) vì API chưa bật JsonStringEnumConverter.
/// </summary>
public sealed record CreateAnalysisItemRequest
{
    /// <summary>
    /// ID mục phân tích cha (cùng SessionId), nếu mục này dẫn xuất từ mục phân tích khác. Tùy chọn.
    /// </summary>
    public int? ParentAnalysisItemId { get; init; }

    /// <summary>
    /// Loại mục phân tích: Finding, Requirement, BusinessRule, Constraint,
    /// Risk, OpenQuestion, Conflict, Assumption. Bắt buộc.
    /// </summary>
    public AnalysisItemType? ItemType { get; init; }

    /// <summary>
    /// Tiêu đề ngắn của mục phân tích. Bắt buộc, tối đa 200 ký tự.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = null!;

    /// <summary>
    /// Mô tả chi tiết nội dung mục phân tích. Bắt buộc.
    /// </summary>
    [Required]
    public string Description { get; init; } = null!;

    /// <summary>
    /// Mức ưu tiên: Low, Medium, High, Critical. Tùy chọn.
    /// </summary>
    public PriorityLevel? Priority { get; init; }

    /// <summary>
    /// Metadata bổ sung dạng JSON (object hoặc array). Tùy chọn.
    /// Server kiểm tra tính hợp lệ của JSON trước khi lưu.
    /// </summary>
    public string? MetadataJson { get; init; }
}
