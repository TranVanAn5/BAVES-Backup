using BAVES.Api.Modules.Analysis.Models.DTOs;
using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Analysis.Models.Requests;

namespace BAVES.Api.Modules.Analysis.Services.Interfaces;

/// <summary>
/// Nghiệp vụ Analysis Item: các mục phân tích (finding, requirement, business rule,
/// constraint, risk, open question, conflict, assumption) mà Learner tạo trong
/// quá trình phỏng vấn stakeholder.
/// </summary>
public interface IAnalysisItemService
{
    /// <summary>
    /// Lấy danh sách Analysis Item của một session thuộc về Learner.
    /// Trả null nếu session không tồn tại hoặc không thuộc Learner này
    /// (Controller sẽ map thành 404 để không lộ sự tồn tại của session của người khác).
    /// Nếu session hợp lệ nhưng chưa có item nào → trả danh sách rỗng.
    /// </summary>
    Task<IReadOnlyList<AnalysisItemDto>?> GetItemsBySessionAsync(
        int learnerId,
        int sessionId,
        AnalysisItemType? itemType,
        AnalysisItemStatus? status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo mới một Analysis Item trong session của Learner.
    /// - Mã <c>Code</c> do server tự sinh theo mẫu PREFIX-NNN, duy nhất trong session.
    /// - Item mới luôn khởi tạo ở trạng thái <see cref="AnalysisItemStatus.Draft"/>.
    /// - Chỉ tạo được khi session còn <c>Active</c>; session đã kết thúc → ConflictException.
    /// Trả null nếu session không tồn tại hoặc không thuộc Learner này (Controller map 404).
    /// </summary>
    Task<AnalysisItemDto?> CreateItemAsync(
        int learnerId,
        int sessionId,
        CreateAnalysisItemRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật một phần (partial update) một Analysis Item trong session của Learner.
    /// - Field không gửi trong request → giữ nguyên giá trị cũ.
    /// - Không cho sửa <c>Code</c>, <c>ItemType</c>, <c>SessionId</c> và các trường audit.
    /// - Chỉ được sửa khi session còn <c>Active</c>; session đã kết thúc → ConflictException.
    /// Trả null nếu session không tồn tại, không thuộc Learner này,
    /// hoặc item không tồn tại trong session đó (Controller map 404).
    /// </summary>
    Task<AnalysisItemDto?> UpdateItemAsync(
        int learnerId,
        int sessionId,
        int itemId,
        UpdateAnalysisItemRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Xoá mềm (soft delete) một Analysis Item trong session của Learner.
    /// - Chuyển trạng thái sang <see cref="AnalysisItemStatus.Archived"/>, giữ nguyên bản ghi.
    /// - Chỉ được xoá khi session còn <c>Active</c>; session đã kết thúc → ConflictException.
    /// - Item đã ở trạng thái <c>Archived</c> → ConflictException (không xoá lặp).
    /// Trả null nếu session không tồn tại, không thuộc Learner này,
    /// hoặc item không tồn tại trong session đó (Controller map 404).
    /// </summary>
    Task<AnalysisItemDto?> ArchiveItemAsync(
        int learnerId,
        int sessionId,
        int itemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách tất cả liên kết bằng chứng của một Analysis Item kèm nội dung tin nhắn.
    /// Trả null nếu item không tồn tại hoặc không thuộc Learner này (Controller map 404).
    /// </summary>
    Task<IReadOnlyList<EvidenceLinkDetailDto>?> GetEvidenceLinksAsync(
        int learnerId,
        int itemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gắn một tin nhắn trong transcript làm bằng chứng cho một Analysis Item.
    /// - Tin nhắn phải thuộc đúng session chứa mục phân tích.
    /// - Một cặp (MessageId, AnalysisItemId, RelationshipType) chỉ tồn tại một lần.
    /// - Chỉ được gắn khi session còn <c>Active</c> và item chưa bị archive.
    /// Trả null nếu item không tồn tại hoặc không thuộc Learner này (Controller map 404).
    /// </summary>
    Task<EvidenceLinkDto?> AddEvidenceAsync(
        int learnerId,
        int itemId,
        CreateEvidenceLinkRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gỡ một liên kết bằng chứng khỏi một Analysis Item.
    /// - Xoá cứng bản ghi liên kết (bảng con, không mang dữ liệu học tập riêng).
    /// - Chỉ được gỡ khi session còn <c>Active</c> và item chưa bị archive.
    /// Trả null nếu item hoặc link không tồn tại, hoặc item không thuộc Learner này
    /// (Controller map 404).
    /// </summary>
    Task<EvidenceLinkDto?> RemoveEvidenceAsync(
        int learnerId,
        int itemId,
        int linkId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi một Analysis Item kèm bằng chứng phỏng vấn tới một AI Stakeholder trong phiên
    /// để xác nhận (Confirmed / Disputed / InsufficientInformation).
    /// Nếu kết quả là Confirmed, tự động chuyển trạng thái AnalysisItem sang Confirmed
    /// và lưu lịch sử xác nhận vào MetadataJson.
    /// Trả null nếu item không tồn tại hoặc không thuộc Learner này (Controller map 404).
    /// </summary>
    Task<AnalysisConfirmationResultDto?> ConfirmItemAsync(
        int learnerId,
        int itemId,
        ConfirmAnalysisItemRequest request,
        string? headerProvider,
        string? headerModel,
        string? headerApiKey,
        CancellationToken cancellationToken = default);
}
