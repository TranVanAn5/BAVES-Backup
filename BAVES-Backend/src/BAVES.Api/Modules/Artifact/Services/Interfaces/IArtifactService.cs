using BAVES.Api.Modules.Artifact.Models.DTOs;
using BAVES.Api.Modules.Artifact.Models.Enums;
using BAVES.Api.Modules.Artifact.Models.Requests;

namespace BAVES.Api.Modules.Artifact.Services.Interfaces;

public interface IArtifactService
{
    // Templates
    Task<IReadOnlyList<ArtifactTemplateDto>> GetTemplatesAsync(
        ArtifactType? type,
        ArtifactTemplateStatus? status = null,
        bool isInstructorOrAdmin = false,
        CancellationToken cancellationToken = default);

    Task<ArtifactTemplateDto?> GetTemplateByIdAsync(
        int templateId,
        CancellationToken cancellationToken = default);

    Task<ArtifactTemplateDto> CreateTemplateAsync(
        CreateArtifactTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<ArtifactTemplateDto?> UpdateTemplateAsync(
        int templateId,
        UpdateArtifactTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<ArtifactTemplateDto?> PublishTemplateAsync(
        int templateId,
        CancellationToken cancellationToken = default);

    // Artifacts
    Task<IReadOnlyList<ArtifactDto>?> GetArtifactsBySessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<ArtifactDto?> GetArtifactByIdAsync(
        int learnerId,
        int artifactId,
        CancellationToken cancellationToken = default);

    Task<ArtifactDto?> CreateArtifactAsync(
        int learnerId,
        int sessionId,
        CreateArtifactRequest request,
        CancellationToken cancellationToken = default);

    Task<ArtifactDto?> UpdateArtifactDraftAsync(
        int learnerId,
        int artifactId,
        UpdateArtifactDraftRequest request,
        CancellationToken cancellationToken = default);

    // Versions
    Task<IReadOnlyList<ArtifactVersionDto>?> GetArtifactVersionsAsync(
        int learnerId,
        int artifactId,
        CancellationToken cancellationToken = default);

    Task<ArtifactVersionDto?> FreezeArtifactVersionAsync(
        int learnerId,
        int artifactId,
        CreateArtifactVersionRequest request,
        CancellationToken cancellationToken = default);

    // Submissions
    Task<IReadOnlyList<SubmissionDto>?> GetSubmissionsBySessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<SubmissionDto?> GetSubmissionByIdAsync(
        int learnerId,
        int submissionId,
        CancellationToken cancellationToken = default);

    Task<SubmissionDto?> SubmitArtifactVersionAsync(
        int learnerId,
        int sessionId,
        CreateSubmissionRequest request,
        CancellationToken cancellationToken = default);
}
