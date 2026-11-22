namespace BAVES.Api.Modules.Commerce.Models.Entities;

public class PaymentWebhookEvent
{
    public int Id { get; set; }
    public string Provider { get; set; } = "PayOS";
    public string EventId { get; set; } = string.Empty;
    public string PayloadHash { get; set; } = string.Empty;
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
    public string Result { get; set; } = "Pending";
}
