namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Bộ lọc visibility catalogue sau khi Expert duyệt:
/// Admin xem tất cả published; Instructor: scenario mình tạo (+ tùy chọn IsFreeAccess library);
/// Learner chỉ version được giao (và tùy chọn free-access do Admin gắn).
/// </summary>
public sealed class ScenarioCatalogVisibilityFilter
{
    /// <summary>Admin: không giới hạn owner / assignment.</summary>
    public bool SeeAllPublished { get; init; }

    /// <summary>Instructor: chỉ scenario CreatedByUserId = giá trị này.</summary>
    public int? OwnerUserId { get; init; }

    /// <summary>
    /// Learner: chỉ published version nằm trong tập này.
    /// null = không áp filter assignment; empty = không có bài giao.
    /// </summary>
    public IReadOnlyCollection<int>? AllowedPublishedVersionIds { get; init; }

    /// <summary>Learner: vẫn thấy version Admin gắn IsFreeAccess.</summary>
    public bool IncludeFreeAccessPublished { get; init; }
}
