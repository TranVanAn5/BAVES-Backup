using BAVES.Api.Modules.Commerce.Models.Entities;

namespace BAVES.Api.Modules.Commerce.Repositories.Interfaces;

public interface IPaymentWebhookRepository
{
    Task<bool> HasEventProcessedAsync(string provider, string eventId, CancellationToken ct = default);
    Task AddEventAsync(PaymentWebhookEvent webhookEvent, CancellationToken ct = default);
    Task AddTransactionAsync(PaymentTransaction transaction, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
