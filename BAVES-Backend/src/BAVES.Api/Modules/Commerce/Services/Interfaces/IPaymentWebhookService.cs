using BAVES.Api.Modules.Commerce.Models.DTOs;

namespace BAVES.Api.Modules.Commerce.Services.Interfaces;

public record WebhookProcessResult(
    bool Success,
    string Message
);

public interface IPaymentWebhookService
{
    Task<WebhookProcessResult> ProcessPayOsWebhookAsync(PayOsWebhookPayload payload, string rawBody, CancellationToken ct = default);
}
