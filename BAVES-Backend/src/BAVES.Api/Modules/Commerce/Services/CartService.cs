using System.Text.Json;
using BAVES.Api.Modules.Commerce.Gateways;
using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Models.Enums;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using BAVES.Api.Modules.Commerce.Services.Interfaces;

namespace BAVES.Api.Modules.Commerce.Services;

public sealed class CartService(
    ICartRepository carts,
    IPlanRepository plans,
    IOrderRepository orders,
    IEntitlementRepository entitlements,
    IPaymentGateway payments,
    ICreditLedgerRepository? credits = null) : ICartService
{
    public async Task<CartDto> GetCartAsync(int userId, CancellationToken ct = default)
    {
        var cart = await carts.GetOrCreateAsync(userId, ct);
        return Map(cart);
    }

    public async Task<CartDto> AddItemAsync(int userId, AddCartItemRequest request, CancellationToken ct = default)
    {
        var plan = await plans.GetByIdWithScopesAsync(request.PlanId, ct)
            ?? throw new KeyNotFoundException($"Không tìm thấy gói ID: {request.PlanId}");
        if (plan.Status != PlanStatus.Published)
            throw new InvalidOperationException("Gói này hiện không mở bán.");

        ValidateTargetScope(plan, request.TargetScopeId);

        var cart = await carts.GetOrCreateAsync(userId, ct);
        var dup = cart.Items.FirstOrDefault(i =>
            i.PlanId == request.PlanId && i.TargetScopeId == request.TargetScopeId);
        if (dup is not null)
            throw new InvalidOperationException("Mục này đã có trong giỏ hàng.");

        await carts.AddItemAsync(new CartItem
        {
            CartId = cart.Id,
            PlanId = plan.Id,
            TargetScopeId = request.TargetScopeId,
            TargetLabel = string.IsNullOrWhiteSpace(request.TargetLabel) ? null : request.TargetLabel.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        }, ct);
        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await carts.SaveChangesAsync(ct);

        var refreshed = await carts.GetByUserIdAsync(userId, ct) ?? cart;
        return Map(refreshed);
    }

    public async Task<CartDto> RemoveItemAsync(int userId, int itemId, CancellationToken ct = default)
    {
        var item = await carts.GetItemAsync(userId, itemId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy mục trong giỏ.");
        await carts.RemoveItemAsync(item, ct);
        var cart = await carts.GetOrCreateAsync(userId, ct);
        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await carts.SaveChangesAsync(ct);
        var refreshed = await carts.GetByUserIdAsync(userId, ct) ?? cart;
        return Map(refreshed);
    }

    public async Task<CartDto> ClearAsync(int userId, CancellationToken ct = default)
    {
        var cart = await carts.GetOrCreateAsync(userId, ct);
        await carts.ClearItemsAsync(cart, ct);
        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await carts.SaveChangesAsync(ct);
        var refreshed = await carts.GetByUserIdAsync(userId, ct) ?? cart;
        return Map(refreshed);
    }

    public async Task<OrderDto> CheckoutAsync(
        int userId,
        CheckoutCartRequest request,
        string? idempotencyKey,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existing = await orders.GetByIdempotencyKeyAsync(userId, idempotencyKey, ct);
            if (existing is not null) return OrderService.MapOrderDto(existing);
        }

        var cart = await carts.GetOrCreateAsync(userId, ct);
        if (cart.Items.Count == 0)
            throw new InvalidOperationException("Giỏ hàng trống.");

        var linePlans = new List<(CartItem Item, Plan Plan)>();
        foreach (var item in cart.Items.OrderBy(i => i.CreatedAt))
        {
            var plan = item.Plan.Scopes.Count > 0
                ? item.Plan
                : await plans.GetByIdWithScopesAsync(item.PlanId, ct)
                  ?? throw new KeyNotFoundException($"Gói {item.PlanId} không tồn tại.");
            if (plan.Status != PlanStatus.Published)
                throw new InvalidOperationException($"Gói {plan.Code} không còn mở bán.");
            ValidateTargetScope(plan, item.TargetScopeId);
            linePlans.Add((item, plan));
        }

        var now = DateTimeOffset.UtcNow;
        var total = linePlans.Sum(x => x.Plan.Price);
        var first = linePlans[0];
        var orderCode = OrderService.NewOrderCode();

        var order = new Order
        {
            OrderCode = orderCode,
            UserId = userId,
            PlanId = first.Plan.Id,
            Plan = first.Plan,
            Amount = total,
            Currency = first.Plan.Currency,
            PlanSnapshotJson = JsonSerializer.Serialize(new
            {
                multiItem = true,
                itemCount = linePlans.Count,
                total,
                lines = linePlans.Select(x => new
                {
                    planId = x.Plan.Id,
                    planCode = x.Plan.Code,
                    targetScopeId = x.Item.TargetScopeId,
                    targetLabel = x.Item.TargetLabel,
                    unitPrice = x.Plan.Price
                })
            }),
            Status = OrderStatus.Pending,
            IdempotencyKey = idempotencyKey,
            CreatedAt = now,
            ExpiresAt = now.AddMinutes(15)
        };

        var display = 0;
        foreach (var (item, plan) in linePlans)
        {
            order.Lines.Add(new OrderLine
            {
                PlanId = plan.Id,
                Plan = plan,
                TargetScopeId = item.TargetScopeId,
                TargetLabel = item.TargetLabel,
                UnitPrice = plan.Price,
                Currency = plan.Currency,
                PlanCode = plan.Code,
                PlanName = plan.Name,
                DisplayOrder = display++,
                LineSnapshotJson = JsonSerializer.Serialize(new
                {
                    plan.Id,
                    plan.Code,
                    plan.Name,
                    plan.Price,
                    plan.DurationDays,
                    plan.SessionQuota,
                    plan.ReviewCredits,
                    plan.ConsultationCredits,
                    targetScopeId = item.TargetScopeId,
                    targetLabel = item.TargetLabel
                })
            });
        }

        var returnUrl = request.ReturnUrl ?? "http://localhost:5173/#/payment";
        var cancelUrl = request.CancelUrl ?? "http://localhost:5173/#/payment";

        if (total == 0)
        {
            order.Status = OrderStatus.Paid;
            order.PaidAt = now;
            order.CheckoutUrl = returnUrl;
            await orders.AddAsync(order, ct);
            await orders.SaveChangesAsync(ct);

            foreach (var (item, plan) in linePlans)
            {
                await EntitlementGrantHelper.GrantPlanAsync(
                    userId, order.Id, plan, item.TargetScopeId, now, entitlements, credits, ct);
            }
            await entitlements.SaveChangesAsync(ct);
        }
        else
        {
            await orders.AddAsync(order, ct);
            await orders.SaveChangesAsync(ct);
            var link = await payments.CreatePaymentLinkAsync(order, returnUrl, cancelUrl, ct);
            order.CheckoutUrl = link.CheckoutUrl;
            order.QrCode = link.QrCode;
            await orders.UpdateAsync(order, ct);
            await orders.SaveChangesAsync(ct);
        }

        await carts.ClearItemsAsync(cart, ct);
        cart.UpdatedAt = now;
        await carts.SaveChangesAsync(ct);

        var saved = await orders.GetByOrderCodeAsync(orderCode, ct) ?? order;
        return OrderService.MapOrderDto(saved);
    }

    private static void ValidateTargetScope(Plan plan, int? targetScopeId)
    {
        foreach (var scope in plan.Scopes)
        {
            if (OrderService.RequiresTargetScope(scope) && OrderService.ResolveScopeId(scope, targetScopeId) is null)
            {
                throw new InvalidOperationException(
                    "Gói này cần chọn kịch bản / Application / Industry cụ thể trước khi thêm vào giỏ.");
            }
        }
    }

    private static CartDto Map(Cart cart)
    {
        var items = cart.Items
            .OrderBy(i => i.CreatedAt)
            .Select(i =>
            {
                var scopeType = i.Plan.Scopes.FirstOrDefault()?.ScopeType.ToString();
                return new CartItemDto(
                    i.Id,
                    i.PlanId,
                    i.Plan.Code,
                    i.Plan.Name,
                    i.Plan.Price,
                    i.Plan.Currency,
                    i.TargetScopeId,
                    i.TargetLabel,
                    scopeType,
                    i.CreatedAt);
            })
            .ToList();

        return new CartDto(
            cart.Id,
            cart.UserId,
            items,
            items.Sum(i => i.UnitPrice),
            items.FirstOrDefault()?.Currency ?? "VND",
            cart.UpdatedAt);
    }
}
