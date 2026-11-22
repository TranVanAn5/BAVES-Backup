using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Services.Interfaces;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Commerce.Controllers;

[ApiController]
[Route("api/plans")]
[Tags("Commerce - Plans")]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    /// <summary>
    /// Lấy danh sách các gói mô phỏng đang mở bán.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublishedPlans(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var plans = await _planService.GetPublishedPlansAsync(ct);
        return this.OkPagedAction(plans, page, pageSize);
    }

    /// <summary>
    /// Xem chi tiết một gói mô phỏng theo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanById(int id, CancellationToken ct)
    {
        var plan = await _planService.GetPlanByIdAsync(id, ct);
        if (plan == null)
        {
            return NotFound(new { message = $"Không tìm thấy gói cước ID: {id}" });
        }
        return Ok(plan);
    }
}
