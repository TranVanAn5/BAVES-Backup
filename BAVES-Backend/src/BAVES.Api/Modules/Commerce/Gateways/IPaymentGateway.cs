using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Models.Entities;

namespace BAVES.Api.Modules.Commerce.Gateways;

public record PaymentLinkResult(
    string CheckoutUrl,
    string? QrCode,
    string? PaymentLinkId
);

public interface IPaymentGateway
{
    string ProviderName { get; }

    Task<PaymentLinkResult> CreatePaymentLinkAsync(
        Order order,
        string returnUrl,
        string cancelUrl,
        CancellationToken ct = default);

    bool VerifyWebhookSignature(PayOsWebhookPayload payload);
}
