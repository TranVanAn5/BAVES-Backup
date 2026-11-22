using BAVES.Api.Modules.Artifact.Models.DTOs;
using BAVES.Api.Modules.Artifact.Models.Entities;
using BAVES.Api.Modules.Artifact.Models.Enums;
using BAVES.Api.Modules.Artifact.Models.Requests;
using BAVES.Api.Modules.Artifact.Repositories.Interfaces;
using BAVES.Api.Modules.Artifact.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;
using ArtifactEntity = BAVES.Api.Modules.Artifact.Models.Entities.Artifact;

namespace BAVES.Api.Modules.Artifact.Services;

public sealed class ArtifactService(
    IArtifactRepository artifactRepository,
    ISimulationSessionService simulationSessionService
) : IArtifactService
{
    // ─────────── TEMPLATES ───────────
    public async Task<IReadOnlyList<ArtifactTemplateDto>> GetTemplatesAsync(
        ArtifactType? type,
        ArtifactTemplateStatus? status = null,
        bool isInstructorOrAdmin = false,
        CancellationToken cancellationToken = default)
    {
        // Learner chỉ được xem các template đã Published; Instructor/Admin có thể xem cả Draft/Archived
        var effectiveStatus = isInstructorOrAdmin
            ? status
            : ArtifactTemplateStatus.Published;

        return await artifactRepository.GetTemplatesAsync(type, effectiveStatus, cancellationToken);
    }

    public async Task<ArtifactTemplateDto?> GetTemplateByIdAsync(
        int templateId,
        CancellationToken cancellationToken = default)
    {
        return await artifactRepository.GetTemplateByIdAsync(templateId, cancellationToken);
    }

    public async Task<ArtifactTemplateDto> CreateTemplateAsync(
        CreateArtifactTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var code = request.Code.Trim().ToUpperInvariant();
        if (await artifactRepository.TemplateCodeExistsAsync(code, null, cancellationToken))
        {
            throw new ConflictException($"Biểu mẫu với mã Code '{code}' đã tồn tại.");
        }

        ValidateTemplateSections(request.Sections);

        var now = DateTimeOffset.UtcNow;
        var template = new ArtifactTemplate
        {
            Code = code,
            Name = request.Name.Trim(),
            ArtifactType = request.ArtifactType,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = request.PublishImmediately ? ArtifactTemplateStatus.Published : ArtifactTemplateStatus.Draft,
            CreatedAt = now,
            ArtifactTemplateSections = request.Sections.Select((s, idx) => new ArtifactTemplateSection
            {
                SectionKey = s.SectionKey.Trim(),
                Title = s.Title.Trim(),
                Instructions = string.IsNullOrWhiteSpace(s.Instructions) ? null : s.Instructions.Trim(),
                DisplayOrder = s.DisplayOrder > 0 ? s.DisplayOrder : idx + 1,
                IsRequired = s.IsRequired
            }).ToList()
        };

        return await artifactRepository.CreateTemplateAsync(template, cancellationToken);
    }

    public async Task<ArtifactTemplateDto?> UpdateTemplateAsync(
        int templateId,
        UpdateArtifactTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await artifactRepository.GetTemplateEntityByIdAsync(templateId, cancellationToken);
        if (template is null)
        {
            return null;
        }

        if (template.Status != ArtifactTemplateStatus.Draft)
        {
            throw new ConflictException(
                $"Chỉ có thể chỉnh sửa biểu mẫu khi còn ở trạng thái Draft (hiện tại: {template.Status}).");
        }

        ValidateTemplateSections(request.Sections);

        template.Name = request.Name.Trim();
        template.ArtifactType = request.ArtifactType;
        template.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        template.UpdatedAt = DateTimeOffset.UtcNow;

        var newSections = request.Sections.Select((s, idx) => new ArtifactTemplateSection
        {
            ArtifactTemplateId = template.Id,
            SectionKey = s.SectionKey.Trim(),
            Title = s.Title.Trim(),
            Instructions = string.IsNullOrWhiteSpace(s.Instructions) ? null : s.Instructions.Trim(),
            DisplayOrder = s.DisplayOrder > 0 ? s.DisplayOrder : idx + 1,
            IsRequired = s.IsRequired
        }).ToList();

        return await artifactRepository.UpdateTemplateAsync(template, newSections, cancellationToken);
    }

    public async Task<ArtifactTemplateDto?> PublishTemplateAsync(
        int templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await artifactRepository.GetTemplateEntityByIdAsync(templateId, cancellationToken);
        if (template is null)
        {
            return null;
        }

        if (template.Status == ArtifactTemplateStatus.Archived)
        {
            throw new ConflictException("Không thể phát hành (Publish) biểu mẫu đã bị lưu trữ (Archived).");
        }

        if (template.ArtifactTemplateSections.Count == 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Sections"] = ["Biểu mẫu phải có ít nhất 1 đề mục (Section) trước khi phát hành."]
            });
        }

        return await artifactRepository.PublishTemplateAsync(templateId, cancellationToken);
    }

    private static void ValidateTemplateSections(IReadOnlyCollection<ArtifactTemplateSectionInputRequest>? sections)
    {
        if (sections is null || sections.Count == 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Sections"] = ["Biểu mẫu phải có ít nhất 1 đề mục (Section)."]
            });
        }

        var duplicateKeys = sections
            .GroupBy(s => s.SectionKey.Trim(), StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateKeys.Count > 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Sections"] = [$"SectionKey bị trùng lặp trong biểu mẫu: {string.Join(", ", duplicateKeys)}."]
            });
        }
    }

    // ─────────── ARTIFACTS ───────────
    public async Task<IReadOnlyList<ArtifactDto>?> GetArtifactsBySessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        return await artifactRepository.GetArtifactsBySessionAsync(sessionId, cancellationToken);
    }

    public async Task<ArtifactDto?> GetArtifactByIdAsync(
        int learnerId,
        int artifactId,
        CancellationToken cancellationToken = default)
    {
        var artifact = await artifactRepository.GetArtifactDtoByIdAsync(artifactId, cancellationToken);
        if (artifact is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, artifact.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        return artifact;
    }

    public async Task<ArtifactDto?> CreateArtifactAsync(
        int learnerId,
        int sessionId,
        CreateArtifactRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException($"Không thể tạo tài liệu khi session đang ở trạng thái {session.Status}.");
        }

        var templatePublished = await artifactRepository.TemplateExistsAndPublishedAsync(request.ArtifactTemplateId, cancellationToken);
        if (!templatePublished)
        {
            throw new KeyNotFoundException($"Không tìm thấy ArtifactTemplate ID {request.ArtifactTemplateId} hoặc template chưa được publish.");
        }

        var artifact = new ArtifactEntity
        {
            SessionId = sessionId,
            ArtifactTemplateId = request.ArtifactTemplateId,
            Title = request.Title.Trim(),
            Status = ArtifactStatus.Draft,
            CurrentVersionNo = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return await artifactRepository.CreateArtifactAsync(artifact, cancellationToken);
    }

    public async Task<ArtifactDto?> UpdateArtifactDraftAsync(
        int learnerId,
        int artifactId,
        UpdateArtifactDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        var artifact = await artifactRepository.GetArtifactEntityByIdAsync(artifactId, cancellationToken);
        if (artifact is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, artifact.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException($"Không thể cập nhật nháp khi session đang ở trạng thái {session.Status}.");
        }

        if (artifact.Status == ArtifactStatus.Submitted)
        {
            throw new InvalidOperationException("Không thể chỉnh sửa tài liệu đã nộp (Submitted).");
        }

        return await artifactRepository.UpdateArtifactDraftAsync(
            artifactId,
            request.DraftStructuredContentJson,
            request.DraftPlainTextContent,
            cancellationToken);
    }

    // ─────────── VERSIONS ───────────
    public async Task<IReadOnlyList<ArtifactVersionDto>?> GetArtifactVersionsAsync(
        int learnerId,
        int artifactId,
        CancellationToken cancellationToken = default)
    {
        var artifact = await artifactRepository.GetArtifactDtoByIdAsync(artifactId, cancellationToken);
        if (artifact is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, artifact.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        return await artifactRepository.GetVersionsByArtifactIdAsync(artifactId, cancellationToken);
    }

    public async Task<ArtifactVersionDto?> FreezeArtifactVersionAsync(
        int learnerId,
        int artifactId,
        CreateArtifactVersionRequest request,
        CancellationToken cancellationToken = default)
    {
        var artifact = await artifactRepository.GetArtifactEntityByIdAsync(artifactId, cancellationToken);
        if (artifact is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, artifact.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException($"Không thể đóng băng phiên bản khi session đang ở trạng thái {session.Status}.");
        }

        if (artifact.Status == ArtifactStatus.Submitted)
        {
            throw new InvalidOperationException("Không thể đóng băng phiên bản cho tài liệu đã nộp (Submitted).");
        }

        var nextVersionNo = artifact.CurrentVersionNo + 1;

        var version = new ArtifactVersion
        {
            ArtifactId = artifactId,
            VersionNo = nextVersionNo,
            StructuredContentJson = request.StructuredContentJson,
            PlainTextContent = request.PlainTextContent,
            CreatedByUserId = learnerId,
            ProcessingStatus = ArtifactProcessingStatus.Ready,
            CreatedAt = DateTimeOffset.UtcNow
        };

        return await artifactRepository.CreateVersionAsync(version, cancellationToken);
    }

    // ─────────── SUBMISSIONS ───────────
    public async Task<IReadOnlyList<SubmissionDto>?> GetSubmissionsBySessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        return await artifactRepository.GetSubmissionsBySessionAsync(sessionId, cancellationToken);
    }

    public async Task<SubmissionDto?> GetSubmissionByIdAsync(
        int learnerId,
        int submissionId,
        CancellationToken cancellationToken = default)
    {
        var submission = await artifactRepository.GetSubmissionDtoByIdAsync(submissionId, cancellationToken);
        if (submission is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, submission.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        return submission;
    }

    public async Task<SubmissionDto?> SubmitArtifactVersionAsync(
        int learnerId,
        int sessionId,
        CreateSubmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var session = await simulationSessionService.GetSessionByIdAsync(learnerId, sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException($"Không thể nộp bài khi session đang ở trạng thái {session.Status}.");
        }

        var version = await artifactRepository.GetVersionEntityByIdAsync(request.ArtifactVersionId, cancellationToken);
        if (version is null)
        {
            throw new KeyNotFoundException($"Không tìm thấy ArtifactVersion ID {request.ArtifactVersionId}.");
        }

        if (version.Artifact.SessionId != sessionId)
        {
            throw new InvalidOperationException("ArtifactVersion không thuộc về phiên làm việc này.");
        }

        var nextAttemptNo = await artifactRepository.GetNextSubmissionAttemptNoAsync(sessionId, cancellationToken);

        var submission = new Submission
        {
            SessionId = sessionId,
            ArtifactVersionId = request.ArtifactVersionId,
            AttemptNo = nextAttemptNo,
            Status = SubmissionStatus.Submitted,
            SubmittedAt = DateTimeOffset.UtcNow
        };

        return await artifactRepository.CreateSubmissionAsync(submission, cancellationToken);
    }
}
