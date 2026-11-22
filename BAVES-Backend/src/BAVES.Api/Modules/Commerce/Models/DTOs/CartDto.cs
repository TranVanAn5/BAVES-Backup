namespace BAVES.Api.Modules.Commerce.Models.DTOs;

public sealed record CartItemDto(
    int Id,
    int PlanId,
    string PlanCode,
    string PlanName,
    decimal UnitPrice,
    string Currency,
    int? TargetScopeId,
    string? TargetLabel,
    string? ScopeType,
    DateTimeOffset CreatedAt
);

public sealed record CartDto(
    int Id,
    int UserId,
    IReadOnlyList<CartItemDto> Items,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset UpdatedAt
);

public sealed class AddCartItemRequest
{
    public int PlanId { get; set; }
    public int? TargetScopeId { get; set; }
    /// <summary>Nhãn hiển thị: tên scenario / application / industry.</summary>
    public string? TargetLabel { get; set; }
}

public sealed class CheckoutCartRequest
{
    public string? ReturnUrl { get; set; }
    public string? CancelUrl { get; set; }
}
