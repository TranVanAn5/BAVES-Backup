using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Requests;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Scenario.Controllers;

[ApiController]
[Route("api/scenario-versions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ScenarioVersionsController(IScenarioAuthoringService authoringService) : ControllerBase
{
    /// <summary>
    /// [Endpoint 17] Lấy toàn bộ định nghĩa Scenario Version (bao gồm Personas, Knowledge Items,
    /// Bí mật ẩn, Business Rules và Events) dành cho Giảng viên / Tác giả chỉnh sửa kịch bản.
    /// </summary>
    [HttpGet("{id:int}/definition")]
    [ProducesResponseType<ScenarioDefinitionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScenarioDefinitionDto>> GetDefinition(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        var result = await authoringService.GetDefinitionAsync(userId, id, cancellationToken, CurrentRoles());
        return Ok(result);
    }

    /// <summary>
    /// [Endpoint 18] Cập nhật toàn bộ nội dung kịch bản (Personas, Knowledge Items,
    /// Bí mật ẩn, Business Rules và Events) khi Scenario Version còn ở trạng thái Draft.
    /// Dành cho Giảng viên / Tác giả kịch bản.
    /// </summary>
    [HttpPut("{id:int}/definition")]
    [ProducesResponseType<ScenarioDefinitionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioDefinitionDto>> UpdateDefinition(
        [FromRoute] int id,
        [FromBody] UpdateScenarioDefinitionRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        var result = await authoringService.UpdateDefinitionAsync(
            userId,
            id,
            request,
            cancellationToken,
            CurrentRoles());
        return Ok(result);
    }

    /// <summary>
    /// [Endpoint mới] Lấy context "core" (BusinessContext, BusinessProblem, BusinessObjectives,
    /// Summary) của một Scenario Version đã được publish — dùng cho Learner đọc trước khi
    /// bắt đầu phỏng vấn stakeholder. User có role BAExpert/Admin có thể xem cả bản nháp.
    /// </summary>
    [HttpGet("{id:int}/context")]
    [ProducesResponseType<ScenarioContextDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScenarioContextDto>> GetContext(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        // Lấy danh sách role từ claims (Identity gắn role claim khi đăng nhập)
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        var result = await authoringService.GetContextAsync(userId, roles, id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Danh sách sprint của version (bảng scenario.ScenarioSprints).
    /// </summary>
    [HttpGet("{id:int}/sprints")]
    [ProducesResponseType<ScenarioSprintListDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScenarioSprintListDto>> GetSprints(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var result = await authoringService.GetSprintsAsync(userId, id, cancellationToken, CurrentRoles());
        return Ok(result);
    }

    /// <summary>
    /// Thay thế toàn bộ danh sách sprint (tạo / chỉnh bao nhiêu sprint tùy ý).
    /// </summary>
    [HttpPut("{id:int}/sprints")]
    [ProducesResponseType<ScenarioSprintListDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioSprintListDto>> ReplaceSprints(
        [FromRoute] int id,
        [FromBody] ReplaceScenarioSprintsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var result = await authoringService.ReplaceSprintsAsync(
            userId, id, request, cancellationToken, CurrentRoles());
        return Ok(result);
    }

    /// <summary>
    /// Thêm một sprint mới vào cuối danh sách.
    /// </summary>
    [HttpPost("{id:int}/sprints")]
    [ProducesResponseType<ScenarioSprintListDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioSprintListDto>> AddSprint(
        [FromRoute] int id,
        [FromBody] UpsertScenarioSprintRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var result = await authoringService.AddSprintAsync(
            userId, id, request, cancellationToken, CurrentRoles());
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Cập nhật một sprint theo số thứ tự.
    /// </summary>
    [HttpPut("{id:int}/sprints/{sprintNo:int}")]
    [ProducesResponseType<ScenarioSprintListDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioSprintListDto>> UpdateSprint(
        [FromRoute] int id,
        [FromRoute] int sprintNo,
        [FromBody] UpsertScenarioSprintRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var result = await authoringService.UpdateSprintAsync(
            userId, id, sprintNo, request, cancellationToken, CurrentRoles());
        return Ok(result);
    }

    /// <summary>
    /// Xóa một sprint (phải còn ít nhất 1 sprint).
    /// </summary>
    [HttpDelete("{id:int}/sprints/{sprintNo:int}")]
    [ProducesResponseType<ScenarioSprintListDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioSprintListDto>> DeleteSprint(
        [FromRoute] int id,
        [FromRoute] int sprintNo,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        var result = await authoringService.DeleteSprintAsync(
            userId, id, sprintNo, cancellationToken, CurrentRoles());
        return Ok(result);
    }

    /// <summary>
    /// Hủy bản nháp / bị từ chối. Nếu còn version khác thì quay về version đó; nếu là bản duy nhất thì xóa luôn scenario.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType<DiscardDraftResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DiscardDraftResultDto>> DiscardDraft(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var userId = RequireUserId();
        var result = await authoringService.DiscardDraftVersionAsync(
            userId,
            CurrentRoles(),
            id,
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Instructor/Admin gửi phiên bản kịch bản cho Expert duyệt. Không publish trực tiếp.
    /// </summary>
    [HttpPost("{id:int}/submit-review")]
    [ProducesResponseType<ScenarioReviewDecisionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioReviewDecisionDto>> SubmitForReview(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var userId = RequireUserId();
        var result = await authoringService.SubmitForReviewAsync(userId, CurrentRoles(), id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Expert duyệt phiên bản đang chờ. Duyệt xong thì phiên bản được publish.
    /// </summary>
    [HttpPost("{id:int}/approve")]
    [ProducesResponseType<ScenarioReviewDecisionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioReviewDecisionDto>> Approve(
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var userId = RequireUserId();
        var result = await authoringService.ApproveVersionAsync(userId, CurrentRoles(), id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Expert từ chối phiên bản đang chờ và gửi nhận xét về cho tác giả.
    /// </summary>
    [HttpPost("{id:int}/reject")]
    [ProducesResponseType<ScenarioReviewDecisionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioReviewDecisionDto>> Reject(
        [FromRoute] int id,
        [FromBody] RejectScenarioVersionRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = RequireUserId();
        var result = await authoringService.RejectVersionAsync(
            userId,
            CurrentRoles(),
            id,
            request.Comment,
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Hàng đợi kịch bản đang chờ Expert duyệt.
    /// </summary>
    [HttpGet("review-queue")]
    [ProducesResponseType<IReadOnlyList<ScenarioReviewQueueItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ScenarioReviewQueueItemDto>>> ReviewQueue(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken
    )
    {
        RequireUserId();
        var result = await authoringService.ListPendingReviewsAsync(CurrentRoles(), cancellationToken);
        return this.OkPaged(result, page, pageSize);
    }

    /// <summary>
    /// Lịch sử kịch bản mà Expert hiện tại đã duyệt hoặc từ chối.
    /// </summary>
    [HttpGet("reviewed-by-me")]
    [ProducesResponseType<IReadOnlyList<ScenarioReviewQueueItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ScenarioReviewQueueItemDto>>> ReviewedByMe(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken
    )
    {
        var userId = RequireUserId();
        var result = await authoringService.ListReviewedByMeAsync(
            userId,
            CurrentRoles(),
            cancellationToken);
        return this.OkPaged(result, page, pageSize);
    }

    /// <summary>
    /// Các phiên bản kịch bản do chính người dùng tạo, kể cả nháp, chờ duyệt và bị từ chối.
    /// </summary>
    [HttpGet("mine")]
    [ProducesResponseType<IReadOnlyList<ScenarioReviewQueueItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ScenarioReviewQueueItemDto>>> Mine(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken
    )
    {
        var userId = RequireUserId();
        var result = await authoringService.ListMyVersionsAsync(userId, cancellationToken);
        return this.OkPaged(result, page, pageSize);
    }

    /// <summary>
    /// Publish trực tiếp đã đóng. Instructor/Admin phải gửi duyệt; Expert approve mới publish.
    /// </summary>
    [HttpPost("{id:int}/publish")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public ActionResult Publish([FromRoute] int id)
    {
        throw new BAVES.Api.Shared.Exceptions.ConflictException(
            $"Direct publish is disabled for version {id}. Submit it for expert review via POST /api/scenario-versions/{id}/submit-review.");
    }

    private int RequireUserId()
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        return userId;
    }

    private IReadOnlyList<string> CurrentRoles() =>
        User.FindAll(ClaimTypes.Role)
            .Concat(User.FindAll("role"))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
}
