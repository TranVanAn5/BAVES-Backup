using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BAVES.Api.Modules.Commerce.Gateways;
using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Models.Enums;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using BAVES.Api.Modules.Commerce.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace BAVES.Api.Modules.Commerce.Services;

public class PaymentWebhookService : IPaymentWebhookService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentWebhookRepository _webhookRepository;
    private readonly IEntitlementRepository _entitlementRepository;
    private readonly ICreditLedgerRepository _creditLedgerRepository;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PaymentWebhookService> _logger;

    public PaymentWebhookService(
        IOrderRepository orderRepository,
        IPaymentWebhookRepository webhookRepository,
        IEntitlementRepository entitlementRepository,
        ICreditLedgerRepository creditLedgerRepository,
        IPaymentGateway paymentGateway,
        ILogger<PaymentWebhookService> logger)
    {
        _orderRepository = orderRepository;
        _webhookRepository = webhookRepository;
        _entitlementRepository = entitlementRepository;
        _creditLedgerRepository = creditLedgerRepository;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    public async Task<WebhookProcessResult> ProcessPayOsWebhookAsync(PayOsWebhookPayload payload, string rawBody, CancellationToken ct = default)
    {
        // 1. Kiểm tra chữ ký bảo mật
        if (!_paymentGateway.VerifyWebhookSignature(payload))
        {
            _logger.LogWarning("PayOS webhook verification failed. Invalid signature.");
            return new WebhookProcessResult(false, "Chữ ký webhook không hợp lệ.");
        }

        if (payload.Data == null)
        {
            return new WebhookProcessResult(false, "Dữ liệu webhook rỗng.");
        }

        var orderCode = payload.Data.OrderCode;
        var eventId = payload.Data.Reference ?? $"{orderCode}_{payload.Data.TransactionDateTime}";
        var payloadHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawBody)));

        // 2. Chống xử lý trùng lặp (Idempotency) qua Repository
        var isDuplicate = await _webhookRepository.HasEventProcessedAsync("PayOS", eventId, ct);
        if (isDuplicate)
        {
            _logger.LogInformation("PayOS webhook {EventId} already processed successfully.", eventId);
            return new WebhookProcessResult(true, "Webhook đã được xử lý trước đó.");
        }

        // 3. Tra cứu đơn hàng qua OrderRepository
        var order = await _orderRepository.GetByOrderCodeAsync(orderCode, ct);
        if (order == null)
        {
            _logger.LogError("PayOS webhook received for unknown OrderCode: {OrderCode}", orderCode);
            return new WebhookProcessResult(false, $"Không tìm thấy đơn hàng với mã {orderCode}.");
        }

        if (order.Status == OrderStatus.Paid)
        {
            return new WebhookProcessResult(true, "Đơn hàng đã ở trạng thái Paid.");
        }

        // 4. Đối chiếu số tiền thanh toán
        if (payload.Data.Amount < (int)order.Amount)
        {
            _logger.LogError("Amount mismatch for order {OrderCode}: expected {Expected}, got {Actual}",
                orderCode, order.Amount, payload.Data.Amount);
            return new WebhookProcessResult(false, "Số tiền thanh toán không khớp với giá trị đơn hàng.");
        }

        // 5. Mở Transaction cập nhật dữ liệu đồng bộ
        using var transaction = await _orderRepository.BeginTransactionAsync(ct);
        try
        {
            var now = DateTimeOffset.UtcNow;
            order.Status = OrderStatus.Paid;
            order.PaidAt = now;
            await _orderRepository.UpdateAsync(order, ct);

            // Ghi nhận lịch sử giao dịch
            var paymentTx = new PaymentTransaction
            {
                OrderId = order.Id,
                Provider = "PayOS",
                ProviderTransactionId = payload.Data.Reference ?? payload.Data.PaymentLinkId ?? $"{orderCode}",
                Amount = payload.Data.Amount,
                Status = PaymentTransactionStatus.Success,
                SignatureValid = true,
                RawPayloadJson = rawBody,
                ReceivedAt = now
            };
            await _webhookRepository.AddTransactionAsync(paymentTx, ct);

            // Cấp entitlement/credits theo từng OrderLine (giỏ hàng); fallback đơn 1 gói
            if (order.Lines.Count > 0)
            {
                foreach (var line in order.Lines.OrderBy(l => l.DisplayOrder))
                {
                    var linePlan = line.Plan ?? order.Plan;
                    var targetScopeId = line.TargetScopeId ?? TryReadTargetScopeId(line.LineSnapshotJson);
                    await EntitlementGrantHelper.GrantPlanAsync(
                        order.UserId,
                        order.Id,
                        linePlan,
                        targetScopeId,
                        now,
                        _entitlementRepository,
                        _creditLedgerRepository,
                        ct);
                }
            }
            else
            {
                var plan = order.Plan;
                var targetScopeId = TryReadTargetScopeId(order.PlanSnapshotJson);
                await EntitlementGrantHelper.GrantPlanAsync(
                    order.UserId,
                    order.Id,
                    plan,
                    targetScopeId,
                    now,
                    _entitlementRepository,
                    _creditLedgerRepository,
                    ct);
            }

            // Ghi nhận sự kiện webhook
            var webhookEvent = new PaymentWebhookEvent
            {
                Provider = "PayOS",
                EventId = eventId,
                PayloadHash = payloadHash,
                ReceivedAt = now,
                ProcessedAt = now,
                Result = "Success"
            };
            await _webhookRepository.AddEventAsync(webhookEvent, ct);

            await _orderRepository.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("Successfully processed PayOS payment for order {OrderCode}. Entitlements granted.", orderCode);
            return new WebhookProcessResult(true, "Xác nhận thanh toán và cấp quyền thành công.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(ex, "Transaction failed while processing PayOS webhook for order {OrderCode}", orderCode);
            throw;
        }
    }

    private static int? TryReadTargetScopeId(string? planSnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(planSnapshotJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(planSnapshotJson);
            if (doc.RootElement.TryGetProperty("targetScopeId", out var prop)
                && prop.ValueKind == JsonValueKind.Number
                && prop.TryGetInt32(out var id))
            {
                return id;
            }
        }
        catch (JsonException)
        {
            // Snapshot cũ không có targetScopeId — dùng ScopeId trên plan.
        }

        return null;
    }
}
