using BAVES.Api.Modules.Commerce.Models.Entities;

namespace BAVES.Api.Modules.Commerce.Repositories.Interfaces;

public interface IPlanRepository
{
    Task<List<Plan>> GetPublishedPlansAsync(CancellationToken ct = default);
    Task<Plan?> GetByIdWithScopesAsync(int id, CancellationToken ct = default);
    Task<Plan?> GetByIdAsync(int id, CancellationToken ct = default);
}
