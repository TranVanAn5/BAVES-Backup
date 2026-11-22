using BAVES.Api.Modules.Commerce.Models.Enums;

namespace BAVES.Api.Modules.Commerce.Models.Entities;

public class Order
{
    public int Id { get; set; }
    public long OrderCode { get; set; }
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string PlanSnapshotJson { get; set; } = "{}";
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? IdempotencyKey { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? QrCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }

    public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();
    public ICollection<Entitlement> Entitlements { get; set; } = new List<Entitlement>();
    public ICollection<OrderLine> Lines { get; set; } = new List<OrderLine>();
}
