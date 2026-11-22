using BAVES.Api.Modules.Commerce.Models.Enums;

namespace BAVES.Api.Modules.Commerce.Models.Entities;

public class Plan
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PlanType PlanType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public int? DurationDays { get; set; }
    public int? SessionQuota { get; set; }
    public int ReviewCredits { get; set; }
    public int ConsultationCredits { get; set; }
    public PlanStatus Status { get; set; } = PlanStatus.Draft;
    public int DisplayOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<PlanScope> Scopes { get; set; } = new List<PlanScope>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
