using System.Text.Json;
using BAVES.Api.Modules.Commerce.Data;
using BAVES.Api.Modules.Commerce.Gateways;
using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Models.Enums;
using BAVES.Api.Modules.Commerce.Repositories;
using BAVES.Api.Modules.Commerce.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BAVES.Commerce.Tests;

public class CommerceTests
{
    private CommerceDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<CommerceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var db = new CommerceDbContext(options);

        // Seed 2 plans for testing
        db.Plans.AddRange(
            new Plan
            {
                Id = 1,
                Code = "PLAN-FREE",
                Name = "Gói Dùng thử Miễn phí",
                PlanType = PlanType.Free,
                Price = 0,
                DurationDays = 14,
                SessionQuota = 1,
                ReviewCredits = 0,
                Status = PlanStatus.Published,
                Scopes = new List<PlanScope>
                {
                    new() { Id = 1, PlanId = 1, ScopeType = ScopeType.Scenario, ScopeId = 1 }
                }
            },
            new Plan
            {
                Id = 2,
                Code = "PLAN-OMNIMART",
                Name = "Gói Kịch bản OmniMart",
                PlanType = PlanType.ScenarioBundle,
                Price = 199000,
                DurationDays = 30,
                SessionQuota = 10,
                ReviewCredits = 1,
                Status = PlanStatus.Published,
                Scopes = new List<PlanScope>
                {
                    new() { Id = 2, PlanId = 2, ScopeType = ScopeType.Scenario, ScopeId = 1 }
                }
            }
        );
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task CreateOrder_FreePlan_AutoCompletesAndGrantsEntitlement()
    {
        using var db = CreateInMemoryDbContext();
        var gateway = new MockPaymentGateway();
        var planRepo = new PlanRepository(db);
        var orderRepo = new OrderRepository(db);
        var entRepo = new EntitlementRepository(db);
        var creditRepo = new CreditLedgerRepository(db);

        var orderService = new OrderService(orderRepo, planRepo, entRepo, gateway, NullLogger<OrderService>.Instance);
        var entitlementService = new EntitlementService(entRepo, creditRepo);

        var order = await orderService.CreateOrderAsync(userId: 101, new CreateOrderRequest { PlanId = 1 }, null);

        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.NotNull(order.PaidAt);

        // Verify Entitlement was automatically granted via Repository
        var canStart = await entitlementService.CanStartSessionAsync(userId: 101, scenarioVersionId: 1);
        Assert.True(canStart);
    }

    [Fact]
    public async Task CreateOrder_PaidPlan_GeneratesPendingOrderWithPaymentLink()
    {
        using var db = CreateInMemoryDbContext();
        var gateway = new MockPaymentGateway();
        var planRepo = new PlanRepository(db);
        var orderRepo = new OrderRepository(db);
        var entRepo = new EntitlementRepository(db);

        var orderService = new OrderService(orderRepo, planRepo, entRepo, gateway, NullLogger<OrderService>.Instance);

        var order = await orderService.CreateOrderAsync(userId: 102, new CreateOrderRequest { PlanId = 2 }, null);

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(199000, order.Amount);
        Assert.NotNull(order.CheckoutUrl);
        Assert.Contains(order.OrderCode.ToString(), order.CheckoutUrl);
    }

    [Fact]
    public async Task CreateOrder_Idempotency_ReturnsSameOrderForDuplicateRequest()
    {
        using var db = CreateInMemoryDbContext();
        var gateway = new MockPaymentGateway();
        var planRepo = new PlanRepository(db);
        var orderRepo = new OrderRepository(db);
        var entRepo = new EntitlementRepository(db);

        var orderService = new OrderService(orderRepo, planRepo, entRepo, gateway, NullLogger<OrderService>.Instance);

        var key = "test-idempotency-key-123";
        var order1 = await orderService.CreateOrderAsync(userId: 103, new CreateOrderRequest { PlanId = 2 }, key);
        var order2 = await orderService.CreateOrderAsync(userId: 103, new CreateOrderRequest { PlanId = 2 }, key);

        Assert.Equal(order1.Id, order2.Id);
        Assert.Equal(order1.OrderCode, order2.OrderCode);
    }

