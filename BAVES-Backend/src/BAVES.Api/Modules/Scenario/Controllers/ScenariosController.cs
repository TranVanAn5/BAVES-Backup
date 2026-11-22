using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Requests;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Scenario.Controllers;

[ApiController]
[Route("api/scenarios")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ScenariosController(IScenarioAuthoringService authoringService) : ControllerBase
{
    /// <summary>
    /// [Endpoint 15] Tạo kịch bản Scenario mới ở trạng thái Draft kèm Version 1.
    /// Admin: catalog toàn hệ thống (sau Expert duyệt → Scenario Library).
    /// Instructor: đề nội bộ lớp (sau duyệt chỉ xuất hiện trong danh sách của instructor).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<ScenarioSummaryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioSummaryDto>> CreateScenario(
        [FromBody] CreateScenarioRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        var result = await authoringService.CreateScenarioAsync(userId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// [Endpoint 16] Tạo một phiên bản mới (VersionNo + 1) cho Scenario đã có.
    /// Cho phép tùy chọn sao chép toàn bộ nội dung từ Version gần nhất.
    /// Chỉ tạo được khi không có bản Draft nào khác đang mở.
    /// </summary>
    [HttpPost("{id:int}/versions")]
    [ProducesResponseType<ScenarioVersionSummaryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ScenarioVersionSummaryDto>> CreateVersion(
        [FromRoute] int id,
        [FromBody] CreateScenarioVersionRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        var result = await authoringService.CreateVersionAsync(userId, id, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
