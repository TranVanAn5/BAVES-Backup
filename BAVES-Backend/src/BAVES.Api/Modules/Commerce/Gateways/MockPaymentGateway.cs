using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Models.Entities;

namespace BAVES.Api.Modules.Commerce.Gateways;

public class MockPaymentGateway : IPaymentGateway
{
    public string ProviderName => "Mock";

    public Task<PaymentLinkResult> CreatePaymentLinkAsync(
        Order order,
        string returnUrl,
        string cancelUrl,
        CancellationToken ct = default)
    {
        var checkoutUrl = $"http://localhost:5000/api/mock-payment/checkout?orderCode={order.OrderCode}&amount={order.Amount}";
        var qrCode = $"vietqr://mock-payment?orderCode={order.OrderCode}&amount={order.Amount}";
        var paymentLinkId = $"mock-link-{order.OrderCode}";

        return Task.FromResult(new PaymentLinkResult(checkoutUrl, qrCode, paymentLinkId));
    }

    public bool VerifyWebhookSignature(PayOsWebhookPayload payload)
    {
        // Mock gateway luôn coi signature hợp lệ trừ khi signature == "INVALID_TEST_SIGNATURE"
        return payload.Signature != "INVALID_TEST_SIGNATURE";
    }
}
