using BAVES.Api.Modules.Commerce.Models.Entities;

namespace BAVES.Api.Modules.Commerce.Repositories.Interfaces;

public interface ICreditLedgerRepository
{
    Task<int> GetCreditBalanceAsync(int userId, CancellationToken ct = default);
    Task<int> GetCreditBalanceAsync(int userId, string creditType, CancellationToken ct = default);
    Task<List<CreditLedger>> GetCreditLedgerHistoryAsync(int userId, CancellationToken ct = default);
    Task AddAsync(CreditLedger entry, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
