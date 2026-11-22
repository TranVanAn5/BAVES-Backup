using BAVES.Api.Modules.Commerce.Data;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BAVES.Api.Modules.Commerce.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly CommerceDbContext _db;

    public OrderRepository(CommerceDbContext db)
    {
        _db = db;
    }

    public Task<Order?> GetByIdAsync(int orderId, int userId, CancellationToken ct = default)
    {
        return _db.Orders
            .Include(o => o.Plan)
            .Include(o => o.Lines)
            .ThenInclude(l => l.Plan)
            .ThenInclude(p => p.Scopes)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId, ct);
    }

    public Task<Order?> GetByOrderCodeAsync(long orderCode, CancellationToken ct = default)
    {
        return _db.Orders
            .Include(o => o.Plan)
            .ThenInclude(p => p.Scopes)
            .Include(o => o.Lines)
            .ThenInclude(l => l.Plan)
            .ThenInclude(p => p.Scopes)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode, ct);
    }

    public Task<Order?> GetByIdempotencyKeyAsync(int userId, string idempotencyKey, CancellationToken ct = default)
    {
        return _db.Orders
            .Include(o => o.Plan)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.IdempotencyKey == idempotencyKey, ct);
    }

    public Task<List<Order>> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return _db.Orders
            .Include(o => o.Plan)
            .Include(o => o.Lines)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _db.Orders.AddAsync(order, ct);
    }

    public Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _db.Orders.Update(order);
        return Task.CompletedTask;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        return _db.Database.BeginTransactionAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}