    [Fact]
    public async Task Webhook_ValidSignature_CompletesOrderAndGrantsEntitlementsAndCredits()
    {
        using var db = CreateInMemoryDbContext();
        var gateway = new MockPaymentGateway();
        var planRepo = new PlanRepository(db);
        var orderRepo = new OrderRepository(db);
        var entRepo = new EntitlementRepository(db);
        var creditRepo = new CreditLedgerRepository(db);
        var webhookRepo = new PaymentWebhookRepository(db);

        var orderService = new OrderService(orderRepo, planRepo, entRepo, gateway, NullLogger<OrderService>.Instance);
        var webhookService = new PaymentWebhookService(orderRepo, webhookRepo, entRepo, creditRepo, gateway, NullLogger<PaymentWebhookService>.Instance);
        var entitlementService = new EntitlementService(entRepo, creditRepo);

        // 1. Tạo đơn hàng
        var order = await orderService.CreateOrderAsync(userId: 104, new CreateOrderRequest { PlanId = 2 }, null);
        Assert.Equal(OrderStatus.Pending, order.Status);

        // 2. Giả lập PayOS Webhook
        var payload = new PayOsWebhookPayload
        {
            Code = "00",
            Desc = "success",
            Data = new PayOsWebhookData
            {
                OrderCode = order.OrderCode,
                Amount = 199000,
                Reference = "PAYOS-REF-9988",
                TransactionDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            },
            Signature = "VALID_SIGNATURE"
        };
        var rawBody = JsonSerializer.Serialize(payload);

        var result = await webhookService.ProcessPayOsWebhookAsync(payload, rawBody);
        Assert.True(result.Success);

        // 3. Kiểm tra Order chuyển sang Paid qua Repository
        var updatedOrder = await orderRepo.GetByIdAsync(order.Id, 104);
        Assert.NotNull(updatedOrder);
        Assert.Equal(OrderStatus.Paid, updatedOrder.Status);

        // 4. Kiểm tra Entitlement đã mở khóa qua Repository
        var canStart = await entitlementService.CanStartSessionAsync(userId: 104, scenarioVersionId: 1);
        Assert.True(canStart);

        // 5. Kiểm tra Review Credit được cộng 1 lượt qua Repository
        var credits = await entitlementService.GetReviewCreditsAsync(userId: 104);
        Assert.Equal(1, credits);
    }

    [Fact]
    public async Task Webhook_InvalidSignature_IsRejected()
    {
        using var db = CreateInMemoryDbContext();
        var gateway = new MockPaymentGateway();
        var planRepo = new PlanRepository(db);
        var orderRepo = new OrderRepository(db);
        var entRepo = new EntitlementRepository(db);
        var creditRepo = new CreditLedgerRepository(db);
        var webhookRepo = new PaymentWebhookRepository(db);

        var orderService = new OrderService(orderRepo, planRepo, entRepo, gateway, NullLogger<OrderService>.Instance);
        var webhookService = new PaymentWebhookService(orderRepo, webhookRepo, entRepo, creditRepo, gateway, NullLogger<PaymentWebhookService>.Instance);

        var order = await orderService.CreateOrderAsync(userId: 105, new CreateOrderRequest { PlanId = 2 }, null);

        var payload = new PayOsWebhookPayload
        {
            Code = "00",
            Desc = "success",
            Data = new PayOsWebhookData
            {
                OrderCode = order.OrderCode,
                Amount = 199000
            },
            Signature = "INVALID_TEST_SIGNATURE"
        };
        var rawBody = JsonSerializer.Serialize(payload);

        var result = await webhookService.ProcessPayOsWebhookAsync(payload, rawBody);
        Assert.False(result.Success);

        var unchangedOrder = await orderRepo.GetByIdAsync(order.Id, 105);
        Assert.Equal(OrderStatus.Pending, unchangedOrder!.Status);
    }

    [Fact]
    public async Task Entitlement_QuotaExceeded_BlocksSession()
    {
        using var db = CreateInMemoryDbContext();
        var entRepo = new EntitlementRepository(db);
        var creditRepo = new CreditLedgerRepository(db);
        var entitlementService = new EntitlementService(entRepo, creditRepo);

        // Tạo 1 entitlement có quota = 1 phiên qua Repository
        await entRepo.AddAsync(new Entitlement
        {
            UserId = 106,
            SourceType = EntitlementSourceType.Purchase,
            ScopeType = ScopeType.Scenario,
            ScopeId = 1,
            SessionQuota = 1,
            SessionsUsed = 0,
            Status = EntitlementStatus.Active
        });
        await entRepo.SaveChangesAsync();

        // Lần 1: Cho phép bắt đầu phiên
        var canStart1 = await entitlementService.CanStartSessionAsync(userId: 106, scenarioVersionId: 1);
        Assert.True(canStart1);

        // Ghi nhận đã dùng 1 phiên
        await entitlementService.RecordSessionUsedAsync(userId: 106, scenarioVersionId: 1);

        // Lần 2: Quota đã hết, bị chặn
        var canStart2 = await entitlementService.CanStartSessionAsync(userId: 106, scenarioVersionId: 1);
        Assert.False(canStart2);
    }

