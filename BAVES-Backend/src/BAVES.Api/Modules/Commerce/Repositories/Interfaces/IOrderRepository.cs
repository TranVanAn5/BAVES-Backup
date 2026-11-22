using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace BAVES.Api.Modules.Commerce.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int orderId, int userId, CancellationToken ct = default);
    Task<Order?> GetByOrderCodeAsync(long orderCode, CancellationToken ct = default);
    Task<Order?> GetByIdempotencyKeyAsync(int userId, string idempotencyKey, CancellationToken ct = default);
    Task<List<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
