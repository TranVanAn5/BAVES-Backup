using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Content.Models.DTOs;
using BAVES.Api.Modules.Content.Services.Interfaces;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Content.Controllers;

[ApiController]
[Route("api/scenarios")]
[Authorize]
[Produces("application/json")]
public sealed class ScenariosController(IContentReadService contentRead) : ControllerBase
{
    /// <summary>
    /// Lấy danh sách scenario theo quyền:
    /// Admin = tất cả đã duyệt; Instructor = của mình; Learner = đã được giao (+ free access).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ScenarioDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ScenarioDto>>> GetScenarios(
        [FromQuery] int? applicationId,
        [FromQuery] bool? isPublished,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var roles = User.FindAll(ClaimTypes.Role)
            .Concat(User.FindAll("role"))
            .Select(c => c.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var result = await contentRead.GetScenariosAsync(
            userId,
            roles,
            applicationId,
            isPublished,
            cancellationToken);
        return this.OkPaged(result, page, pageSize);
    }

    private bool TryGetUserId(out int userId)
    {
        var raw =
            User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out userId);
    }
}
