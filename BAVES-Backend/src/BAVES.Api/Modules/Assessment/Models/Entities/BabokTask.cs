namespace BAVES.Api.Modules.Assessment.Models.Entities;

public class BabokTask
{
    public int Id { get; set; }

    public string KnowledgeAreaCode { get; set; } = "RADD";

    public string KnowledgeAreaName { get; set; } = "Requirements Analysis and Design Definition";

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Purpose { get; set; }

    public string? Description { get; set; }

    public string BabokVersion { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public ICollection<RubricCriterion> RubricCriteria { get; set; } = new List<RubricCriterion>();
}
