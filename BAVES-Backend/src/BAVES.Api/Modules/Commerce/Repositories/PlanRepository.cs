using BAVES.Api.Modules.Commerce.Data;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Models.Enums;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Commerce.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly CommerceDbContext _db;

    public PlanRepository(CommerceDbContext db)
    {
        _db = db;
    }

    public Task<List<Plan>> GetPublishedPlansAsync(CancellationToken ct = default)
    {
        return _db.Plans
            .Include(p => p.Scopes)
            .Where(p => p.Status == PlanStatus.Published)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(ct);
    }

    public Task<Plan?> GetByIdWithScopesAsync(int id, CancellationToken ct = default)
    {
        return _db.Plans
            .Include(p => p.Scopes)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public Task<Plan?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return _db.Plans.FirstOrDefaultAsync(p => p.Id == id, ct);
    }
}
