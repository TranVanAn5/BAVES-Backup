
namespace BAVES.Api.Modules.Artifact.Models.Entities;

public class ArtifactTemplateSection
{
    public int Id { get; set; }

    public int ArtifactTemplateId { get; set; }

    public string SectionKey { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Instructions { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsRequired { get; set; }

    public ArtifactTemplate ArtifactTemplate { get; set; } = null!;

}
