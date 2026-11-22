using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Analysis.Models.DTOs;
using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Analysis.Models.Requests;
using BAVES.Api.Modules.Analysis.Services.Interfaces;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Analysis.Controllers;

/// <summary>
/// Endpoint: GET /api/sessions/{sessionId}/analysis-items
/// Lấy danh sách mục phân tích (Analysis Item) của một Simulation Session cho Learner.
///
///   - Chỉ role LEARNER được truy cập (xem Policy attribute).
///   - Session phải thuộc về learner đang đăng nhập.
///   - Session hợp lệ nhưng chưa có item nào → 200 với danh sách rỗng.
///   - Session không tồn tại hoặc không thuộc learner → 404.
///   - Query param `itemType` và `status` là tùy chọn, dùng để lọc.
/// </summary>
[ApiController]
[Route("api/sessions")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = BAVES.Api.Shared.Authorization.Policies.LearnerOnly)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SessionAnalysisItemsController(
    IAnalysisItemService analysisItemService) : ControllerBase
{
    private int GetLearnerIdOrThrow()
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            NumberStyles.None, CultureInfo.InvariantCulture, out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }
        return learnerId;
    }

    /// <summary>
    /// GET /api/sessions/{sessionId}/analysis-items
    /// itemType: lọc theo loại (Finding, Requirement, BusinessRule, Constraint,
    ///               Risk, OpenQuestion, Conflict, Assumption).
    /// status: lọc theo trạng thái (Draft, Confirmed, Resolved, Archived).
    /// </summary>
    [HttpGet("{sessionId:int}/analysis-items")]
    [ProducesResponseType<IReadOnlyList<AnalysisItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AnalysisItemDto>>> GetSessionAnalysisItems(
        [FromRoute] int sessionId,
        [FromQuery] AnalysisItemType? itemType,
        [FromQuery] AnalysisItemStatus? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        // Lấy learnerId từ JWT (dùng helper để tránh lặp code).
        var learnerId = GetLearnerIdOrThrow();

        // Gọi service. Null → 404 (không tồn tại / không thuộc learner).
        var items = await analysisItemService.GetItemsBySessionAsync(
            learnerId, sessionId, itemType, status, cancellationToken);

        if (items is null)
        {
            return NotFound();
        }

        return this.OkPaged(items, page, pageSize);
    }

    /// <summary>
    /// POST /api/sessions/{sessionId}/analysis-items
    /// Tạo mới một mục phân tích trong phiên của Learner.
    ///
    ///   - Mã Code do server tự sinh (PREFIX-NNN), duy nhất trong session.
    ///   - Item mới luôn khởi tạo ở trạng thái Draft.
    ///   - Chỉ tạo được khi session còn Active → session đã kết thúc trả 409.
    ///   - Session không tồn tại hoặc không thuộc learner → 404.
    /// </summary>
    [HttpPost("{sessionId:int}/analysis-items")]
    [ProducesResponseType<AnalysisItemDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AnalysisItemDto>> CreateSessionAnalysisItem(
        [FromRoute] int sessionId,
        [FromBody] CreateAnalysisItemRequest request,
        CancellationToken cancellationToken)
    {
        // Lấy learnerId từ JWT (dùng helper để tránh lặp code).
        var learnerId = GetLearnerIdOrThrow();

        // Gọi service. Null → 404 (không tồn tại / không thuộc learner).
        var item = await analysisItemService.CreateItemAsync(
            learnerId, sessionId, request, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return StatusCode(StatusCodes.Status201Created, item);
    }

    /// <summary>
    /// PATCH /api/sessions/{sessionId}/analysis-items/{itemId}
    /// Cập nhật một phần một mục phân tích của Learner.
    ///
    ///   - Chỉ field được gửi mới thay đổi; field không gửi → giữ nguyên giá trị cũ.
    ///   - Không cho sửa Code / ItemType / SessionId (do server quản lý).
    ///   - Status không cho đặt Archived — dùng DELETE để archive.
    ///   - Chỉ sửa được khi session còn Active → session đã kết thúc trả 409.
    ///   - Item không tồn tại trong session này → 404.
    /// </summary>
    [HttpPatch("{sessionId:int}/analysis-items/{itemId:int}")]
    [ProducesResponseType<AnalysisItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AnalysisItemDto>> UpdateSessionAnalysisItem(
        [FromRoute] int sessionId,
        [FromRoute] int itemId,
        [FromBody] UpdateAnalysisItemRequest request,
        CancellationToken cancellationToken)
    {
        // Lấy learnerId từ JWT (dùng helper để tránh lặp code).
        var learnerId = GetLearnerIdOrThrow();

        // Gọi service. Null → 404 (không tồn tại / không thuộc learner).
        var item = await analysisItemService.UpdateItemAsync(
            learnerId, sessionId, itemId, request, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    /// <summary>
    /// DELETE /api/sessions/{sessionId}/analysis-items/{itemId}
    /// Xoá mềm một mục phân tích của Learner.
    ///
    ///   - Soft delete: chuyển Status sang Archived, giữ nguyên bản ghi và các
    ///     quan hệ liên kết để bảo toàn lịch sử học tập.
    ///   - Chỉ xoá được khi session còn Active → session đã kết thúc trả 409.
    ///   - Item đã Archived → 409 (không xoá lặp).
    ///   - Item không tồn tại trong session này → 404.
    /// </summary>
    [HttpDelete("{sessionId:int}/analysis-items/{itemId:int}")]
    [ProducesResponseType<AnalysisItemDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AnalysisItemDto>> DeleteSessionAnalysisItem(
        [FromRoute] int sessionId,
        [FromRoute] int itemId,
        CancellationToken cancellationToken)
    {
        // Lấy learnerId từ JWT (dùng helper để tránh lặp code).
        var learnerId = GetLearnerIdOrThrow();

        // Gọi service. Null → 404 (không tồn tại / không thuộc learner).
        var item = await analysisItemService.ArchiveItemAsync(
            learnerId, sessionId, itemId, cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}
