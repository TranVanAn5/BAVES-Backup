using BAVES.Api.Modules.Artifact.Models.Enums;

namespace BAVES.Api.Modules.Artifact.Models.Entities;

public class ArtifactTemplate
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public ArtifactType ArtifactType { get; set; }

    public string? Description { get; set; }

    public ArtifactTemplateStatus Status { get; set; } = ArtifactTemplateStatus.Draft;

    /// <summary>Deliverable code shown in Docs UI, e.g. DEL-02.</summary>
    public string? DeliverableCode { get; set; }

    /// <summary>UI category for filtering (no framework branding).</summary>
    public string? Category { get; set; }

    public string? DefaultTitle { get; set; }

    public string? IssueKeyPrefix { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>Empty scaffold HTML for TipTap Docs workspace.</summary>
    public string? SkeletonHtmlContent { get; set; }

    /// <summary>Optional filled sample HTML for learners.</summary>
    public string? SampleHtmlContent { get; set; }

    /// <summary>Optional checklist JSON for the Docs guide drawer.</summary>
    public string? ChecklistJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ArtifactTemplateSection> ArtifactTemplateSections { get; set; } = new List<ArtifactTemplateSection>();

    public ICollection<Artifact> Artifacts { get; set; } = new List<Artifact>();

}
