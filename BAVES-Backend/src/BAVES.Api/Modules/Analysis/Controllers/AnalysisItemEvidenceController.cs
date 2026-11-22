using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Analysis.Models.DTOs;
using BAVES.Api.Modules.Analysis.Models.Requests;
using BAVES.Api.Modules.Analysis.Services.Interfaces;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Analysis.Controllers;

/// <summary>
/// Quản lý liên kết bằng chứng (Evidence Links) và xác nhận mục phân tích với AI Stakeholder (Confirmation AI)
/// trên đường dẫn /api/analysis-items/{itemId}.
/// </summary>
[ApiController]
[Route("api/analysis-items")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = BAVES.Api.Shared.Authorization.Policies.LearnerOnly)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AnalysisItemEvidenceController(
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
    /// GET /api/analysis-items/{itemId}/evidence
    /// Lấy danh sách tất cả liên kết bằng chứng của một mục phân tích kèm nội dung tin nhắn trích dẫn.
    /// </summary>
    [HttpGet("{itemId:int}/evidence")]
    [ProducesResponseType<IReadOnlyList<EvidenceLinkDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<EvidenceLinkDetailDto>>> GetAnalysisItemEvidence(
        [FromRoute] int itemId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        var links = await analysisItemService.GetEvidenceLinksAsync(
            learnerId, itemId, cancellationToken);

        if (links is null)
        {
            return NotFound();
        }

        return this.OkPaged(links, page, pageSize);
    }

    /// <summary>
    /// POST /api/analysis-items/{itemId}/evidence
    /// Gắn một tin nhắn trong transcript làm bằng chứng cho mục phân tích.
    ///
    ///   - Tin nhắn phải thuộc đúng session chứa mục phân tích → sai thì 404.
    ///   - Cặp (MessageId, RelationshipType) chỉ gắn một lần cho một item → trùng thì 409.
    ///   - Session phải còn Active và item chưa bị archive → nếu không thì 409.
    /// </summary>
    [HttpPost("{itemId:int}/evidence")]
    [ProducesResponseType<EvidenceLinkDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EvidenceLinkDto>> AddAnalysisItemEvidence(
        [FromRoute] int itemId,
        [FromBody] CreateEvidenceLinkRequest request,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        var link = await analysisItemService.AddEvidenceAsync(
            learnerId, itemId, request, cancellationToken);

        if (link is null)
        {
            return NotFound();
        }

        return StatusCode(StatusCodes.Status201Created, link);
    }

    /// <summary>
    /// DELETE /api/analysis-items/{itemId}/evidence/{linkId}
    /// Gỡ một liên kết bằng chứng khỏi mục phân tích.
    ///
    ///   - Link phải thuộc đúng mục phân tích → sai thì 404.
    ///   - Session phải còn Active và item chưa bị archive → nếu không thì 409.
    ///   - Trả về thông tin liên kết đã gỡ.
    /// </summary>
    [HttpDelete("{itemId:int}/evidence/{linkId:int}")]
    [ProducesResponseType<EvidenceLinkDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EvidenceLinkDto>> DeleteAnalysisItemEvidence(
        [FromRoute] int itemId,
        [FromRoute] int linkId,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        var link = await analysisItemService.RemoveEvidenceAsync(
            learnerId, itemId, linkId, cancellationToken);

        if (link is null)
        {
            return NotFound();
        }

        return Ok(link);
    }

    /// <summary>
    /// POST /api/analysis-items/{itemId}/confirmation
    /// Gửi một mục phân tích (Requirement, Finding, BusinessRule, Constraint...) kèm các bằng chứng
    /// phỏng vấn tới một AI Stakeholder trong phiên để xác nhận:
    ///   - Confirmed: Thông tin khớp với thực tế vận hành của Stakeholder (tự động chuyển trạng thái Item sang Confirmed).
    ///   - Disputed: Thông tin mâu thuẫn hoặc sai lệch so với chia sẻ của Stakeholder.
    ///   - InsufficientInformation: Bằng chứng chưa đủ để Stakeholder xác nhận.
    /// Hỗ trợ truyền kết nối AI (BYOK) qua Header: X-AI-Provider, X-AI-Model, X-AI-Key.
    /// </summary>
    [HttpPost("{itemId:int}/confirmation")]
    [ProducesResponseType<AnalysisConfirmationResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AnalysisConfirmationResultDto>> ConfirmAnalysisItem(
        [FromRoute] int itemId,
        [FromBody] ConfirmAnalysisItemRequest request,
        [FromHeader(Name = "X-AI-Provider")] string? headerProvider,
        [FromHeader(Name = "X-AI-Model")] string? headerModel,
        [FromHeader(Name = "X-AI-Key")] string? headerApiKey,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        var result = await analysisItemService.ConfirmItemAsync(
            learnerId,
            itemId,
            request,
            headerProvider,
            headerModel,
            headerApiKey,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
