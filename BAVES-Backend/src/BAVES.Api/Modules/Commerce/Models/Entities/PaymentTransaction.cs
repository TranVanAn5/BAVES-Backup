using BAVES.Api.Modules.Commerce.Models.Enums;

namespace BAVES.Api.Modules.Commerce.Models.Entities;

public class PaymentTransaction
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string Provider { get; set; } = "PayOS";
    public string ProviderTransactionId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;
    public bool SignatureValid { get; set; }
    public string? RawPayloadJson { get; set; }
    public DateTimeOffset ReceivedAt { get; set; } = DateTimeOffset.UtcNow;
}
