using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Assessment.Controllers;

[ApiController]
[Route("api/me")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = Policies.AnyAuthenticated)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Tags("Assessment - Learner Competency Progress")]
public sealed class MeCompetencyController(IAssessmentService assessmentService) : ControllerBase
{
    /// <summary>
    /// Hồ sơ năng lực theo thời gian: điểm AI / expert / sprint trung bình theo từng phiên.
    /// </summary>
    [HttpGet("competency-progress")]
    [ProducesResponseType<CompetencyProgressDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CompetencyProgressDto>> GetCompetencyProgress(CancellationToken cancellationToken)
    {
        var progress = await assessmentService.GetCompetencyProgressAsync(RequireUserId(), cancellationToken);
        return Ok(progress);
    }

    private int RequireUserId()
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }

        return userId;
    }
}
