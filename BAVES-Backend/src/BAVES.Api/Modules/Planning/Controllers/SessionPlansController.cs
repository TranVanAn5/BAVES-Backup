using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Planning.Models.DTOs;
using BAVES.Api.Modules.Planning.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Planning.Controllers;

/// <summary>
/// Endpoint: GET /api/sessions/{id}/plan
/// Lấy BA Plan (kế hoạch) của một Simulation Session cho Learner.
/// 
/// Quy tắc:
///   - Chỉ role LEARNER được truy cập (xem Policy attribute).
///   - Session phải thuộc về learner đang đăng nhập.
///   - Nếu session chưa có plan → 404 NotFound.
///   - Trả về plan + danh sách câu hỏi (sắp theo DisplayOrder).
/// </summary>
[ApiController]
[Route("api/sessions")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = BAVES.Api.Shared.Authorization.Policies.LearnerOnly)] //Policy này yêu cầu role = "LEARNER"
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)] //mỗi lần gọi phải trả data mới
public sealed class SessionPlansController(
    IPlanningService planningService) : ControllerBase //Kế thừa từ ControllerBase (cung cấp Ok(), NotFound()..
{
    // ─────────── HELPER ───────────
    /// <summary>
    /// Đọc learnerId từ JWT claim "sub" (lưu trong token khi login).
    /// [Authorize] ở trên class đã đảm bảo token hợp lệ,
    /// nhưng vẫn check format để tránh lỗi parse lúc runtime.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// Ném khi claim "sub" không tồn tại hoặc không phải số nguyên.
    /// Middleware sẽ chuyển thành HTTP 401.
    /// </exception>
    private int GetLearnerIdOrThrow()
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            NumberStyles.None, CultureInfo.InvariantCulture, out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }
        return learnerId;
    }

    ///<summary>
    ///GET /api/session/{id}/plan
    /// </summary>
    [HttpGet("{id:int}/plan")]
    [ProducesResponseType<BAPlanDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BAPlanDto>> GetSessionPlan(
        int id,
        CancellationToken cancellationToken)
    {
        // Lấy learnerId từ JWT (dùng helper để tránh lặp code).
        var learnerId = GetLearnerIdOrThrow();

        // Gọi service. Null → 404 (không tồn tại / không thuộc learner / chưa có plan).
        var plan = await planningService.GetPlanBySessionAsync(
            learnerId, id, cancellationToken);

        if (plan == null)
        {
            return NotFound();
        }
        return Ok(plan);
    }
    [HttpPut("{id:int}/plan")]
    public async Task<ActionResult<BAPlanDto>> UpsertPlan(
        [FromRoute] int id,    // {id} trong URL
        [FromBody] UpsertPlanRequest request, // body JSON client gửi
        CancellationToken cancellationToken = default)  // nút bấm Hủy
    {
        // Lấy learnerId từ JWT (dùng helper để tránh lặp code).
        var learnerId = GetLearnerIdOrThrow();

        // GỌI SERVICE
        var plan = await planningService.UpsertPlanAsync(
            learnerId,
            id,
            request,
            cancellationToken);

        // CHUYỂN KẾT QUẢ THÀNH HTTP RESPONSE
        if (plan == null)
        {
            return NotFound();
        }

        return Ok(plan);
    }
}
