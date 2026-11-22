using BAVES.Api.Modules.Commerce.Models.DTOs;

namespace BAVES.Api.Modules.Commerce.Services.Interfaces;

public interface IEntitlementService
{
    Task<bool> CanStartSessionAsync(int userId, int scenarioVersionId, CancellationToken ct = default);
    Task RecordSessionUsedAsync(int userId, int scenarioVersionId, CancellationToken ct = default);
    Task<int> GetReviewCreditsAsync(int userId, CancellationToken ct = default);
    Task<bool> ConsumeReviewCreditAsync(int userId, int reviewRequestId, CancellationToken ct = default);
    Task<int> GetConsultationCreditsAsync(int userId, CancellationToken ct = default);
    Task<bool> ConsumeConsultationCreditAsync(int userId, int bookingId, CancellationToken ct = default);
    Task RefundConsultationCreditAsync(int userId, int bookingId, string reason, CancellationToken ct = default);
    Task<CreditBalanceSummaryDto> GetCreditBalanceSummaryAsync(int userId, CancellationToken ct = default);
    Task<List<EntitlementDto>> GetUserEntitlementsAsync(int userId, CancellationToken ct = default);
}
