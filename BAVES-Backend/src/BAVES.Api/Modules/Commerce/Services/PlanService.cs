using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using BAVES.Api.Modules.Commerce.Services.Interfaces;

namespace BAVES.Api.Modules.Commerce.Services;

public class PlanService : IPlanService
{
    private readonly IPlanRepository _planRepository;

    public PlanService(IPlanRepository planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<List<PlanDto>> GetPublishedPlansAsync(CancellationToken ct = default)
    {
        var plans = await _planRepository.GetPublishedPlansAsync(ct);
        return plans.Select(MapToDto).ToList();
    }

    public async Task<PlanDto?> GetPlanByIdAsync(int id, CancellationToken ct = default)
    {
        var plan = await _planRepository.GetByIdWithScopesAsync(id, ct);
        return plan != null ? MapToDto(plan) : null;
    }

    private static PlanDto MapToDto(Plan p) =>
        new(
            p.Id,
            p.Code,
            p.Name,
            p.Description,
            p.PlanType,
            p.Price,
            p.Currency,
            p.DurationDays,
            p.SessionQuota,
            p.ReviewCredits,
            p.ConsultationCredits,
            p.Status,
            p.DisplayOrder,
            p.Scopes.Select(s => new PlanScopeDto(s.Id, s.ScopeType, s.ScopeId)).ToList()
        );
}
