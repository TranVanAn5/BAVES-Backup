using BAVES.Api.Modules.Analysis.Models.DTOs;
using BAVES.Api.Modules.Analysis.Models.Entities;
using BAVES.Api.Modules.Analysis.Models.Enums;

namespace BAVES.Api.Modules.Analysis.Repositories.Interfaces;

/// <summary>
/// Truy vấn dữ liệu Analysis từ bảng [analysis].[AnalysisItems].
/// </summary>
public interface IAnalysisItemRepository
{
    /// <summary>
    /// Lấy danh sách Analysis Item của một session.
    /// Sắp xếp theo CreatedAt rồi Id để thứ tự ổn định.
    /// </summary>
    Task<IReadOnlyList<AnalysisItemDto>> GetItemsBySessionAsync(
        int sessionId,
        AnalysisItemType? itemType,
        AnalysisItemStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Đếm số Analysis Item của một session theo loại.
    /// Dùng làm số thứ tự cơ sở để sinh mã Code (vd: FND-001, FND-002, ...).
    /// </summary>
    Task<int> CountItemsByTypeAsync(
        int sessionId,
        AnalysisItemType itemType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra mã Code đã tồn tại trong session hay chưa.
    /// Ràng buộc DB: unique (SessionId, Code).
    /// </summary>
    Task<bool> CodeExistsAsync(
        int sessionId,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy một AnalysisItem theo Id, giới hạn trong một session cụ thể.
    /// Trả null nếu item không tồn tại hoặc không thuộc session đó.
    /// Dùng cho PATCH / DELETE: xác thực item có thực sự nằm trong session đang xử lý.
    /// Không dùng AsNoTracking vì cần theo dõi entity để update / soft-delete.
    /// </summary>
    Task<AnalysisItem?> GetItemByIdAsync(
        int sessionId,
        int itemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy một AnalysisItem theo Id mà không ràng buộc session.
    /// Dùng cho các route phẳng (vd /api/analysis-items/{itemId}/evidence) vốn
    /// chưa biết sessionId từ đầu — service sẽ lấy SessionId từ item rồi mới
    /// kiểm tra quyền sở hữu qua session.
    /// Không dùng AsNoTracking vì cần theo dõi entity để thay đổi.
    /// </summary>
    Task<AnalysisItem?> GetItemEntityByIdAsync(
        int itemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một AnalysisItem. Id được sinh bởi database sau khi SaveChanges.
    /// </summary>
    void AddItem(AnalysisItem item);

    /// <summary>
    /// Xoá mềm (soft delete) một AnalysisItem: đổi Status sang Archived.
    /// Giữ nguyên bản ghi và các quan hệ liên kết (Relation / EvidenceLink)
    /// để bảo toàn lịch sử học tập và dữ liệu chấm điểm.
    /// </summary>
    void ArchiveItem(AnalysisItem item);

    /// <summary>
    /// Lấy danh sách tất cả liên kết bằng chứng của một AnalysisItem, sắp theo CreatedAt tăng dần.
    /// </summary>
    Task<IReadOnlyList<MessageEvidenceLink>> GetEvidenceLinksByItemIdAsync(
        int analysisItemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy một liên kết bằng chứng theo Id, giới hạn trong một AnalysisItem cụ thể.
    /// Trả null nếu link không tồn tại hoặc không thuộc item đó.
    /// </summary>
    Task<MessageEvidenceLink?> GetEvidenceLinkAsync(
        int analysisItemId,
        int linkId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra một liên kết bằng chứng đã tồn tại hay chưa.
    /// Ràng buộc DB: unique (MessageId, AnalysisItemId, RelationshipType).
    /// </summary>
    Task<bool> EvidenceLinkExistsAsync(
        int analysisItemId,
        int messageId,
        EvidenceRelationshipType relationshipType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một liên kết bằng chứng vào bảng [analysis].[MessageEvidenceLinks].
    /// Id được sinh bởi database sau khi SaveChanges.
    /// </summary>
    void AddEvidenceLink(MessageEvidenceLink link);

    /// <summary>
    /// Xoá cứng một liên kết bằng chứng.
    /// Đây là bảng con trỏ tới tin nhắn, không mang dữ liệu học tập riêng,
    /// nên gỡ bỏ hoàn toàn thay vì soft delete.
    /// </summary>
    void RemoveEvidenceLink(MessageEvidenceLink link);

    /// <summary>
    /// Lưu toàn bộ thay đổi (insert / update / delete) trong một transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
