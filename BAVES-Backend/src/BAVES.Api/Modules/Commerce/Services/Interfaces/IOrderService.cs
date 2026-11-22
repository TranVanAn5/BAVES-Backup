using BAVES.Api.Modules.Commerce.Models.DTOs;

namespace BAVES.Api.Modules.Commerce.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request, string? idempotencyKey, CancellationToken ct = default);
    Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId, CancellationToken ct = default);
    Task<List<OrderDto>> GetUserOrdersAsync(int userId, CancellationToken ct = default);
    Task<bool> CancelOrderAsync(int orderId, int userId, CancellationToken ct = default);
}
