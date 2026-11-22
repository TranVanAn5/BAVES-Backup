using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Models.Entities;

public class Cohort
{
    public int Id { get; set; }

    public int InstructorId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public CohortStatus Status { get; set; } = CohortStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<CohortMember> CohortMembers { get; set; } = new List<CohortMember>();

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

}
