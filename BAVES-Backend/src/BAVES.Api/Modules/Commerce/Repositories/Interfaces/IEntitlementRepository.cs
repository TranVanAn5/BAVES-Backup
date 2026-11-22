using BAVES.Api.Modules.Commerce.Models.Entities;

namespace BAVES.Api.Modules.Commerce.Repositories.Interfaces;

public interface IEntitlementRepository
{
    Task<List<Entitlement>> GetActiveByUserIdAsync(int userId, CancellationToken ct = default);
    Task<List<Entitlement>> GetAllByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(Entitlement entitlement, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Entitlement> entitlements, CancellationToken ct = default);
    Task UpdateAsync(Entitlement entitlement, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
