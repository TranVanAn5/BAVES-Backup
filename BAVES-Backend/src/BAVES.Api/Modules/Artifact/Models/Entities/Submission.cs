using BAVES.Api.Modules.Artifact.Models.Enums;

namespace BAVES.Api.Modules.Artifact.Models.Entities;

public class Submission
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int ArtifactVersionId { get; set; }

    public int AttemptNo { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    public DateTimeOffset SubmittedAt { get; set; }

    public DateTimeOffset? FinalizedAt { get; set; }

    public ArtifactVersion ArtifactVersion { get; set; } = null!;

}
