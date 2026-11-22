using BAVES.Api.Modules.Commerce.Models.DTOs;
using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Models.Enums;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;
using BAVES.Api.Modules.Commerce.Services.Interfaces;

namespace BAVES.Api.Modules.Commerce.Services;

public class EntitlementService : IEntitlementService
{
    private readonly IEntitlementRepository _entitlementRepository;
    private readonly ICreditLedgerRepository _creditLedgerRepository;
    private readonly IScenarioPurchaseTargetLookup? _purchaseTargets;

    public EntitlementService(
        IEntitlementRepository entitlementRepository,
        ICreditLedgerRepository creditLedgerRepository,
        IScenarioPurchaseTargetLookup? purchaseTargets = null)
    {
        _entitlementRepository = entitlementRepository;
        _creditLedgerRepository = creditLedgerRepository;
        _purchaseTargets = purchaseTargets;
    }

    public async Task<bool> CanStartSessionAsync(int userId, int scenarioVersionId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entitlements = await _entitlementRepository.GetActiveByUserIdAsync(userId, ct);
        var target = _purchaseTargets is null
            ? null
            : await _purchaseTargets.FindByVersionIdAsync(scenarioVersionId, ct);

        // Scenario Beginner được gắn miễn phí — không cần mua gói
        if (target?.IsFreeAccess == true)
        {
            return true;
        }

        foreach (var ent in entitlements)
        {
            // Kiểm tra hạn sử dụng
            if (ent.EndsAt.HasValue && ent.EndsAt.Value < now)
            {
                ent.Status = EntitlementStatus.Expired;
                await _entitlementRepository.UpdateAsync(ent, ct);
                continue;
            }

            // Kiểm tra số lượt phiên còn lại
            if (ent.SessionQuota.HasValue && ent.SessionsUsed >= ent.SessionQuota.Value)
            {
                ent.Status = EntitlementStatus.Expired;
                await _entitlementRepository.UpdateAsync(ent, ct);
                continue;
            }

            if (Covers(ent, scenarioVersionId, target))
            {
                return true;
            }
        }

        await _entitlementRepository.SaveChangesAsync(ct);
        return false;
    }

    public async Task RecordSessionUsedAsync(int userId, int scenarioVersionId, CancellationToken ct = default)
    {
        var entitlements = await _entitlementRepository.GetActiveByUserIdAsync(userId, ct);
        var target = _purchaseTargets is null
            ? null
            : await _purchaseTargets.FindByVersionIdAsync(scenarioVersionId, ct);

        var entitlement = entitlements.FirstOrDefault(e => Covers(e, scenarioVersionId, target));

        if (entitlement != null)
        {
            entitlement.SessionsUsed++;
            if (entitlement.SessionQuota.HasValue && entitlement.SessionsUsed >= entitlement.SessionQuota.Value)
            {
                entitlement.Status = EntitlementStatus.Expired;
            }
            await _entitlementRepository.UpdateAsync(entitlement, ct);
            await _entitlementRepository.SaveChangesAsync(ct);
        }
    }

    public Task<int> GetReviewCreditsAsync(int userId, CancellationToken ct = default)
    {
        return _creditLedgerRepository.GetCreditBalanceAsync(userId, "Review", ct);
    }

