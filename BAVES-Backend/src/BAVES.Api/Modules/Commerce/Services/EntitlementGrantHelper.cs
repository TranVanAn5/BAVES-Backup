using BAVES.Api.Modules.Commerce.Models.Entities;
using BAVES.Api.Modules.Commerce.Models.Enums;
using BAVES.Api.Modules.Commerce.Repositories.Interfaces;

namespace BAVES.Api.Modules.Commerce.Services;

/// <summary>Cấp entitlement + credits cho một plan (một dòng đơn / mua lẻ).</summary>
internal static class EntitlementGrantHelper
{
    public static async Task GrantPlanAsync(
        int userId,
        int orderId,
        Plan plan,
        int? targetScopeId,
        DateTimeOffset now,
        IEntitlementRepository entitlements,
        ICreditLedgerRepository? credits,
        CancellationToken ct)
    {
        if (plan.PlanType == PlanType.Subscription)
        {
            await entitlements.AddAsync(new Entitlement
            {
                UserId = userId,
                SourceType = EntitlementSourceType.Subscription,
                SourceOrderId = orderId,
                ScopeType = ScopeType.All,
                ScopeId = null,
                StartsAt = now,
                EndsAt = plan.DurationDays.HasValue ? now.AddDays(plan.DurationDays.Value) : null,
                SessionQuota = plan.SessionQuota,
                SessionsUsed = 0,
                Status = EntitlementStatus.Active,
                CreatedAt = now
            }, ct);
        }
        else if (plan.PlanType is PlanType.ScenarioBundle or PlanType.Free)
        {
            var scopes = plan.Scopes.Count > 0
                ? plan.Scopes
                : new List<PlanScope> { new() { ScopeType = ScopeType.Scenario } };

            foreach (var scope in scopes)
            {
                await entitlements.AddAsync(new Entitlement
                {
                    UserId = userId,
                    SourceType = plan.PlanType == PlanType.Free
                        ? EntitlementSourceType.Free
                        : EntitlementSourceType.Purchase,
                    SourceOrderId = orderId,
                    ScopeType = scope.ScopeType,
                    ScopeId = scope.ScopeId ?? targetScopeId,
                    StartsAt = now,
                    EndsAt = plan.DurationDays.HasValue ? now.AddDays(plan.DurationDays.Value) : null,
                    SessionQuota = plan.SessionQuota,
                    SessionsUsed = 0,
                    Status = EntitlementStatus.Active,
                    CreatedAt = now
                }, ct);
            }
        }

        if (credits is null) return;

        if (plan.ReviewCredits > 0)
        {
            await credits.AddAsync(new CreditLedger
            {
                UserId = userId,
                Delta = plan.ReviewCredits,
                CreditType = "Review",
                Reason = CreditReason.Purchase,
                OrderId = orderId,
                Note = $"Cộng {plan.ReviewCredits} lượt Expert từ đơn #{orderId} ({plan.Code})",
                CreatedAt = now
            }, ct);
        }

        if (plan.ConsultationCredits > 0)
        {
            await credits.AddAsync(new CreditLedger
            {
                UserId = userId,
                Delta = plan.ConsultationCredits,
                CreditType = "Consultation",
                Reason = CreditReason.Purchase,
                OrderId = orderId,
                Note = $"Cộng {plan.ConsultationCredits} buổi cố vấn từ đơn #{orderId} ({plan.Code})",
                CreatedAt = now
            }, ct);
        }
    }
}
