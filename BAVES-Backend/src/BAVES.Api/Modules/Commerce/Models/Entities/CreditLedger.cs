using BAVES.Api.Modules.Commerce.Models.Enums;

namespace BAVES.Api.Modules.Commerce.Models.Entities;

public class CreditLedger
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Delta { get; set; }
    public CreditReason Reason { get; set; }
    public string CreditType { get; set; } = "Review";
    public int? OrderId { get; set; }
    public int? ReviewRequestId { get; set; }
    public int? BookingId { get; set; }
    public string? Note { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
