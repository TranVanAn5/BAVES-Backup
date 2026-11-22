using BAVES.Api.Modules.Artifact.Data;
using BAVES.Api.Modules.Artifact.Models.DTOs;
using BAVES.Api.Modules.Artifact.Models.Entities;
using BAVES.Api.Modules.Artifact.Models.Enums;
using BAVES.Api.Modules.Artifact.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ArtifactEntity = BAVES.Api.Modules.Artifact.Models.Entities.Artifact;

namespace BAVES.Api.Modules.Artifact.Repositories;

public sealed class ArtifactRepository(ArtifactDbContext db) : IArtifactRepository
{
    // ─────────── TEMPLATES ───────────
    public async Task<IReadOnlyList<ArtifactTemplateDto>> GetTemplatesAsync(
        ArtifactType? type,
        ArtifactTemplateStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.ArtifactTemplates
            .AsNoTracking()
            .Include(t => t.ArtifactTemplateSections)
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(t => t.ArtifactType == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var entities = await query
            .OrderBy(t => t.DisplayOrder)
            .ThenBy(t => t.Code)
            .ToListAsync(cancellationToken);

        return entities.Select(MapTemplateToDto).ToList();
    }

    public async Task<ArtifactTemplateDto?> GetTemplateByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.ArtifactTemplates
            .AsNoTracking()
            .Include(t => t.ArtifactTemplateSections)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return entity is null ? null : MapTemplateToDto(entity);
    }

