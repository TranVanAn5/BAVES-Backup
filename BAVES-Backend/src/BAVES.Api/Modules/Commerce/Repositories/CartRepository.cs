using BAVES.Api.Modules.Commerce.Data;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Commerce.Repositories;

public sealed class CartRepository(CommerceDbContext db) : ICartRepository
{
    public Task<Cart?> GetByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return db.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Plan)
            .ThenInclude(p => p.Scopes)
            .FirstOrDefaultAsync(c => c.UserId == userId, ct);
    }

    public async Task<Cart> GetOrCreateAsync(int userId, CancellationToken ct = default)
    {
        var cart = await GetByUserIdAsync(userId, ct);
        if (cart is not null) return cart;

        cart = new Cart { UserId = userId, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow };
        await db.Carts.AddAsync(cart, ct);
        await db.SaveChangesAsync(ct);
        return await GetByUserIdAsync(userId, ct) ?? cart;
    }

    public async Task<CartItem?> GetItemAsync(int userId, int itemId, CancellationToken ct = default)
    {
        return await db.CartItems
            .Include(i => i.Cart)
            .Include(i => i.Plan)
            .ThenInclude(p => p.Scopes)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.Cart.UserId == userId, ct);
    }

    public async Task AddItemAsync(CartItem item, CancellationToken ct = default)
    {
        await db.CartItems.AddAsync(item, ct);
    }

    public Task RemoveItemAsync(CartItem item, CancellationToken ct = default)
    {
        db.CartItems.Remove(item);
        return Task.CompletedTask;
    }

    public Task ClearItemsAsync(Cart cart, CancellationToken ct = default)
    {
        db.CartItems.RemoveRange(cart.Items);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
