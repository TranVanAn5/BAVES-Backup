using BAVES.Api.Modules.Commerce.Data;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Commerce.Repositories;

public class PaymentWebhookRepository : IPaymentWebhookRepository
{
    private readonly CommerceDbContext _db;

    public PaymentWebhookRepository(CommerceDbContext db)
    {
        _db = db;
    }

    public Task<bool> HasEventProcessedAsync(string provider, string eventId, CancellationToken ct = default)
    {
        return _db.PaymentWebhookEvents
            .AnyAsync(e => e.Provider == provider && e.EventId == eventId && e.Result == "Success", ct);
    }

    public async Task AddEventAsync(PaymentWebhookEvent webhookEvent, CancellationToken ct = default)
    {
        await _db.PaymentWebhookEvents.AddAsync(webhookEvent, ct);
    }

    public async Task AddTransactionAsync(PaymentTransaction transaction, CancellationToken ct = default)
    {
        await _db.PaymentTransactions.AddAsync(transaction, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return _db.SaveChangesAsync(ct);
    }
}
