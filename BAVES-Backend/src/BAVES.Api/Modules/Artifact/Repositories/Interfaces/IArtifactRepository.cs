using BAVES.Api.Modules.Artifact.Models.DTOs;
using BAVES.Api.Modules.Artifact.Models.Entities;
using BAVES.Api.Modules.Artifact.Models.Enums;
using ArtifactEntity = BAVES.Api.Modules.Artifact.Models.Entities.Artifact;

namespace BAVES.Api.Modules.Artifact.Repositories.Interfaces;

public interface IArtifactRepository
{
    // Templates
    Task<IReadOnlyList<ArtifactTemplateDto>> GetTemplatesAsync(
        ArtifactType? type,
        ArtifactTemplateStatus? status,
        CancellationToken cancellationToken = default);

    Task<ArtifactTemplateDto?> GetTemplateByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ArtifactTemplate?> GetTemplateEntityByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<bool> TemplateCodeExistsAsync(
        string code,
        int? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<bool> TemplateExistsAndPublishedAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ArtifactTemplateDto> CreateTemplateAsync(
        ArtifactTemplate template,
        CancellationToken cancellationToken = default);

    Task<ArtifactTemplateDto> UpdateTemplateAsync(
        ArtifactTemplate template,
        List<ArtifactTemplateSection> newSections,
        CancellationToken cancellationToken = default);

    Task<ArtifactTemplateDto?> PublishTemplateAsync(
        int id,
        CancellationToken cancellationToken = default);

    // Artifacts
    Task<IReadOnlyList<ArtifactDto>> GetArtifactsBySessionAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ArtifactDto?> GetArtifactDtoByIdAsync(
        int artifactId,
        CancellationToken cancellationToken = default);

    Task<ArtifactEntity?> GetArtifactEntityByIdAsync(
        int artifactId,
        CancellationToken cancellationToken = default);

    Task<ArtifactDto> CreateArtifactAsync(
        ArtifactEntity artifact,
        CancellationToken cancellationToken = default);

    Task<ArtifactDto?> UpdateArtifactDraftAsync(
        int artifactId,
        string? draftStructuredContentJson,
        string? draftPlainTextContent,
        CancellationToken cancellationToken = default);

    // Versions
    Task<IReadOnlyList<ArtifactVersionDto>> GetVersionsByArtifactIdAsync(
        int artifactId,
        CancellationToken cancellationToken = default);

    Task<ArtifactVersionDto?> GetVersionDtoByIdAsync(
        int versionId,
        CancellationToken cancellationToken = default);

    Task<ArtifactVersion?> GetVersionEntityByIdAsync(
        int versionId,
        CancellationToken cancellationToken = default);

    Task<ArtifactVersionDto> CreateVersionAsync(
        ArtifactVersion version,
        CancellationToken cancellationToken = default);

    // Submissions
    Task<IReadOnlyList<SubmissionDto>> GetSubmissionsBySessionIdsAsync(
        IReadOnlyCollection<int> sessionIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SubmissionDto>> GetSubmissionsBySessionAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<SubmissionDto?> GetSubmissionDtoByIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default);

    Task<int> GetNextSubmissionAttemptNoAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<SubmissionDto> CreateSubmissionAsync(
        Submission submission,
        CancellationToken cancellationToken = default);

    Task<bool> FinalizeSubmissionAsync(
        int submissionId,
        CancellationToken cancellationToken = default);
}
