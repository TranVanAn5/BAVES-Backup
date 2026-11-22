using BAVES.Api.Modules.Content.Models.DTOs;
using BAVES.Api.Modules.Content.Services.Interfaces;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Content.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
[Produces("application/json")]
public sealed class ApplicationsController(IContentReadService contentRead) : ControllerBase
{
    
    // Get the list of active business applications, optionally filtered by industry.
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ApplicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ApplicationDto>>> GetApplications(
        [FromQuery] int? industryId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var result = await contentRead.GetApplicationsAsync(industryId, cancellationToken);
        return this.OkPaged(result, page, pageSize);
    }
}
