using BAVES.Api.Modules.Commerce.Data;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Commerce.Repositories;

public class CreditLedgerRepository : ICreditLedgerRepository
{
    private readonly CommerceDbContext _db;

    public CreditLedgerRepository(CommerceDbContext db)
    {
        _db = db;
    }

    public async Task<int> GetCreditBalanceAsync(int userId, CancellationToken ct = default)
    {
        return await GetCreditBalanceAsync(userId, "Review", ct);
    }

    public async Task<int> GetCreditBalanceAsync(int userId, string creditType, CancellationToken ct = default)
    {
        var total = await _db.CreditLedgers
            .Where(c => c.UserId == userId && c.CreditType == creditType)
            .SumAsync(c => (int?)c.Delta, ct);

        return Math.Max(0, total ?? 0);
    }

    public async Task<List<CreditLedger>> GetCreditLedgerHistoryAsync(int userId, CancellationToken ct = default)
    {
        return await _db.CreditLedgers
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(CreditLedger entry, CancellationToken ct = default)
    {
        await _db.CreditLedgers.AddAsync(entry, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}
