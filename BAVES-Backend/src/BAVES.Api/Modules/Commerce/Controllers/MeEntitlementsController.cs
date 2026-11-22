using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Services.Interfaces;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Commerce.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
[Tags("Commerce - Entitlements")]
public class MeEntitlementsController : ControllerBase
{
    private readonly IEntitlementService _entitlementService;

    public MeEntitlementsController(IEntitlementService entitlementService)
    {
        _entitlementService = entitlementService;
    }

    /// <summary>
    /// Lấy danh sách các quyền truy cập kịch bản mô phỏng của người dùng hiện tại.
    /// </summary>
    [HttpGet("entitlements")]
    [ProducesResponseType(typeof(List<EntitlementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyEntitlements(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var entitlements = await _entitlementService.GetUserEntitlementsAsync(userId.Value, ct);
        return this.OkPagedAction(entitlements, page, pageSize);
    }

    /// <summary>
    /// Lấy số dư credit gửi bài cho Chuyên gia chấm điểm.
    /// </summary>
    [HttpGet("review-credits")]
    [ProducesResponseType(typeof(CreditSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyReviewCredits(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var credits = await _entitlementService.GetReviewCreditsAsync(userId.Value, ct);
        return Ok(new CreditSummaryDto(userId.Value, credits, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Lấy số dư credit booking buổi trao đổi 1-on-1 với Chuyên gia.
    /// </summary>
    [HttpGet("consultation-credits")]
    [ProducesResponseType(typeof(CreditSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyConsultationCredits(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var credits = await _entitlementService.GetConsultationCreditsAsync(userId.Value, ct);
        return Ok(new CreditSummaryDto(userId.Value, credits, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Lấy tổng quan số dư credit (Review và Consultation) kèm lịch sử biến động.
    /// </summary>
    [HttpGet("credits-summary")]
    [ProducesResponseType(typeof(CreditBalanceSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCreditsSummary(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var summary = await _entitlementService.GetCreditBalanceSummaryAsync(userId.Value, ct);
        return Ok(summary);
    }

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(sub, out var id) ? id : null;
    }
}
