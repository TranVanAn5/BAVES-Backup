using BAVES.Api.Modules.Planning.Models.Enums;

namespace BAVES.Api.Modules.Planning.Models.Entities;

public class PlannedQuestion
{
    public int Id { get; set; }

    public int BAPlanId { get; set; }

    public int? StakeholderPersonaId { get; set; }

    public string? Topic { get; set; }

    public string? InformationNeed { get; set; }

    public string Question { get; set; } = null!;

    public PriorityLevel Priority { get; set; }

    public int DisplayOrder { get; set; }

    public PlannedQuestionStatus Status { get; set; } = PlannedQuestionStatus.Planned;

    public DateTimeOffset CreatedAt { get; set; }

    public BAPlan BAPlan { get; set; } = null!;

}
