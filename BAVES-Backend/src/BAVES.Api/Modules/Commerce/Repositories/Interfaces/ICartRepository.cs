using BAVES.Api.Modules.Commerce.Models.Entities;

namespace BAVES.Api.Modules.Commerce.Repositories.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId, CancellationToken ct = default);
    Task<Cart> GetOrCreateAsync(int userId, CancellationToken ct = default);
    Task<CartItem?> GetItemAsync(int userId, int itemId, CancellationToken ct = default);
    Task AddItemAsync(CartItem item, CancellationToken ct = default);
    Task RemoveItemAsync(CartItem item, CancellationToken ct = default);
    Task ClearItemsAsync(Cart cart, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
