using BAVES.Api.Modules.Planning.Models.DTOs;
using BAVES.Api.Modules.Planning.Models.Entities;

namespace BAVES.Api.Modules.Planning.Repositories.Interfaces;

/// <summary>
/// Truy vấn dữ liệu Planning (BAPlans, PlannedQuestions) từ bảng
/// [planning].[BAPlans] và [planning].[PlannedQuestions].
/// Module Planning là chủ sở hữu duy nhất của các bảng này.
/// </summary>
public interface IPlanningRepository
{
    /// <summary>
    /// Lấy BA Plan của một session kèm danh sách câu hỏi đã sắp theo DisplayOrder.
    /// Trả null nếu session chưa có plan.
    /// </summary>
    Task<BAPlanDto?> GetPlanBySessionAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy entity BAPlan (kèm PlannedQuestions) để theo dõi thay đổi và lưu xuống DB.
    /// Dùng cho thao tác ghi (upsert). Trả null nếu session chưa có plan.
    /// </summary>
    Task<BAPlan?> GetPlanEntityForUpdateAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm mới một BAPlan. Id được sinh bởi database sau khi SaveChanges.
    /// </summary>
    void AddPlan(BAPlan plan);

    /// <summary>
    /// Xóa các PlannedQuestion đã tải (dùng cho thao tác replace danh sách câu hỏi).
    /// </summary>
    void RemoveQuestions(IEnumerable<PlannedQuestion> questions);

    /// <summary>
    /// Lưu toàn bộ thay đổi (insert / update / delete) trong một transaction.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}