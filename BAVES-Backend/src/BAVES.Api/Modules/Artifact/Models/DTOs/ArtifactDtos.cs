using BAVES.Api.Modules.Artifact.Models.Enums;

namespace BAVES.Api.Modules.Artifact.Models.DTOs;

public sealed record ArtifactTemplateSectionDto(
    int Id,
    string SectionKey,
    string Title,
    string? Instructions,
    int DisplayOrder,
    bool IsRequired
);

public sealed record ArtifactTemplateDto(
    int Id,
    string Code,
    string Name,
    ArtifactType ArtifactType,
    string? Description,
    ArtifactTemplateStatus Status,
    DateTimeOffset CreatedAt,
    IReadOnlyList<ArtifactTemplateSectionDto> Sections,
    string? DeliverableCode = null,
    string? Category = null,
    string? DefaultTitle = null,
    string? IssueKeyPrefix = null,
    int DisplayOrder = 0,
    string? SkeletonHtmlContent = null,
    string? SampleHtmlContent = null,
    string? ChecklistJson = null
);

public sealed record ArtifactDto(
    int Id,
    int SessionId,
    int ArtifactTemplateId,
    string TemplateCode,
    string TemplateName,
    ArtifactType ArtifactType,
    string Title,
    string? DraftStructuredContentJson,
    string? DraftPlainTextContent,
    ArtifactStatus Status,
    int CurrentVersionNo,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public sealed record ArtifactVersionDto(
    int Id,
    int ArtifactId,
    int VersionNo,
    string StructuredContentJson,
    string PlainTextContent,
    int CreatedByUserId,
    ArtifactProcessingStatus ProcessingStatus,
    DateTimeOffset CreatedAt
);

public sealed record SubmissionDto(
    int Id,
    int SessionId,
    int ArtifactVersionId,
    int AttemptNo,
    SubmissionStatus Status,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? FinalizedAt,
    ArtifactVersionDto? ArtifactVersion = null
);
