using System.ComponentModel.DataAnnotations;
using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Planning.Models.Enums;

namespace BAVES.Api.Modules.Analysis.Models.Requests;

/// <summary>
/// Body cho PATCH /api/sessions/{sessionId}/analysis-items/{itemId}.
/// Cập nhật một phần (partial update) cho một mục phân tích của Learner.
///
/// field KHÔNG gửi (null) → giữ nguyên giá trị cũ.
/// `MetadataJson` gửi chuỗi rỗng "" → xoá metadata (đặt về null).
/// `Status` KHÔNG cho đặt `Archived` (3): muốn archive phải dùng
///  DELETE /api/sessions/{sessionId}/analysis-items/{itemId} để đi đúng luồng.
///
/// Các field KHÔNG cho sửa qua PATCH:
/// `Code`: do server sinh, unique theo (SessionId, Code).
/// `ItemType`: đổi loại sẽ làm sai tiền tố của `Code` (vd REQ-001 → FND-001).
/// `SessionId`: thuộc session khác sẽ vi phạm ràng buộc BR-11.
/// `CreatedByUserId`, `CreatedAt`: trường audit, lấy từ JWT/hệ thống.
/// </summary>
public sealed record UpdateAnalysisItemRequest
{
    /// <summary>
    /// ID mục phân tích cha (cùng SessionId). Gửi 0 hoặc số âm để gỡ liên kết cha (đặt về null).
    /// </summary>
    public int? ParentAnalysisItemId { get; init; }

    /// <summary>
    /// Tiêu đề mới của mục phân tích. Tùy chọn, tối đa 200 ký tự.
    /// </summary>
    [MaxLength(200)]
    public string? Title { get; init; }

    /// <summary>
    /// Mô tả mới của mục phân tích. Tùy chọn.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Mức ưu tiên mới: Low, Medium, High, Critical. Tùy chọn.
    /// </summary>
    public PriorityLevel? Priority { get; init; }

    /// <summary>
    /// Trạng thái mới: Draft (0), Confirmed (1), Resolved (2).
    /// Không cho đặt Archived (3) — dùng DELETE để archive.
    /// </summary>
    public AnalysisItemStatus? Status { get; init; }

    /// <summary>
    /// Metadata bổ sung dạng JSON (object hoặc array). Tùy chọn.
    /// Gửi chuỗi rỗng "" để xoá metadata hiện có.
    /// </summary>
    public string? MetadataJson { get; init; }
}
