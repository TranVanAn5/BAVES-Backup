using BAVES.Api.Modules.Commerce.Data;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Models.Enums;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Commerce.Repositories;

public class EntitlementRepository : IEntitlementRepository
{
    private readonly CommerceDbContext _db;

    public EntitlementRepository(CommerceDbContext db)
    {
        _db = db;
    }

    public Task<List<Entitlement>> GetActiveByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return _db.Entitlements
            .Where(e => e.UserId == userId && e.Status == EntitlementStatus.Active)
            .ToListAsync(ct);
    }

    public Task<List<Entitlement>> GetAllByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return _db.Entitlements
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Entitlement entitlement, CancellationToken ct = default)
    {
        await _db.Entitlements.AddAsync(entitlement, ct);
    }

    public Task AddRangeAsync(IEnumerable<Entitlement> entitlements, CancellationToken ct = default)
    {
        return _db.Entitlements.AddRangeAsync(entitlements, ct);
    }

    public Task UpdateAsync(Entitlement entitlement, CancellationToken ct = default)
    {
        _db.Entitlements.Update(entitlement);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}
