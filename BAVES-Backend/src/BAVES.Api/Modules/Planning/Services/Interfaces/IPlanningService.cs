using BAVES.Api.Modules.Planning.Models.DTOs;
namespace BAVES.Api.Modules.Planning.Services.Interfaces;

/// <summary>
/// lấy BA Plan của một session.
/// </summary>
public interface IPlanningService
{
    /// <summary>
    /// Lấy BA Plan + danh sách câu hỏi dự kiến của một session.
    /// </summary>
    /// <returns>
    /// BAPlanDto nếu hợp lệ VÀ đã có plan.
    /// null nếu session không tồn tại / không thuộc learner / chưa có plan.
    /// </returns>
    Task<BAPlanDto?> GetPlanBySessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default
        );
    /// <summary>
    /// Tạo mới hoặc CẬP NHẬT (Upsert) BA Plan cho một session của Learner.
    /// Nếu session không tồn tại / không thuộc learner → trả null  (Controller → 404)
    /// Nếu plan chưa có → tạo mới (Status=Draft, CreatedAt=now)
    /// Nếu plan đã có → cập nhật Objective/Approach + replace câu hỏi + UpdatedAt=now
    /// </summary>
    Task<BAPlanDto?> UpsertPlanAsync(
        int learnerId,
        int sessionId,
        UpsertPlanRequest request,
        CancellationToken cancellationToken = default
        );
}


