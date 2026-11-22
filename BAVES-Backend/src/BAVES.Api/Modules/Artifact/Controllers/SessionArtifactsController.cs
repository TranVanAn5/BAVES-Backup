using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Artifact.Models.DTOs;
using BAVES.Api.Modules.Artifact.Models.Requests;
using BAVES.Api.Modules.Artifact.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Artifact.Controllers;

/// <summary>
/// Quản lý các tài liệu BA được tạo trong phạm vi một Simulation Session của Learner.
/// Học viên có thể tạo nhiều tài liệu khác nhau (BRD, SRS, User Stories...) trong cùng phiên.
/// </summary>
[ApiController]
[Route("api/sessions/{sessionId:int}/artifacts")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = Policies.LearnerOnly)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SessionArtifactsController(IArtifactService artifactService) : ControllerBase
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
    /// Lấy danh sách tất cả các tài liệu BA mà học viên đang soạn trong phiên.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ArtifactDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ArtifactDto>>> GetSessionArtifacts(
        [FromRoute] int sessionId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();
        var artifacts = await artifactService.GetArtifactsBySessionAsync(learnerId, sessionId, cancellationToken);
        if (artifacts is null)
        {
            return NotFound();
        }

        return this.OkPaged(artifacts, page, pageSize);
    }

    /// <summary>
    /// Tạo một tài liệu BA mới trong phiên làm việc dựa trên một ArtifactTemplate.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<ArtifactDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtifactDto>> CreateArtifact(
        [FromRoute] int sessionId,
        [FromBody] CreateArtifactRequest request,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        try
        {
            var artifact = await artifactService.CreateArtifactAsync(learnerId, sessionId, request, cancellationToken);
            if (artifact is null)
            {
                return NotFound();
            }

            return Created($"/api/artifacts/{artifact.Id}", artifact);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Business Rule Conflict",
                Detail = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = ex.Message
            });
        }
    }
}