    [Fact]
    public async Task CreditLedger_ConsumeCredit_DecrementsBalanceAndBlocksWhenZero()
    {
        using var db = CreateInMemoryDbContext();
        var entRepo = new EntitlementRepository(db);
        var creditRepo = new CreditLedgerRepository(db);
        var entitlementService = new EntitlementService(entRepo, creditRepo);

        // Ban đầu chưa có credit
        var c0 = await entitlementService.GetReviewCreditsAsync(userId: 107);
        Assert.Equal(0, c0);

        var consumeFail = await entitlementService.ConsumeReviewCreditAsync(userId: 107, reviewRequestId: 1);
        Assert.False(consumeFail);

        // Nạp 1 credit qua Repository
        await creditRepo.AddAsync(new CreditLedger
        {
            UserId = 107,
            Delta = 1,
            Reason = CreditReason.Purchase,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await creditRepo.SaveChangesAsync();

        var c1 = await entitlementService.GetReviewCreditsAsync(userId: 107);
        Assert.Equal(1, c1);

        // Trừ 1 credit
        var consumeSuccess = await entitlementService.ConsumeReviewCreditAsync(userId: 107, reviewRequestId: 1);
        Assert.True(consumeSuccess);

        var c2 = await entitlementService.GetReviewCreditsAsync(userId: 107);
        Assert.Equal(0, c2);
    }

    [Fact]
    public async Task GetPublishedPlans_ReturnsOrderedPublishedPlansWithScopes()
    {
        using var db = CreateInMemoryDbContext();
        var planRepo = new PlanRepository(db);
        var planService = new PlanService(planRepo);

        var plans = await planService.GetPublishedPlansAsync();

        Assert.Equal(2, plans.Count);
        Assert.Contains(plans, p => p.Code == "PLAN-FREE");
        Assert.Contains(plans, p => p.Code == "PLAN-OMNIMART");
        Assert.All(plans, p => Assert.NotEmpty(p.Scopes));
    }

    [Fact]
    public async Task Webhook_AmountMismatch_RejectsPaymentAndDoesNotGrantCredits()
    {
        using var db = CreateInMemoryDbContext();
        var gateway = new MockPaymentGateway();
        var planRepo = new PlanRepository(db);
        var orderRepo = new OrderRepository(db);
        var entRepo = new EntitlementRepository(db);
        var creditRepo = new CreditLedgerRepository(db);
        var webhookRepo = new PaymentWebhookRepository(db);

        var orderService = new OrderService(orderRepo, planRepo, entRepo, gateway, NullLogger<OrderService>.Instance);
        var webhookService = new PaymentWebhookService(orderRepo, webhookRepo, entRepo, creditRepo, gateway, NullLogger<PaymentWebhookService>.Instance);

        var order = await orderService.CreateOrderAsync(userId: 108, new CreateOrderRequest { PlanId = 2 }, null);

        // Gửi amount 50,000 VND thay vì 199,000 VND
        var payload = new PayOsWebhookPayload
        {
            Code = "00",
            Desc = "success",
            Data = new PayOsWebhookData
            {
                OrderCode = order.OrderCode,
                Amount = 50000,
                Reference = "PAYOS-REF-MISMATCH",
                TransactionDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            },
            Signature = "VALID_SIGNATURE"
        };
        var rawBody = JsonSerializer.Serialize(payload);

        var result = await webhookService.ProcessPayOsWebhookAsync(payload, rawBody);
        Assert.False(result.Success);
        Assert.Contains("không khớp", result.Message);

        var unchangedOrder = await orderRepo.GetByIdAsync(order.Id, 108);
        Assert.Equal(OrderStatus.Pending, unchangedOrder!.Status);
    }

    [Fact]
    public async Task Webhook_PlanWithConsultationCredits_ProvisionsBothReviewAndConsultationCredits()
    {
        using var db = CreateInMemoryDbContext();
        var gateway = new MockPaymentGateway();
        var planRepo = new PlanRepository(db);
        var orderRepo = new OrderRepository(db);
        var entRepo = new EntitlementRepository(db);
        var creditRepo = new CreditLedgerRepository(db);
        var webhookRepo = new PaymentWebhookRepository(db);

        // Seed Gói 5: 1 Review + 1 Consultation
        var premiumPlan = new Plan
        {
            Id = 5,
            Code = "PLAN-PREMIUM-CONSULT",
            Name = "Gói Chấm bài & Trao đổi 1-on-1",
            PlanType = PlanType.Consultation,
            Price = 199000,
            SessionQuota = 0,
            ReviewCredits = 1,
            ConsultationCredits = 1,
            Status = PlanStatus.Published
        };
        db.Plans.Add(premiumPlan);
        await db.SaveChangesAsync();

        var orderService = new OrderService(orderRepo, planRepo, entRepo, gateway, NullLogger<OrderService>.Instance);
        var webhookService = new PaymentWebhookService(orderRepo, webhookRepo, entRepo, creditRepo, gateway, NullLogger<PaymentWebhookService>.Instance);
        var entitlementService = new EntitlementService(entRepo, creditRepo);

        var order = await orderService.CreateOrderAsync(userId: 109, new CreateOrderRequest { PlanId = 5 }, null);

        var payload = new PayOsWebhookPayload
        {
            Code = "00",
            Desc = "success",
            Data = new PayOsWebhookData
            {
                OrderCode = order.OrderCode,
                Amount = 199000,
                Reference = "PAYOS-REF-PREMIUM",
                TransactionDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            },
            Signature = "VALID_SIGNATURE"
        };
        var rawBody = JsonSerializer.Serialize(payload);

        var result = await webhookService.ProcessPayOsWebhookAsync(payload, rawBody);
        Assert.True(result.Success);

        // Kiểm tra nhận cả Review Credit và Consultation Credit
        var reviewCredits = await entitlementService.GetReviewCreditsAsync(userId: 109);
        var consultationCredits = await entitlementService.GetConsultationCreditsAsync(userId: 109);

        Assert.Equal(1, reviewCredits);
        Assert.Equal(1, consultationCredits);
    }

    [Fact]
    public async Task CancelOrder_PendingOrder_SucceedsAndCannotCancelPaidOrder()
    {
        using var db = CreateInMemoryDbContext();
        var gateway = new MockPaymentGateway();
        var planRepo = new PlanRepository(db);
        var orderRepo = new OrderRepository(db);
        var entRepo = new EntitlementRepository(db);

        var orderService = new OrderService(orderRepo, planRepo, entRepo, gateway, NullLogger<OrderService>.Instance);

        // 1. Hủy đơn Pending -> thành công
        var pendingOrder = await orderService.CreateOrderAsync(userId: 110, new CreateOrderRequest { PlanId = 2 }, null);
        var cancelSuccess = await orderService.CancelOrderAsync(pendingOrder.Id, userId: 110);
        Assert.True(cancelSuccess);

        var cancelledOrder = await orderRepo.GetByIdAsync(pendingOrder.Id, 110);
        Assert.Equal(OrderStatus.Cancelled, cancelledOrder!.Status);
        Assert.NotNull(cancelledOrder.CancelledAt);

        // 2. Cố hủy đơn Free đã Paid -> thất bại
        var freeOrder = await orderService.CreateOrderAsync(userId: 110, new CreateOrderRequest { PlanId = 1 }, null);
        var cancelPaid = await orderService.CancelOrderAsync(freeOrder.Id, userId: 110);
        Assert.False(cancelPaid);
    }

    [Fact]
    public async Task GetOrderById_ExpiredOrder_AutomaticallyTransitionsToExpired()
    {
        using var db = CreateInMemoryDbContext();
        var orderRepo = new OrderRepository(db);
        var planRepo = new PlanRepository(db);
        var entRepo = new EntitlementRepository(db);
        var gateway = new MockPaymentGateway();

        var orderService = new OrderService(orderRepo, planRepo, entRepo, gateway, NullLogger<OrderService>.Instance);

        var order = new Order
        {
            Id = 500,
            OrderCode = 9988776655,
            UserId = 111,
            PlanId = 2,
            Amount = 199000,
            Status = OrderStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-15) // Quá hạn 15 phút trước
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var orderDto = await orderService.GetOrderByIdAsync(500, userId: 111);
        Assert.NotNull(orderDto);
        Assert.Equal(OrderStatus.Expired, orderDto.Status);
    }

    [Fact]
    public async Task CreditLedger_GetCreditBalanceSummary_ReturnsCorrectBalancesAndHistory()
    {
        using var db = CreateInMemoryDbContext();
        var entRepo = new EntitlementRepository(db);
        var creditRepo = new CreditLedgerRepository(db);
        var entitlementService = new EntitlementService(entRepo, creditRepo);

        const int userId = 112;

        // Nạp 2 Review credits và 1 Consultation credit
        await creditRepo.AddAsync(new CreditLedger
        {
            UserId = userId,
            Delta = 2,
            CreditType = "Review",
            Reason = CreditReason.Purchase,
            Note = "Nạp review credits"
        });
        await creditRepo.AddAsync(new CreditLedger
        {
            UserId = userId,
            Delta = 1,
            CreditType = "Consultation",
            Reason = CreditReason.Purchase,
            Note = "Nạp consultation credit"
        });
        await creditRepo.SaveChangesAsync();

        // Sử dụng 1 Review credit
        await entitlementService.ConsumeReviewCreditAsync(userId, reviewRequestId: 77);

        // Lấy summary
        var summary = await entitlementService.GetCreditBalanceSummaryAsync(userId);

        Assert.Equal(1, summary.ReviewCreditsBalance);
        Assert.Equal(1, summary.ConsultationCreditsBalance);
        Assert.Equal(3, summary.History.Count);
    }
}
