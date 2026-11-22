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
/// Quản lý việc nộp bài tài liệu BA (Submissions) của học viên để chuẩn bị cho quá trình đánh giá AI và Chuyên gia.
/// </summary>
[ApiController]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = Policies.LearnerOnly)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SessionSubmissionsController(IArtifactService artifactService) : ControllerBase
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
    /// Nộp một phiên bản tài liệu BA đã đóng băng (ArtifactVersion) để đánh giá.
    /// Tự động đánh số lần nộp (AttemptNo = 1, 2...).
    /// </summary>
    [HttpPost("api/sessions/{sessionId:int}/submissions")]
    [ProducesResponseType<SubmissionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SubmissionDto>> SubmitArtifactVersion(
        [FromRoute] int sessionId,
        [FromBody] CreateSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        try
        {
            var submission = await artifactService.SubmitArtifactVersionAsync(
                learnerId, sessionId, request, cancellationToken);

            if (submission is null)
            {
                return NotFound();
            }

            return Created($"/api/submissions/{submission.Id}", submission);
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

    /// <summary>
    /// Lấy lịch sử tất cả các lần nộp bài của phiên làm việc.
    /// </summary>
    [HttpGet("api/sessions/{sessionId:int}/submissions")]
    [ProducesResponseType<IReadOnlyList<SubmissionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SubmissionDto>>> GetSessionSubmissions(
        [FromRoute] int sessionId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();
        var submissions = await artifactService.GetSubmissionsBySessionAsync(learnerId, sessionId, cancellationToken);
        if (submissions is null)
        {
            return NotFound();
        }

        return this.OkPaged(submissions, page, pageSize);
    }

    /// <summary>
    /// Xem chi tiết một lần nộp bài theo ID.
    /// </summary>
    [HttpGet("api/submissions/{id:int}")]
    [ProducesResponseType<SubmissionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SubmissionDto>> GetSubmissionById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();
        var submission = await artifactService.GetSubmissionByIdAsync(learnerId, id, cancellationToken);
        if (submission is null)
        {
            return NotFound();
        }

        return Ok(submission);
    }
}
