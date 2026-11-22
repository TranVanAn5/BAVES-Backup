using BAVES.Api.Modules.Content.Models.DTOs;
using BAVES.Api.Modules.Content.Services.Interfaces;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Content.Controllers
{
    [ApiController]
    [Route("api/industries")]
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Policy = BAVES.Api.Shared.Authorization.Policies.AnyAuthenticated)]
    [ResponseCache(NoStore =  true, Location = ResponseCacheLocation.None)]
    public sealed class IndustriesController (IContentReadService content) : ControllerBase

    {
        // Lấy danh sách các Industry đang ở trạng thái Active
        // Dùng cho màn hình catalogue để Learner/Instructor chọn ngành
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<IndustryDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IReadOnlyList<IndustryDto>>> GetIndustries(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken)
        {
            var industries = await content.GetIndustriesAsync(cancellationToken);
            return this.OkPaged(industries, page, pageSize);
        }
    }
}
