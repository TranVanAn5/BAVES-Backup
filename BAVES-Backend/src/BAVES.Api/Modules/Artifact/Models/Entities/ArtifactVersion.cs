using BAVES.Api.Modules.Artifact.Models.Enums;

namespace BAVES.Api.Modules.Artifact.Models.Entities;

public class ArtifactVersion
{
    public int Id { get; set; }

    public int ArtifactId { get; set; }

    public int VersionNo { get; set; }

    public string StructuredContentJson { get; set; } = null!;

    public string PlainTextContent { get; set; } = null!;

    public int CreatedByUserId { get; set; }

    public ArtifactProcessingStatus ProcessingStatus { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Artifact Artifact { get; set; } = null!;

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

}
