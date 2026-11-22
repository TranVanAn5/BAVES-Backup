using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Simulation.Models.DTOs;
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
/// Quản lý Giao bài tập (Assignments) cho Giảng viên và luồng làm bài tập cho Học viên.
/// </summary>
[ApiController]
[Route("api/assignments")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AssignmentsController(IAssignmentService assignmentService) : ControllerBase
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
    /// Giảng viên hoặc Quản trị viên tạo một Bài tập mới (giao cho Lớp học hoặc Cá nhân).
    /// </summary>
    [HttpPost]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<AssignmentDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentDetailDto>> CreateAssignment(
        [FromBody] CreateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var created = await assignmentService.CreateAssignmentAsync(
                userId,
                role,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetAssignmentById),
                new { id = created.Id },
                created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Assignment Data",
                Detail = ex.Message
            });
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
                Title = "Resource Not Found",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Lấy danh sách Bài tập (Giảng viên xem bài tập mình quản lý, Admin xem toàn bộ).
    /// Hỗ trợ lọc theo lớp học (cohortId), trạng thái (status) và tìm kiếm tiêu đề (search).
    /// </summary>
    [HttpGet]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<IReadOnlyList<AssignmentSummaryDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<AssignmentSummaryDto>>> GetAssignments(
        [FromQuery] int? cohortId,
        [FromQuery] AssignmentStatus? status,
        [FromQuery] string? search,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        var list = await assignmentService.GetAssignmentsAsync(
            userId,
            role,
            cohortId,
            status,
            search,
            cancellationToken);

        return this.OkPaged(list, page, pageSize);
    }

    /// <summary>
    /// Xem chi tiết một Bài tập (Đề bài, Hướng dẫn, Hạn nộp, Kịch bản liên kết).
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.AnyAuthenticated)]
    [ProducesResponseType<AssignmentDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentDetailDto>> GetAssignmentById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var detail = await assignmentService.GetAssignmentByIdAsync(
                userId,
                role,
                id,
                cancellationToken);

            if (detail is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Assignment Not Found",
                    Detail = $"Không tìm thấy bài tập ID {id}."
                });
            }

            return Ok(detail);
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
    /// Chỉnh sửa thông tin Bài tập (Tiêu đề, Hướng dẫn, Hạn nộp, Số lần làm tối đa).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<AssignmentDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentDetailDto>> UpdateAssignment(
        [FromRoute] int id,
        [FromBody] UpdateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var updated = await assignmentService.UpdateAssignmentAsync(
                userId,
                role,
                id,
                request,
                cancellationToken);

            if (updated is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Assignment Not Found",
                    Detail = $"Không tìm thấy bài tập ID {id}."
                });
            }

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid Assignment Data",
                Detail = ex.Message
            });
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
    /// Phát hành / Mở đề Bài tập (Chuyển trạng thái sang Active để học viên bắt đầu làm bài).
    /// </summary>
    [HttpPost("{id:int}/publish")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<AssignmentDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AssignmentDetailDto>> PublishAssignment(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var published = await assignmentService.PublishAssignmentAsync(
                userId,
                role,
                id,
                cancellationToken);

            if (published is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Assignment Not Found",
                    Detail = $"Không tìm thấy bài tập ID {id}."
                });
            }

            return Ok(published);
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Assignment State Conflict",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Đóng / Khóa Bài tập (Chuyển trạng thái sang Closed, dừng nhận lượt làm bài mới).
    /// </summary>
    [HttpPost("{id:int}/close")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<AssignmentDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentDetailDto>> CloseAssignment(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var closed = await assignmentService.CloseAssignmentAsync(
                userId,
                role,
                id,
                cancellationToken);

            if (closed is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Assignment Not Found",
                    Detail = $"Không tìm thấy bài tập ID {id}."
                });
            }

            return Ok(closed);
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
    /// Học viên xem danh sách các Bài tập được giao cho mình (theo Lớp học hoặc Cá nhân),
    /// kèm tiến độ thực hiện, hạn nộp và số lượt làm bài còn lại.
    /// </summary>
    /// <summary>
    /// L?y S? di?m v� b?ng theo d�i ti?n d? c?a c? L?p h?c cho m?t B�i t?p (Gradebook).
    /// </summary>
    [HttpGet("{id:int}/gradebook")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<AssignmentGradebookDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentGradebookDto>> GetAssignmentGradebook(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var gradebook = await assignmentService.GetAssignmentGradebookAsync(
                userId,
                role,
                id,
                cancellationToken);

            if (gradebook is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Assignment Not Found",
                    Detail = $"Kh�ng t�m th?y b�i t?p ID {id}."
                });
            }

            return Ok(gradebook);
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
    /// Xu?t S? di?m c?a B�i t?p ra file CSV chu?n UTF-8.
    /// </summary>
    [HttpGet("{id:int}/gradebook/export")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportAssignmentGradebookCsv(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var role = GetUserRole();

        try
        {
            var csvBytes = await assignmentService.ExportAssignmentGradebookCsvAsync(
                userId,
                role,
                id,
                cancellationToken);

            if (csvBytes is null)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Assignment Not Found",
                    Detail = $"Kh�ng t�m th?y b�i t?p ID {id}."
                });
            }

            var fileName = $"gradebook-assignment-{id}-{DateTime.UtcNow:yyyyMMdd}.csv";
            return File(csvBytes, "text/csv; charset=utf-8", fileName);
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

    [HttpGet("/api/me/assignments")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.LearnerOnly)]
    [ProducesResponseType<IReadOnlyList<LearnerAssignmentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<LearnerAssignmentDto>>> GetMyAssignments(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var learnerId = GetUserIdOrThrow();

        var list = await assignmentService.GetMyAssignmentsAsync(learnerId, cancellationToken);
        return this.OkPaged(list, page, pageSize);
    }

    /// <summary>
    /// Học viên bắt đầu một phiên làm bài tập (Simulation Session) gắn trực tiếp với Assignment.
    /// Hệ thống tự động kiểm tra quyền thuộc lớp, thời hạn mở/đóng đề và giới hạn số lần làm (MaxAttempts).
    /// </summary>
    [HttpPost("{id:int}/start")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = Policies.LearnerOnly)]
    [ProducesResponseType<SessionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionDto>> StartAssignmentSession(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var learnerId = GetUserIdOrThrow();

        try
        {
            var session = await assignmentService.StartAssignmentSessionAsync(
                learnerId,
                id,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, session);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Assignment Not Found",
                Detail = ex.Message
            });
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Cannot Start Assignment",
                Detail = ex.Message
            });
        }
    }
}
