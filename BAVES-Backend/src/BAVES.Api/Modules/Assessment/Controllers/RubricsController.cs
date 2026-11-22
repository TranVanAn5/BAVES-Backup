using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Assessment.Models.DTOs;
using BAVES.Api.Modules.Assessment.Models.Enums;
using BAVES.Api.Modules.Assessment.Models.Requests;
using BAVES.Api.Modules.Assessment.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Assessment.Controllers;

/// <summary>
/// Quản lý bộ tiêu chí chấm điểm (Rubrics) và danh mục nhiệm vụ chuẩn BABOK (BABOK Tasks).
/// Cho phép Learner/Expert xem tiêu chí chấm điểm và cho phép Instructor/Admin thiết kế, cập nhật, phát hành Rubric.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class RubricsController(IAssessmentService assessmentService) : ControllerBase
{
    private bool IsInstructorOrAdmin => User.IsInRole("INSTRUCTOR") || User.IsInRole("ADMIN");

    private int GetUserIdOrThrow()
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
        {
            throw new UnauthorizedAccessException();
        }
        return userId;
    }

    /// <summary>
    /// Lấy danh mục các nhiệm vụ chuẩn BABOK v3 (BABOK Tasks) để gắn với từng tiêu chí trong Rubric.
    /// </summary>
    [HttpGet("/api/babok-tasks")]
    [ProducesResponseType<IReadOnlyList<BabokTaskDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<BabokTaskDto>>> GetBabokTasks(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var tasks = await assessmentService.GetBabokTasksAsync(cancellationToken);
        return this.OkPaged(tasks, page, pageSize);
    }

    /// <summary>
    /// Lấy danh sách các bộ tiêu chí chấm điểm (Rubrics).
    /// Hỗ trợ lọc theo scenarioVersionId và status (dành cho Instructor/Admin).
    /// </summary>
    [HttpGet("/api/rubrics")]
    [ProducesResponseType<IReadOnlyList<RubricDetailDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<RubricDetailDto>>> GetRubrics(
        [FromQuery] int? scenarioVersionId,
        [FromQuery] RubricStatus? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var rubrics = await assessmentService.GetRubricsAsync(
            scenarioVersionId, status, IsInstructorOrAdmin, cancellationToken);
        return this.OkPaged(rubrics, page, pageSize);
    }

    /// <summary>
    /// Lấy chi tiết một bộ tiêu chí chấm điểm (Rubric) cùng danh sách các tiêu chí con (Criteria).
    /// </summary>
    [HttpGet("/api/rubrics/{id:int}")]
    [ProducesResponseType<RubricDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RubricDetailDto>> GetRubricById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var rubric = await assessmentService.GetRubricDetailByIdAsync(id, cancellationToken);
        if (rubric is null)
        {
            return NotFound();
        }

        return Ok(rubric);
    }

    /// <summary>
    /// Giảng viên / Quản trị viên tạo mới một bộ tiêu chí chấm điểm (Rubric) cho một Scenario Version.
    /// Tự động kiểm tra tổng trọng số (Weight = 100% hoặc 1.0) và tính Version tiếp theo.
    /// </summary>
    [HttpPost("/api/rubrics")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<RubricDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RubricDetailDto>> CreateRubric(
        [FromBody] CreateRubricRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserIdOrThrow();
        var created = await assessmentService.CreateRubricAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetRubricById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Giảng viên / Quản trị viên cập nhật bộ tiêu chí chấm điểm (Rubric) khi còn ở trạng thái Draft.
    /// </summary>
    [HttpPut("/api/rubrics/{id:int}")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<RubricDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RubricDetailDto>> UpdateRubric(
        [FromRoute] int id,
        [FromBody] UpdateRubricRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await assessmentService.UpdateRubricAsync(id, request, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    /// <summary>
    /// Giảng viên / Quản trị viên phát hành (Publish) bộ tiêu chí chấm điểm (Rubric) để đóng băng và sẵn sàng chấm bài.
    /// </summary>
    [HttpPost("/api/rubrics/{id:int}/publish")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<RubricDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RubricDetailDto>> PublishRubric(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var published = await assessmentService.PublishRubricAsync(id, cancellationToken);
        if (published is null)
        {
            return NotFound();
        }

        return Ok(published);
    }
}