    public async Task<ArtifactTemplate?> GetTemplateEntityByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.ArtifactTemplates
            .Include(t => t.ArtifactTemplateSections)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> TemplateCodeExistsAsync(
        string code,
        int? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.ArtifactTemplates
            .AsNoTracking()
            .Where(t => t.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(t => t.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> TemplateExistsAndPublishedAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await db.ArtifactTemplates
            .AsNoTracking()
            .AnyAsync(t => t.Id == id && t.Status == ArtifactTemplateStatus.Published, cancellationToken);
    }

    public async Task<ArtifactTemplateDto> CreateTemplateAsync(
        ArtifactTemplate template,
        CancellationToken cancellationToken = default)
    {
        db.ArtifactTemplates.Add(template);
        await db.SaveChangesAsync(cancellationToken);

        return MapTemplateToDto(template);
    }

    public async Task<ArtifactTemplateDto> UpdateTemplateAsync(
        ArtifactTemplate template,
        List<ArtifactTemplateSection> newSections,
        CancellationToken cancellationToken = default)
    {
        if (template.ArtifactTemplateSections.Count > 0)
        {
            db.ArtifactTemplateSections.RemoveRange(template.ArtifactTemplateSections);
        }

        template.ArtifactTemplateSections.Clear();
        foreach (var section in newSections)
        {
            section.ArtifactTemplateId = template.Id;
            template.ArtifactTemplateSections.Add(section);
        }

        await db.SaveChangesAsync(cancellationToken);
        return MapTemplateToDto(template);
    }

    public async Task<ArtifactTemplateDto?> PublishTemplateAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var template = await db.ArtifactTemplates
            .Include(t => t.ArtifactTemplateSections)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (template is null)
        {
            return null;
        }

        template.Status = ArtifactTemplateStatus.Published;
        template.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return MapTemplateToDto(template);
    }

    // ─────────── ARTIFACTS ───────────
    public async Task<IReadOnlyList<ArtifactDto>> GetArtifactsBySessionAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var entities = await db.Artifacts
            .AsNoTracking()
            .Include(a => a.ArtifactTemplate)
            .Where(a => a.SessionId == sessionId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return entities.Select(MapArtifactToDto).ToList();
    }

    public async Task<ArtifactDto?> GetArtifactDtoByIdAsync(
        int artifactId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.Artifacts
            .AsNoTracking()
            .Include(a => a.ArtifactTemplate)
            .FirstOrDefaultAsync(a => a.Id == artifactId, cancellationToken);

        return entity is null ? null : MapArtifactToDto(entity);
    }

    public async Task<ArtifactEntity?> GetArtifactEntityByIdAsync(
        int artifactId,
        CancellationToken cancellationToken = default)
    {
        return await db.Artifacts
            .Include(a => a.ArtifactTemplate)
            .FirstOrDefaultAsync(a => a.Id == artifactId, cancellationToken);
    }

    public async Task<ArtifactDto> CreateArtifactAsync(
        ArtifactEntity artifact,
        CancellationToken cancellationToken = default)
    {
        db.Artifacts.Add(artifact);
        await db.SaveChangesAsync(cancellationToken);

        if (artifact.ArtifactTemplate is null)
        {
            await db.Entry(artifact)
                .Reference(a => a.ArtifactTemplate)
                .LoadAsync(cancellationToken);
        }

        return MapArtifactToDto(artifact);
    }

    public async Task<ArtifactDto?> UpdateArtifactDraftAsync(
        int artifactId,
        string? draftStructuredContentJson,
        string? draftPlainTextContent,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.Artifacts
            .Include(a => a.ArtifactTemplate)
            .FirstOrDefaultAsync(a => a.Id == artifactId, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.DraftStructuredContentJson = draftStructuredContentJson;
        entity.DraftPlainTextContent = draftPlainTextContent;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
        return MapArtifactToDto(entity);
    }

    // ─────────── VERSIONS ───────────
    public async Task<IReadOnlyList<ArtifactVersionDto>> GetVersionsByArtifactIdAsync(
        int artifactId,
        CancellationToken cancellationToken = default)
    {
        var entities = await db.ArtifactVersions
            .AsNoTracking()
            .Where(v => v.ArtifactId == artifactId)
            .OrderByDescending(v => v.VersionNo)
            .ToListAsync(cancellationToken);

        return entities.Select(MapVersionToDto).ToList();
    }

    public async Task<ArtifactVersionDto?> GetVersionDtoByIdAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.ArtifactVersions
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        return entity is null ? null : MapVersionToDto(entity);
    }

    public async Task<ArtifactVersion?> GetVersionEntityByIdAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        return await db.ArtifactVersions
            .Include(v => v.Artifact)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);
    }

    public async Task<ArtifactVersionDto> CreateVersionAsync(
        ArtifactVersion version,
        CancellationToken cancellationToken = default)
    {
        var artifact = await db.Artifacts.FirstOrDefaultAsync(a => a.Id == version.ArtifactId, cancellationToken);
        if (artifact is not null)
        {
            artifact.CurrentVersionNo = version.VersionNo;
            artifact.Status = ArtifactStatus.ReadyForSubmission;
            artifact.UpdatedAt = DateTimeOffset.UtcNow;
        }

        db.ArtifactVersions.Add(version);
        await db.SaveChangesAsync(cancellationToken);

        return MapVersionToDto(version);
    }

    // ─────────── SUBMISSIONS ───────────
    public async Task<IReadOnlyList<SubmissionDto>> GetSubmissionsBySessionIdsAsync(
        IReadOnlyCollection<int> sessionIds,
        CancellationToken cancellationToken = default)
    {
        if (sessionIds.Count == 0)
        {
            return Array.Empty<SubmissionDto>();
        }

        var entities = await db.Submissions
            .AsNoTracking()
            .Include(s => s.ArtifactVersion)
            .Where(s => sessionIds.Contains(s.SessionId))
            .OrderByDescending(s => s.SubmittedAt)
            .ThenByDescending(s => s.AttemptNo)
            .ToListAsync(cancellationToken);

        return entities.Select(MapSubmissionToDto).ToList();
    }

    public async Task<IReadOnlyList<SubmissionDto>> GetSubmissionsBySessionAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var entities = await db.Submissions
            .AsNoTracking()
            .Include(s => s.ArtifactVersion)
            .Where(s => s.SessionId == sessionId)
            .OrderByDescending(s => s.AttemptNo)
            .ToListAsync(cancellationToken);

        return entities.Select(MapSubmissionToDto).ToList();
    }

    public async Task<SubmissionDto?> GetSubmissionDtoByIdAsync(
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        var entity = await db.Submissions
            .AsNoTracking()
            .Include(s => s.ArtifactVersion)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        return entity is null ? null : MapSubmissionToDto(entity);
    }

    public async Task<int> GetNextSubmissionAttemptNoAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var maxAttempt = await db.Submissions
            .AsNoTracking()
            .Where(s => s.SessionId == sessionId)
            .MaxAsync(s => (int?)s.AttemptNo, cancellationToken);

        return (maxAttempt ?? 0) + 1;
    }

    public async Task<SubmissionDto> CreateSubmissionAsync(
        Submission submission,
        CancellationToken cancellationToken = default)
    {
        db.Submissions.Add(submission);
        await db.SaveChangesAsync(cancellationToken);

        return MapSubmissionToDto(submission);
    }

    public async Task<bool> FinalizeSubmissionAsync(
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        var affected = await db.Submissions
            .Where(s => s.Id == submissionId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, SubmissionStatus.Finalized)
                .SetProperty(x => x.FinalizedAt, DateTimeOffset.UtcNow),
                cancellationToken);

        return affected > 0;
    }

    // ─────────── MAPPERS ───────────
    private static ArtifactTemplateDto MapTemplateToDto(ArtifactTemplate t) =>
        new(
            t.Id,
            t.Code,
            t.Name,
            t.ArtifactType,
            t.Description,
            t.Status,
            t.CreatedAt,
            t.ArtifactTemplateSections
                .OrderBy(s => s.DisplayOrder)
                .Select(s => new ArtifactTemplateSectionDto(
                    s.Id,
                    s.SectionKey,
                    s.Title,
                    s.Instructions,
                    s.DisplayOrder,
                    s.IsRequired))
                .ToList(),
            t.DeliverableCode,
            t.Category,
            t.DefaultTitle,
            t.IssueKeyPrefix,
            t.DisplayOrder,
            t.SkeletonHtmlContent,
            t.SampleHtmlContent,
            t.ChecklistJson
        );

    private static ArtifactDto MapArtifactToDto(ArtifactEntity a) =>
        new(
            a.Id,
            a.SessionId,
            a.ArtifactTemplateId,
            a.ArtifactTemplate?.Code ?? string.Empty,
            a.ArtifactTemplate?.Name ?? string.Empty,
            a.ArtifactTemplate?.ArtifactType ?? ArtifactType.Generic,
            a.Title,
            a.DraftStructuredContentJson,
            a.DraftPlainTextContent,
            a.Status,
            a.CurrentVersionNo,
            a.CreatedAt,
            a.UpdatedAt
        );

    private static ArtifactVersionDto MapVersionToDto(ArtifactVersion v) =>
        new(
            v.Id,
            v.ArtifactId,
            v.VersionNo,
            v.StructuredContentJson,
            v.PlainTextContent,
            v.CreatedByUserId,
            v.ProcessingStatus,
            v.CreatedAt
        );

    private static SubmissionDto MapSubmissionToDto(Submission s) =>
        new(
            s.Id,
            s.SessionId,
            s.ArtifactVersionId,
            s.AttemptNo,
            s.Status,
            s.SubmittedAt,
            s.FinalizedAt,
            s.ArtifactVersion is null ? null : MapVersionToDto(s.ArtifactVersion)
        );
}
