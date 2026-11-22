using BAVES.Api.Modules.Assessment.Models.Enums;

namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class Rubric
{
    public int Id { get; set; }

    public int ScenarioVersionId { get; set; }

    public int CreatedByUserId { get; set; }

    public string Name { get; set; } = null!;

    public int Version { get; set; }

    public string? Description { get; set; }

    public RubricStatus Status { get; set; } = RubricStatus.Draft;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<RubricCriterion> RubricCriteria { get; set; } = new List<RubricCriterion>();

    public ICollection<HumanReview> HumanReviews { get; set; } = new List<HumanReview>();

}
