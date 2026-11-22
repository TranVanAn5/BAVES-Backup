using BAVES.Api.Modules.Commerce.Models.DTOs;

namespace BAVES.Api.Modules.Commerce.Services.Interfaces;

public interface IPlanService
{
    Task<List<PlanDto>> GetPublishedPlansAsync(CancellationToken ct = default);
    Task<PlanDto?> GetPlanByIdAsync(int id, CancellationToken ct = default);
}
