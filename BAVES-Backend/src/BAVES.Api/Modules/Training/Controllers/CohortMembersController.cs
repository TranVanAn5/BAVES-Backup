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
/// Quản lý danh sách Học viên trong một Lớp học (Cohort Members).
/// Cho phép Giảng viên và Quản trị viên xem danh sách, thêm từng học viên, duyệt yêu cầu tham gia, thêm hàng loạt và xóa học viên khỏi lớp.
/// </summary>
[ApiController]
[Route("api/cohorts/{cohortId:int}/members")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = Policies.InstructorOrAdmin)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class CohortMembersController(ICohortService cohortService) : ControllerBase
{
    private int GetUserIdOrThrow()
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
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

    /// <summary>
    /// Lấy danh sách học viên trong một Lớp học.
    /// Hỗ trợ lọc theo trạng thái (Active, Pending, Removed, Completed) và tìm kiếm theo Họ tên hoặc Email.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CohortMemberDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CohortMemberDto>>> GetCohortMembers(
        [FromRoute] int cohortId,
        [FromQuery] CohortMemberStatus? status,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var members = await cohortService.GetCohortMembersAsync(
                userId,
                role,
                cohortId,
                status,
                search,
                cancellationToken);

            if (members is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Cohort Not Found",
                    Detail = $"Không tìm thấy lớp học ID {cohortId}."
                });
            }

            return this.OkPaged(members, page, pageSize);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Thêm một học viên vào Lớp học qua LearnerId hoặc Email.
    /// </summary>
    [HttpPost]
    [ProducesResponseType<CohortMemberDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CohortMemberDto>> AddCohortMember(
        [FromRoute] int cohortId,
        [FromBody] AddCohortMemberRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var added = await cohortService.AddCohortMemberAsync(
                userId,
                role,
                cohortId,
                request,
                cancellationToken);

            if (added is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Cohort Not Found",
                    Detail = $"Không tìm thấy lớp học ID {cohortId}."
                });
            }

            return StatusCode(StatusCodes.Status201Created, added);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Member Request",
                Detail = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Learner Not Found",
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Member Already Exists",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Giảng viên phê duyệt yêu cầu tham gia lớp học của học viên (chuyển từ Pending sang Active).
    /// </summary>
    [HttpPost("{learnerId:int}/approve")]
    [ProducesResponseType<CohortMemberDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CohortMemberDto>> ApproveCohortMember(
        [FromRoute] int cohortId,
        [FromRoute] int learnerId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var approved = await cohortService.ApproveCohortMemberAsync(
                userId,
                role,
                cohortId,
                learnerId,
                cancellationToken);

            if (approved is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Cohort Not Found",
                    Detail = $"Không tìm thấy lớp học ID {cohortId}."
                });
            }

            return Ok(approved);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Member Request Not Found",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Thêm danh sách học viên hàng loạt vào Lớp học (qua mảng LearnerIds hoặc mảng Emails).
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType<BatchAddCohortMembersResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BatchAddCohortMembersResultDto>> BatchAddCohortMembers(
        [FromRoute] int cohortId,
        [FromBody] BatchAddCohortMembersRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var result = await cohortService.BatchAddCohortMembersAsync(
                userId,
                role,
                cohortId,
                request,
                cancellationToken);

            if (result is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Cohort Not Found",
                    Detail = $"Không tìm thấy lớp học ID {cohortId}."
                });
            }

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Batch Request",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Xóa một học viên hoặc từ chối yêu cầu tham gia Lớp học (chuyển trạng thái sang Removed).
    /// </summary>
    [HttpDelete("{learnerId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCohortMember(
        [FromRoute] int cohortId,
        [FromRoute] int learnerId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var removed = await cohortService.RemoveCohortMemberAsync(
                userId,
                role,
                cohortId,
                learnerId,
                cancellationToken);

            if (!removed)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Cohort Member Not Found",
                    Detail = $"Không tìm thấy học viên ID {learnerId} đang hoạt động trong lớp học ID {cohortId}."
                });
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = ex.Message
            });
        }
    }
}
