using BAVES.Api.Modules.Artifact.Models.Enums;

namespace BAVES.Api.Modules.Artifact.Models.Entities;

public class Artifact
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int ArtifactTemplateId { get; set; }

    public string Title { get; set; } = null!;

    public string? DraftStructuredContentJson { get; set; }

    public string? DraftPlainTextContent { get; set; }

    public ArtifactStatus Status { get; set; } = ArtifactStatus.Draft;

    public int CurrentVersionNo { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ArtifactTemplate ArtifactTemplate { get; set; } = null!;

    public ICollection<ArtifactVersion> ArtifactVersions { get; set; } = new List<ArtifactVersion>();

}
