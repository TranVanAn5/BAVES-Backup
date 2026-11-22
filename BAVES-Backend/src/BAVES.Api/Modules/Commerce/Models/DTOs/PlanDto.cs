using BAVES.Api.Modules.Commerce.Models.Enums;

namespace BAVES.Api.Modules.Commerce.Models.DTOs;

public record PlanScopeDto(
    int Id,
    ScopeType ScopeType,
    int? ScopeId
);

public record PlanDto(
    int Id,
    string Code,
    string Name,
    string Description,
    PlanType PlanType,
    decimal Price,
    string Currency,
    int? DurationDays,
    int? SessionQuota,
    int ReviewCredits,
    int ConsultationCredits,
    PlanStatus Status,
    int DisplayOrder,
    List<PlanScopeDto> Scopes
);
