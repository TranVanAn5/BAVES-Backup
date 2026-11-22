using BAVES.Api.Modules.Commerce.Models.Enums;

namespace BAVES.Api.Modules.Commerce.Models.Entities;

public class PlanScope
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public Plan Plan { get; set; } = null!;
    public ScopeType ScopeType { get; set; }
    public int? ScopeId { get; set; }
}
