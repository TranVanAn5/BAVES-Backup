using BAVES.Api.Modules.Commerce.Models.Enums;

namespace BAVES.Api.Modules.Commerce.Models.Entities;

public class Entitlement
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public EntitlementSourceType SourceType { get; set; } = EntitlementSourceType.Purchase;
    public int? SourceOrderId { get; set; }
    public Order? SourceOrder { get; set; }
    public ScopeType ScopeType { get; set; }
    public int? ScopeId { get; set; }
    public DateTimeOffset StartsAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EndsAt { get; set; }
    public int? SessionQuota { get; set; }
    public int SessionsUsed { get; set; } = 0;
    public EntitlementStatus Status { get; set; } = EntitlementStatus.Active;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
