using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Identity.Models.DTOs;
using BAVES.Api.Modules.Identity.Models.Enums;
using BAVES.Api.Modules.Identity.Models.Requests;
using BAVES.Api.Modules.Identity.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using BAVES.Api.Shared.Exceptions;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Identity.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class UsersController(IUserService users) : ControllerBase
{
    private int GetUserIdOrThrow()
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }
        return userId;
    }

    /// <summary>Returns the authenticated user's profile and current active roles.</summary>
    [HttpGet("me")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> Me(CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        return Ok(await users.GetMeAsync(userId, cancellationToken));
    }

    /// <summary>Updates the authenticated user's personal profile (FullName, AvatarUrl).</summary>
    [HttpPut("me")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> UpdateMe(
        [FromBody] UpdateMyProfileRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        return Ok(await users.UpdateMyProfileAsync(userId, request, cancellationToken));
    }

    /// <summary>Changes the authenticated user's password.</summary>
    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangeMyPassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        await users.ChangeMyPasswordAsync(userId, request, cancellationToken);
        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    /// <summary>Lists and filters users by search keyword, role, and status (for Instructor/Admin).</summary>
    [HttpGet]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<IReadOnlyList<UserProfileDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<UserProfileDto>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] UserStatus? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await users.GetUsersAsync(search, role, status, cancellationToken);
        return this.OkPaged(result, page, pageSize);
    }

    /// <summary>Gets a user's profile and active roles by ID (for Instructor/Admin).</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetUserById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var user = await users.GetUserByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>Admin tạo tài khoản người dùng mới (email, mật khẩu, họ tên, vai trò).</summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileDto>> CreateUser(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserIdOrThrow();
        try
        {
            var created = await users.CreateUserAsync(adminId, request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, created);
        }
        catch (ConflictException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message
            });
        }
    }

    /// <summary>Admin cập nhật hồ sơ tài khoản (họ tên, email, mật khẩu tùy chọn, vai trò).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileDto>> UpdateUser(
        [FromRoute] int id,
        [FromBody] UpdateAdminUserRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserIdOrThrow();
        try
        {
            var updated = await users.UpdateUserAsync(adminId, id, request, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(updated);
        }
        catch (ConflictException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = ex.Message
            });
        }
    }

    /// <summary>Admin assigns/updates roles (LEARNER, INSTRUCTOR, EXPERT, ADMIN) for a user.</summary>
    [HttpPut("{id:int}/roles")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileDto>> UpdateUserRoles(
        [FromRoute] int id,
        [FromBody] UpdateUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserIdOrThrow();
        var updated = await users.UpdateUserRolesAsync(adminId, id, request, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    /// <summary>Admin updates a user's account status (Active, Suspended, Archived).</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileDto>> UpdateUserStatus(
        [FromRoute] int id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = GetUserIdOrThrow();
        var updated = await users.UpdateUserStatusAsync(adminId, id, request, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }
}