    public async Task<bool> ConsumeReviewCreditAsync(int userId, int reviewRequestId, CancellationToken ct = default)
    {
        var currentCredits = await _creditLedgerRepository.GetCreditBalanceAsync(userId, "Review", ct);
        if (currentCredits < 1)
        {
            return false;
        }

        var entry = new CreditLedger
        {
            UserId = userId,
            Delta = -1,
            CreditType = "Review",
            Reason = CreditReason.ReviewRequest,
            ReviewRequestId = reviewRequestId,
            Note = $"Trừ 1 lượt chấm bài cho ReviewRequest #{reviewRequestId}",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _creditLedgerRepository.AddAsync(entry, ct);
        await _creditLedgerRepository.SaveChangesAsync(ct);
        return true;
    }

    public Task<int> GetConsultationCreditsAsync(int userId, CancellationToken ct = default)
    {
        return _creditLedgerRepository.GetCreditBalanceAsync(userId, "Consultation", ct);
    }

    public async Task<bool> ConsumeConsultationCreditAsync(int userId, int bookingId, CancellationToken ct = default)
    {
        var currentCredits = await _creditLedgerRepository.GetCreditBalanceAsync(userId, "Consultation", ct);
        if (currentCredits < 1)
        {
            return false;
        }

        var entry = new CreditLedger
        {
            UserId = userId,
            Delta = -1,
            CreditType = "Consultation",
            Reason = CreditReason.BookingRequest,
            BookingId = bookingId,
            Note = $"Trừ 1 lượt trao đổi 1-on-1 cho Booking #{bookingId}",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _creditLedgerRepository.AddAsync(entry, ct);
        await _creditLedgerRepository.SaveChangesAsync(ct);
        return true;
    }

    public async Task RefundConsultationCreditAsync(int userId, int bookingId, string reason, CancellationToken ct = default)
    {
        var entry = new CreditLedger
        {
            UserId = userId,
            Delta = 1,
            CreditType = "Consultation",
            Reason = CreditReason.BookingRefund,
            BookingId = bookingId,
            Note = $"Hoàn trả 1 lượt trao đổi 1-on-1 cho Booking #{bookingId}. Lý do: {reason}",
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _creditLedgerRepository.AddAsync(entry, ct);
        await _creditLedgerRepository.SaveChangesAsync(ct);
    }

    public async Task<CreditBalanceSummaryDto> GetCreditBalanceSummaryAsync(int userId, CancellationToken ct = default)
    {
        var reviewCredits = await _creditLedgerRepository.GetCreditBalanceAsync(userId, "Review", ct);
        var consultationCredits = await _creditLedgerRepository.GetCreditBalanceAsync(userId, "Consultation", ct);
        var rawHistory = await _creditLedgerRepository.GetCreditLedgerHistoryAsync(userId, ct);

        var historyDtos = rawHistory.Select(h => new CreditHistoryItemDto(
            h.Id,
            h.Delta,
            h.CreditType,
            h.Reason.ToString(),
            h.OrderId,
            h.ReviewRequestId,
            h.BookingId,
            h.Note,
            h.CreatedAt
        )).ToList();

        return new CreditBalanceSummaryDto(userId, reviewCredits, consultationCredits, historyDtos);
    }

    public async Task<List<EntitlementDto>> GetUserEntitlementsAsync(int userId, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        var list = await _entitlementRepository.GetAllByUserIdAsync(userId, ct);

        // Tự động đánh dấu expired nếu hết hạn
        foreach (var e in list.Where(e => e.Status == EntitlementStatus.Active && e.EndsAt.HasValue && e.EndsAt.Value < now))
        {
            e.Status = EntitlementStatus.Expired;
            await _entitlementRepository.UpdateAsync(e, ct);
        }
        await _entitlementRepository.SaveChangesAsync(ct);

        return list.Select(e => new EntitlementDto(
            e.Id,
            e.UserId,
            e.SourceType,
            e.SourceOrderId,
            e.ScopeType,
            e.ScopeId,
            e.StartsAt,
            e.EndsAt,
            e.SessionQuota,
            e.SessionsUsed,
            e.Status
        )).ToList();
    }

    private static bool Covers(Entitlement entitlement, int scenarioVersionId, ScenarioPurchaseTarget? target)
    {
        if (entitlement.ScopeType == ScopeType.All)
        {
            return true;
        }

        if (entitlement.ScopeType == ScopeType.Scenario)
        {
            return !entitlement.ScopeId.HasValue
                || entitlement.ScopeId.Value == scenarioVersionId
                || (target != null && entitlement.ScopeId.Value == target.ScenarioId);
        }

        if (target is null || !entitlement.ScopeId.HasValue)
        {
            return false;
        }

        return entitlement.ScopeType switch
        {
            ScopeType.Application => entitlement.ScopeId.Value == target.ApplicationId,
            ScopeType.Industry => target.IndustryId.HasValue && entitlement.ScopeId.Value == target.IndustryId.Value,
            _ => false
        };
    }
}
