using BAVES.Api.Modules.Commerce.Models.Enums;

namespace BAVES.Api.Modules.Commerce.Models.DTOs;

public record EntitlementDto(
    int Id,
    int UserId,
    EntitlementSourceType SourceType,
    int? SourceOrderId,
    ScopeType ScopeType,
    int? ScopeId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    int? SessionQuota,
    int SessionsUsed,
    EntitlementStatus Status
);

public record CreditSummaryDto(
    int UserId,
    int TotalCredits,
    DateTimeOffset AsOf
);

public record CreditBalanceSummaryDto(
    int UserId,
    int ReviewCreditsBalance,
    int ConsultationCreditsBalance,
    List<CreditHistoryItemDto> History
);

public record CreditHistoryItemDto(
    int Id,
    int Delta,
    string CreditType,
    string Reason,
    int? OrderId,
    int? ReviewRequestId,
    int? BookingId,
    string? Note,
    DateTimeOffset CreatedAt
);
