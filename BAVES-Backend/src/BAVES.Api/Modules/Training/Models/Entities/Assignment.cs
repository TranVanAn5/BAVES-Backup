using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Models.Entities;

public class Assignment
{
    public int Id { get; set; }

    public int ScenarioVersionId { get; set; }

    public int? CohortId { get; set; }

    public int? LearnerId { get; set; }

    public int? RubricId { get; set; }

    public int AssignedByUserId { get; set; }

    public string Title { get; set; } = null!;

    public string? Instructions { get; set; }

    public DateTimeOffset? StartAt { get; set; }

    public DateTimeOffset? DueAt { get; set; }

    public int? MaxAttempts { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Cohort? Cohort { get; set; }

}
