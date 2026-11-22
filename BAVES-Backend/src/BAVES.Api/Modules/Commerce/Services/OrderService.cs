using System.Text.Json;
using BAVES.Api.Modules.Commerce.Gateways;
using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Models.Enums;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using BAVES.Api.Modules.Commerce.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BAVES.Api.Modules.Commerce.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IEntitlementRepository _entitlementRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<OrderService> _logger;
    private readonly ICreditLedgerRepository? _creditLedgerRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IPlanRepository planRepository,
        IEntitlementRepository entitlementRepository,
        IPaymentGateway paymentGateway,
        ILogger<OrderService> logger,
        ICreditLedgerRepository? creditLedgerRepository = null)
    {
        _orderRepository = orderRepository;
        _planRepository = planRepository;
        _entitlementRepository = entitlementRepository;
        _paymentGateway = paymentGateway;
        _logger = logger;
        _creditLedgerRepository = creditLedgerRepository;
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request, string? idempotencyKey, CancellationToken ct = default)
    {
        // 1. Kiểm tra idempotency nếu có
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existingOrder = await _orderRepository.GetByIdempotencyKeyAsync(userId, idempotencyKey, ct);
            if (existingOrder != null)
            {
                return MapToDto(existingOrder);
            }
        }

        // 2. Tìm gói cước từ PlanRepository
        var plan = await _planRepository.GetByIdWithScopesAsync(request.PlanId, ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy gói cước ID: {request.PlanId}");

        if (plan.Status != PlanStatus.Published)
        {
            throw new InvalidOperationException("Gói cước này hiện không còn mở bán.");
        }

        // 3. Tạo mã OrderCode ngẫu nhiên duy nhất cho PayOS
        var orderCode = NewOrderCode();
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(15);

        var snapshotJson = JsonSerializer.Serialize(new
        {
            plan.Id,
            plan.Code,
            plan.Name,
            plan.Price,
            plan.DurationDays,
            plan.SessionQuota,
            plan.ReviewCredits,
            plan.ConsultationCredits,
            targetScopeId = request.TargetScopeId
        });

        var returnUrl = request.ReturnUrl ?? "http://localhost:5173/#/payment";
        var cancelUrl = request.CancelUrl ?? "http://localhost:5173/#/payment";

        var order = new Order
        {
            OrderCode = orderCode,
            UserId = userId,
            PlanId = plan.Id,
            Plan = plan,
            Amount = plan.Price,
            Currency = plan.Currency,
            PlanSnapshotJson = snapshotJson,
            Status = OrderStatus.Pending,
            IdempotencyKey = idempotencyKey,
            CreatedAt = now,
            ExpiresAt = expiresAt
        };
        order.Lines.Add(new OrderLine
        {
            PlanId = plan.Id,
            Plan = plan,
            TargetScopeId = request.TargetScopeId,
            UnitPrice = plan.Price,
            Currency = plan.Currency,
            PlanCode = plan.Code,
            PlanName = plan.Name,
            DisplayOrder = 0,
            LineSnapshotJson = snapshotJson
        });

        // Gói Scenario/Application/Industry với ScopeId null bắt buộc có TargetScopeId
        foreach (var scope in plan.Scopes)
        {
            if (RequiresTargetScope(scope) && ResolveScopeId(scope, request.TargetScopeId) is null)
            {
                throw new InvalidOperationException(
                    "Gói này cần chọn kịch bản / Application / Industry cụ thể trước khi thanh toán.");
            }
        }

        // Nếu là gói miễn phí (Price == 0), tự động hoàn tất luôn
        if (plan.Price == 0)
        {
            order.Status = OrderStatus.Paid;
            order.PaidAt = now;
            order.CheckoutUrl = returnUrl;

            await _orderRepository.AddAsync(order, ct);
            await _orderRepository.SaveChangesAsync(ct);

            IEnumerable<PlanScope> scopes = plan.Scopes.Count > 0
                ? plan.Scopes
                : new[] { new PlanScope { ScopeType = ScopeType.Scenario } };
            foreach (var scope in scopes)
            {
                var resolvedScopeId = ResolveScopeId(scope, request.TargetScopeId);
                if (RequiresTargetScope(scope) && resolvedScopeId is null)
                {
                    throw new InvalidOperationException(
                        "Gói này cần chọn kịch bản / Application / Industry cụ thể trước khi thanh toán.");
                }

                await _entitlementRepository.AddAsync(new Entitlement
                {
                    UserId = userId,
                    SourceType = EntitlementSourceType.Free,
                    SourceOrderId = order.Id,
                    ScopeType = scope.ScopeType,
                    ScopeId = resolvedScopeId,
                    StartsAt = now,
                    EndsAt = plan.DurationDays.HasValue ? now.AddDays(plan.DurationDays.Value) : null,
                    SessionQuota = plan.SessionQuota,
                    SessionsUsed = 0,
                    Status = EntitlementStatus.Active,
                    CreatedAt = now
                }, ct);
            }

            await GrantPlanCreditsAsync(userId, order.Id, plan, now, ct);
            await _entitlementRepository.SaveChangesAsync(ct);

            return MapToDto(order);
        }

        // 4. Gọi cổng thanh toán sinh payment link
        var linkResult = await _paymentGateway.CreatePaymentLinkAsync(order, returnUrl, cancelUrl, ct);
        order.CheckoutUrl = linkResult.CheckoutUrl;
        order.QrCode = linkResult.QrCode;

        await _orderRepository.AddAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);

        return MapToDto(order);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId, int userId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, userId, ct);
        if (order == null) return null;

        // Tự động chuyển Expired nếu đã quá hạn mà vẫn Pending
        if (order.Status == OrderStatus.Pending && DateTimeOffset.UtcNow > order.ExpiresAt)
        {
            order.Status = OrderStatus.Expired;
            await _orderRepository.UpdateAsync(order, ct);
            await _orderRepository.SaveChangesAsync(ct);
        }

        return MapToDto(order);
    }

    public async Task<List<OrderDto>> GetUserOrdersAsync(int userId, CancellationToken ct = default)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId, ct);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<bool> CancelOrderAsync(int orderId, int userId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, userId, ct);
        if (order == null || order.Status != OrderStatus.Pending)
        {
            return false;
        }

        order.Status = OrderStatus.Cancelled;
        order.CancelledAt = DateTimeOffset.UtcNow;
        await _orderRepository.UpdateAsync(order, ct);
        await _orderRepository.SaveChangesAsync(ct);
        return true;
    }

    private async Task GrantPlanCreditsAsync(int userId, int orderId, Plan plan, DateTimeOffset now, CancellationToken ct)
    {
        if (_creditLedgerRepository is null)
        {
            return;
        }

        if (plan.ReviewCredits > 0)
        {
            await _creditLedgerRepository.AddAsync(new CreditLedger
            {
                UserId = userId,
                Delta = plan.ReviewCredits,
                CreditType = "Review",
                Reason = CreditReason.Purchase,
                OrderId = orderId,
                Note = $"Cộng {plan.ReviewCredits} lượt chấm bài từ đơn #{orderId}",
                CreatedAt = now
            }, ct);
        }

        if (plan.ConsultationCredits > 0)
        {
            await _creditLedgerRepository.AddAsync(new CreditLedger
            {
                UserId = userId,
                Delta = plan.ConsultationCredits,
                CreditType = "Consultation",
                Reason = CreditReason.Purchase,
                OrderId = orderId,
                Note = $"Cộng {plan.ConsultationCredits} buổi tư vấn từ đơn #{orderId}",
                CreatedAt = now
            }, ct);
        }
    }

    internal static long NewOrderCode()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var randomSuffix = Random.Shared.Next(100, 999);
        return timestamp * 1000 + randomSuffix;
    }

    internal static bool RequiresTargetScope(PlanScope scope) =>
        scope.ScopeId is null
        && scope.ScopeType is ScopeType.Scenario or ScopeType.Application or ScopeType.Industry;

    internal static int? ResolveScopeId(PlanScope scope, int? targetScopeId) =>
        scope.ScopeId ?? targetScopeId;

    internal static OrderDto MapOrderDto(Order o) =>
        new(
            o.Id,
            o.OrderCode,
            o.UserId,
            o.PlanId,
            o.Plan?.Name ?? (o.Lines.Count > 1 ? $"Giỏ hàng ({o.Lines.Count} mục)" : "Gói mô phỏng"),
            o.Amount,
            o.Currency,
            o.Status,
            o.CheckoutUrl,
            o.QrCode,
            o.CreatedAt,
            o.ExpiresAt,
            o.PaidAt,
            o.Lines
                .OrderBy(l => l.DisplayOrder)
                .Select(l => new OrderLineDto(
                    l.Id,
                    l.PlanId,
                    l.PlanCode,
                    l.PlanName,
                    l.UnitPrice,
                    l.Currency,
                    l.TargetScopeId,
                    l.TargetLabel,
                    l.DisplayOrder))
                .ToList()
        );

    private static OrderDto MapToDto(Order o) => MapOrderDto(o);
}
