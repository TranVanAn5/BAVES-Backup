using System.ComponentModel.DataAnnotations;
using BAVES.Api.Modules.Commerce.Models.Enums;

namespace BAVES.Api.Modules.Commerce.Models.DTOs;

public class CreateOrderRequest
{
    [Required]
    public int PlanId { get; set; }

    /// <summary>
    /// Khi gói có ScopeType Scenario/Application/Industry và ScopeId null,
    /// client truyền ID đích (scenarioId / applicationId / industryId) lúc mua.
    /// </summary>
    public int? TargetScopeId { get; set; }

    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}

public record OrderLineDto(
    int Id,
    int PlanId,
    string PlanCode,
    string PlanName,
    decimal UnitPrice,
    string Currency,
    int? TargetScopeId,
    string? TargetLabel,
    int DisplayOrder
);

public record OrderDto(
    int Id,
    long OrderCode,
    int UserId,
    int PlanId,
    string PlanName,
    decimal Amount,
    string Currency,
    OrderStatus Status,
    string? CheckoutUrl,
    string? QrCode,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? PaidAt,
    IReadOnlyList<OrderLineDto>? Lines = null
);
