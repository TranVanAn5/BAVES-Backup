namespace BAVES.Api.Modules.Commerce.Models.Entities;

public class OrderLine
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public int? TargetScopeId { get; set; }
    public string? TargetLabel { get; set; }
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "VND";
    public string PlanCode { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;
    public string LineSnapshotJson { get; set; } = "{}";
    public int DisplayOrder { get; set; }
}
