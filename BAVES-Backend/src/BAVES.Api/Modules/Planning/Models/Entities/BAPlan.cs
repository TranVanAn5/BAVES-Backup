using BAVES.Api.Modules.Planning.Models.Enums;

namespace BAVES.Api.Modules.Planning.Models.Entities;

public class BAPlan
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public string Objective { get; set; } = null!;

    public string? Approach { get; set; }

    public BAPlanStatus Status { get; set; } = BAPlanStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<PlannedQuestion> PlannedQuestions { get; set; } = new List<PlannedQuestion>();

}
