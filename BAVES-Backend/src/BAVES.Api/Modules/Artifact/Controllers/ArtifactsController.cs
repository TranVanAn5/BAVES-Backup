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
/// Quản lý chi tiết một tài liệu BA: xem nội dung, lưu nháp (draft) và đóng băng phiên bản (version).
/// </summary>
[ApiController]
[Route("api/artifacts")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = Policies.LearnerOnly)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class ArtifactsController(IArtifactService artifactService) : ControllerBase
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
    /// Lấy thông tin chi tiết và nội dung nháp hiện tại của một tài liệu BA.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ArtifactDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArtifactDto>> GetArtifactById(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();
        var artifact = await artifactService.GetArtifactByIdAsync(learnerId, id, cancellationToken);
        if (artifact is null)
        {
            return NotFound();
        }

        return Ok(artifact);
    }

    /// <summary>
    /// Lưu nháp (Autosave / Update Draft) nội dung tài liệu BA (hỗ trợ cả JSON cấu trúc và Plain text Markdown).
    /// </summary>
    [HttpPut("{id:int}/draft")]
    [ProducesResponseType<ArtifactDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtifactDto>> UpdateDraft(
        [FromRoute] int id,
        [FromBody] UpdateArtifactDraftRequest request,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        try
        {
            var updated = await artifactService.UpdateArtifactDraftAsync(learnerId, id, request, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(updated);
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
    }

    /// <summary>
    /// Lấy danh sách các mốc phiên bản đã đóng băng (immutable versions) của tài liệu.
    /// </summary>
    [HttpGet("{id:int}/versions")]
    [ProducesResponseType<IReadOnlyList<ArtifactVersionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ArtifactVersionDto>>> GetArtifactVersions(
        [FromRoute] int id,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();
        var versions = await artifactService.GetArtifactVersionsAsync(learnerId, id, cancellationToken);
        if (versions is null)
        {
            return NotFound();
        }

        return this.OkPaged(versions, page, pageSize);
    }

    /// <summary>
    /// Đóng băng phiên bản tài liệu (Freeze Version) để tạo snapshot bất biến trước khi nộp bài.
    /// Tự động tăng version số (VersionNo = 1, 2, 3...) và cập nhật trạng thái ReadyForReview.
    /// </summary>
    [HttpPost("{id:int}/versions")]
    [ProducesResponseType<ArtifactVersionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArtifactVersionDto>> FreezeVersion(
        [FromRoute] int id,
        [FromBody] CreateArtifactVersionRequest request,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        try
        {
            var version = await artifactService.FreezeArtifactVersionAsync(learnerId, id, request, cancellationToken);
            if (version is null)
            {
                return NotFound();
            }

            return Created($"/api/artifacts/{id}/versions/{version.Id}", version);
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
    }
}
