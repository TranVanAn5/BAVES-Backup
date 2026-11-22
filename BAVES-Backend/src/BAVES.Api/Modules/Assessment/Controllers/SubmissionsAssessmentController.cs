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

/// <summary>
/// Quản lý đánh giá AI (AI Practice Feedback) cho bài nộp của học viên.
/// Kết nối trực tiếp với AI Assessor Runtime để chấm điểm theo Rubric BABOK.
/// </summary>
[ApiController]
[Route("api/submissions")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = Policies.LearnerOnly)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SubmissionsAssessmentController(IAssessmentService assessmentService) : ControllerBase
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
    /// Gửi bài nộp sang AI Assessor Runtime để thực hiện chấm điểm theo Rubric BABOK.
    /// Hỗ trợ truyền API Key qua Header (BYOK: X-AI-Provider, X-AI-Model, X-AI-Key).
    /// </summary>
    [HttpPost("{id:int}/evaluate")]
    [ProducesResponseType<AIPracticeFeedbackDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<AIPracticeFeedbackDto>> EvaluateSubmission(
        [FromRoute] int id,
        [FromHeader(Name = "X-AI-Provider")] string? headerProvider,
        [FromHeader(Name = "X-AI-Model")] string? headerModel,
        [FromHeader(Name = "X-AI-Key")] string? headerApiKey,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        try
        {
            var feedback = await assessmentService.EvaluateSubmissionAsync(
                learnerId,
                id,
                headerProvider,
                headerModel,
                headerApiKey,
                cancellationToken);

            if (feedback is null)
            {
                return NotFound();
            }

            return Ok(feedback);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Assessment Conflict",
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
    /// Xem kết quả phản hồi và bảng điểm chi tiết theo từng tiêu chí Rubric của bài nộp.
    /// </summary>
    [HttpGet("{id:int}/ai-feedback")]
    [ProducesResponseType<AIPracticeFeedbackDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AIPracticeFeedbackDto>> GetAIFeedback(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();
        var feedback = await assessmentService.GetFeedbackBySubmissionAsync(learnerId, id, cancellationToken);
        if (feedback is null)
        {
            return NotFound();
        }

        return Ok(feedback);
    }
}
