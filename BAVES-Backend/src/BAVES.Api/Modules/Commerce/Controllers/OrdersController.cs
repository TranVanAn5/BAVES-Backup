using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Services.Interfaces;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Commerce.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
[Tags("Commerce - Orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Tạo đơn hàng mua gói mô phỏng và nhận link thanh toán PayOS.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(new { message = "Vui lòng đăng nhập để tiếp tục." });
        }

        try
        {
            var order = await _orderService.CreateOrderAsync(userId.Value, request, idempotencyKey, ct);
            return Ok(order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Lấy trạng thái đơn hàng theo ID (Dùng cho giao diện Polling sau khi thanh toán).
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var order = await _orderService.GetOrderByIdAsync(id, userId.Value, ct);
        if (order == null)
        {
            return NotFound(new { message = $"Không tìm thấy đơn hàng ID: {id}" });
        }
        return Ok(order);
    }

    /// <summary>
    /// Lịch sử đơn hàng của người dùng hiện tại.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var orders = await _orderService.GetUserOrdersAsync(userId.Value, ct);
        return this.OkPagedAction(orders, page, pageSize);
    }

    /// <summary>
    /// Hủy đơn hàng đang chờ thanh toán.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CancelOrder(int id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var success = await _orderService.CancelOrderAsync(id, userId.Value, ct);
        if (!success)
        {
            return BadRequest(new { message = "Không thể hủy đơn hàng (đơn không tồn tại hoặc đã xử lý)." });
        }
        return Ok(new { message = "Hủy đơn hàng thành công." });
    }

    private int? GetCurrentUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(sub, out var id) ? id : null;
    }
}
