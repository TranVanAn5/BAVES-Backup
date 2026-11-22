using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Requests;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Simulation.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "LEARNER,INSTRUCTOR,ADMIN")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SessionSprintsController(ISprintSimulationService sprintSimulationService) : ControllerBase
{
    [HttpGet("{sessionId:int}/sprints")]
    [ProducesResponseType<SprintBoardDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SprintBoardDto>> GetBoard(int sessionId, CancellationToken cancellationToken)
    {
        var board = await sprintSimulationService.GetBoardAsync(RequireUserId(), sessionId, cancellationToken);
        return Ok(board);
    }

    [HttpPost("{sessionId:int}/sprints/{sprintNo:int}/review")]
    [ProducesResponseType<SprintReviewTurnDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SprintReviewTurnDto>> Review(
        int sessionId,
        int sprintNo,
        [FromBody] ConductSprintReviewRequest request,
        [FromHeader(Name = "X-AI-Provider")] string? headerProvider,
        [FromHeader(Name = "X-AI-Model")] string? headerModel,
        [FromHeader(Name = "X-AI-Key")] string? headerApiKey,
        CancellationToken cancellationToken)
    {
        var provider = string.IsNullOrWhiteSpace(headerProvider) ? "OpenAI" : headerProvider.Trim();
        var model = string.IsNullOrWhiteSpace(headerModel) ? null : headerModel.Trim();
        var apiKey = string.IsNullOrWhiteSpace(headerApiKey) ? null : headerApiKey.Trim();
        var result = await sprintSimulationService.ConductReviewAsync(
            RequireUserId(),
            sessionId,
            sprintNo,
            request.LearnerReply,
            request.FinalizeSprint,
            new AgentAiConnection(provider, model, apiKey),
            cancellationToken);
        return Ok(result);
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
