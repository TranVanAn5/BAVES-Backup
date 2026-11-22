using BAVES.Api.Modules.Artifact.Models.DTOs;
using BAVES.Api.Modules.Artifact.Models.Enums;
using BAVES.Api.Modules.Artifact.Models.Requests;
using BAVES.Api.Modules.Artifact.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Artifact.Controllers;

/// <summary>
/// Quản lý danh mục các biểu mẫu tài liệu BA (Artifact Templates).
/// Cho phép Learner và Instructor khám phá cấu trúc các loại biểu mẫu
/// (BRD, SRS, User Stories Backlog, Use Case Specification, Stakeholder Register...),
/// và cho phép Instructor/Admin tạo mới, chỉnh sửa, phát hành biểu mẫu.
/// </summary>
[ApiController]
[Route("api/artifact-templates")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ArtifactTemplatesController(IArtifactService artifactService) : ControllerBase
{
    private bool IsInstructorOrAdmin => User.IsInRole("INSTRUCTOR") || User.IsInRole("ADMIN");

    /// <summary>
    /// Lấy danh sách các biểu mẫu tài liệu BA trong hệ thống.
    /// Có thể lọc theo loại tài liệu (ArtifactType) và trạng thái (Status - dành cho Instructor/Admin).
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ArtifactTemplateDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ArtifactTemplateDto>>> GetTemplates(
        [FromQuery] ArtifactType? type,
        [FromQuery] ArtifactTemplateStatus? status,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var templates = await artifactService.GetTemplatesAsync(
            type, status, IsInstructorOrAdmin, cancellationToken);
        return this.OkPaged(templates, page, pageSize);
    }

    /// <summary>
    /// Lấy chi tiết một biểu mẫu tài liệu BA cùng danh sách các đề mục (Sections).
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ArtifactTemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtifactTemplateDto>> GetTemplateById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var template = await artifactService.GetTemplateByIdAsync(id, cancellationToken);
        if (template is null)
        {
            return NotFound();
        }

        return Ok(template);
    }

    /// <summary>
    /// Giảng viên / Quản trị viên tạo mới một biểu mẫu tài liệu BA (ArtifactTemplate) kèm các đề mục (Sections).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<ArtifactTemplateDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtifactTemplateDto>> CreateTemplate(
        [FromBody] CreateArtifactTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var created = await artifactService.CreateTemplateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTemplateById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Giảng viên / Quản trị viên cập nhật thông tin biểu mẫu tài liệu BA và các đề mục khi biểu mẫu còn ở trạng thái Draft.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<ArtifactTemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtifactTemplateDto>> UpdateTemplate(
        [FromRoute] int id,
        [FromBody] UpdateArtifactTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await artifactService.UpdateTemplateAsync(id, request, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    /// <summary>
    /// Giảng viên / Quản trị viên phát hành (Publish) biểu mẫu tài liệu BA để sẵn sàng cho Học viên sử dụng.
    /// </summary>
    [HttpPost("{id:int}/publish")]
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<ArtifactTemplateDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtifactTemplateDto>> PublishTemplate(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var published = await artifactService.PublishTemplateAsync(id, cancellationToken);
        if (published is null)
        {
            return NotFound();
        }

        return Ok(published);
    }
}
