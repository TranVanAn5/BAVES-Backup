using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Training.Models.DTOs;
using BAVES.Api.Modules.Training.Models.Enums;
using BAVES.Api.Modules.Training.Models.Requests;
using BAVES.Api.Modules.Training.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Training.Controllers;

/// <summary>
/// Quản lý Lớp học (Cohorts), Báo cáo phân tích Lớp học và Yêu cầu tham gia của Học viên.
/// </summary>
[ApiController]
[Route("api/cohorts")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class CohortsController(ICohortService cohortService) : ControllerBase
{
    private int GetUserIdOrThrow()
    {
        var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }
        return userId;
    }

    private string GetUserRole()
    {
        if (User.IsInRole("ADMIN")) return "ADMIN";
        if (User.IsInRole("INSTRUCTOR")) return "INSTRUCTOR";
        if (User.IsInRole("EXPERT")) return "EXPERT";
        return "LEARNER";
    }

    [HttpPost]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<CohortDetailDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<CohortDetailDto>> CreateCohort(
        [FromBody] CreateCohortRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        try
        {
            var cohort = await cohortService.CreateCohortAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetCohortById), new { id = cohort.Id }, cohort);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Invalid Cohort", Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Duplicate Cohort Code", Detail = ex.Message });
        }
    }

    [HttpGet]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<IReadOnlyList<CohortSummaryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CohortSummaryDto>>> GetCohorts(
        [FromQuery] CohortStatus? status,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();
        var cohorts = await cohortService.GetCohortsAsync(userId, role, status, search, cancellationToken);
        return this.OkPaged(cohorts, page, pageSize);
    }

    [HttpGet("available")]
    [ProducesResponseType<IReadOnlyList<AvailableCohortDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AvailableCohortDto>>> GetAvailableCohorts(
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var cohorts = await cohortService.GetAvailableCohortsAsync(userId, search, cancellationToken);
        return this.OkPaged(cohorts, page, pageSize);
    }

    [HttpPost("{id:int}/join-request")]
    [ProducesResponseType<LearnerCohortDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LearnerCohortDto>> RequestJoinCohort(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        try
        {
            var result = await cohortService.RequestJoinCohortAsync(userId, id, null, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Cohort Not Found", Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Join Request Conflict", Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Invalid Request", Detail = ex.Message });
        }
    }

    [HttpPost("join-by-code")]
    [ProducesResponseType<LearnerCohortDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LearnerCohortDto>> RequestJoinCohortByCode(
        [FromBody] JoinCohortByCodeRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        try
        {
            var result = await cohortService.RequestJoinCohortAsync(userId, null, request.Code, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Cohort Not Found", Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Join Request Conflict", Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Invalid Request", Detail = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<CohortDetailDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CohortDetailDto>> GetCohortById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();
        try
        {
            var cohort = await cohortService.GetCohortByIdAsync(userId, role, id, cancellationToken);
            if (cohort is null)
                return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Cohort Not Found" });
            return Ok(cohort);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Status = StatusCodes.Status403Forbidden, Title = "Forbidden", Detail = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<CohortDetailDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CohortDetailDto>> UpdateCohort(
        [FromRoute] int id,
        [FromBody] UpdateCohortRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();
        try
        {
            var cohort = await cohortService.UpdateCohortAsync(userId, role, id, request, cancellationToken);
            if (cohort is null)
                return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Cohort Not Found" });
            return Ok(cohort);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Status = StatusCodes.Status403Forbidden, Title = "Forbidden", Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Status = StatusCodes.Status400BadRequest, Title = "Invalid Cohort", Detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Status = StatusCodes.Status409Conflict, Title = "Duplicate Cohort Code", Detail = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<CohortDetailDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CohortDetailDto>> UpdateCohortStatus(
        [FromRoute] int id,
        [FromBody] UpdateCohortStatusRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();
        try
        {
            var cohort = await cohortService.UpdateCohortStatusAsync(userId, role, id, request.Status, cancellationToken);
            if (cohort is null)
                return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Cohort Not Found" });
            return Ok(cohort);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Status = StatusCodes.Status403Forbidden, Title = "Forbidden", Detail = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCohort(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();
        try
        {
            var deleted = await cohortService.DeleteCohortAsync(userId, role, id, cancellationToken);
            if (!deleted)
                return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Cohort Not Found" });
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Status = StatusCodes.Status403Forbidden, Title = "Forbidden", Detail = ex.Message });
        }
    }

    [HttpGet("{id:int}/analytics")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<CohortAnalyticsDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CohortAnalyticsDto>> GetCohortAnalytics(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();
        try
        {
            var analytics = await cohortService.GetCohortAnalyticsAsync(userId, role, id, cancellationToken);
            if (analytics is null)
                return NotFound(new ProblemDetails { Status = StatusCodes.Status404NotFound, Title = "Cohort Not Found" });
            return Ok(analytics);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails { Status = StatusCodes.Status403Forbidden, Title = "Forbidden", Detail = ex.Message });
        }
    }

    [HttpGet("/api/me/cohorts")]
    [ProducesResponseType<IReadOnlyList<LearnerCohortDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LearnerCohortDto>>> GetMyCohorts(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var result = await cohortService.GetMyCohortsAsync(userId, cancellationToken);
        return this.OkPaged(result, page, pageSize);
    }
}
