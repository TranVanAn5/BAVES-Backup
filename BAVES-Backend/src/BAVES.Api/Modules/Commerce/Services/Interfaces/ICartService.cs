using BAVES.Api.Modules.Commerce.Models.DTOs;

namespace BAVES.Api.Modules.Commerce.Services.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId, CancellationToken ct = default);
    Task<CartDto> AddItemAsync(int userId, AddCartItemRequest request, CancellationToken ct = default);
    Task<CartDto> RemoveItemAsync(int userId, int itemId, CancellationToken ct = default);
    Task<CartDto> ClearAsync(int userId, CancellationToken ct = default);
    Task<OrderDto> CheckoutAsync(int userId, CheckoutCartRequest request, string? idempotencyKey, CancellationToken ct = default);
}
