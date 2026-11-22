using BAVES.Api.Modules.Identity.Services.Interfaces;
using BAVES.Api.Modules.Scenario.Data;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Entities;
using BAVES.Api.Modules.Scenario.Models.Enums;
using BAVES.Api.Modules.Scenario.Models.Requests;
using BAVES.Api.Modules.Scenario.Repositories.Interfaces;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Training.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using ScenarioEntity = BAVES.Api.Modules.Scenario.Models.Entities.Scenario;

namespace BAVES.Api.Modules.Scenario.Services;

public sealed class ScenarioAuthoringService(
    IScenarioAuthoringRepository repository,
    ScenarioDbContext scenarioDb,
    IAssignmentService assignmentService,
    IUserService userService
) : IScenarioAuthoringService
{
    public async Task<ScenarioSummaryDto> CreateScenarioAsync(
        int userId,
        CreateScenarioRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Kiểm tra ApplicationId có hợp lệ và tồn tại hay không
        var applicationExists = await repository.ApplicationExistsAsync(
            request.ApplicationId,
            cancellationToken
        );

        if (!applicationExists)
        {
            throw new NotFoundException($"Business application with ID {request.ApplicationId} was not found.");
        }

        // 2. Kiểm tra Scenario Code có bị trùng lặp không (Code là Unique)
        var codeExists = await repository.ScenarioCodeExistsAsync(
            request.Code,
            cancellationToken
        );

        if (codeExists)
        {
            throw new ConflictException($"Scenario with code '{request.Code}' already exists.");
        }

        var now = DateTimeOffset.UtcNow;

        // 3. Khởi tạo Scenario chính ở trạng thái Draft
        var scenario = new ScenarioEntity
        {
            ApplicationId = request.ApplicationId,
            CreatedByUserId = userId,
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = ScenarioStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now
        };

        // 4. Khởi tạo luôn Version 1 (Draft) đi kèm theo nghiệp vụ
        var version1 = new ScenarioVersion
        {
            VersionNo = 1,
            Summary = request.Summary?.Trim() ?? request.Title.Trim(),
            BusinessContext = request.BusinessContext?.Trim() ?? string.Empty,
            BusinessProblem = request.BusinessProblem?.Trim() ?? string.Empty,
            BusinessObjectives = request.BusinessObjectives?.Trim() ?? string.Empty,
            BARole = request.BARole?.Trim() ?? "Business Analyst",
            Assignment = string.Empty,
            Difficulty = ScenarioDifficulty.Intermediate,
            // Admin → thư viện toàn hệ thống; Instructor → chỉ danh sách của creator (sau khi duyệt).
            IsFreeAccess = await IsUserAdminAsync(userId, cancellationToken),
            IsPublished = false,
            CreatedAt = now
        };

        // Chỉ dùng ScenarioSprints — không còn Assignment brief trên scenario.
        version1.Sprints.Add(ScenarioSprintMapper.ToEntity(0, new ScenarioSprintDto(1, string.Empty, 1, "weeks", [])));

        scenario.Versions.Add(version1);

        await repository.AddScenarioAsync(scenario, cancellationToken);

        return new ScenarioSummaryDto(
            scenario.Id,
            scenario.ApplicationId,
            scenario.Code,
            scenario.Title,
            scenario.Description,
            scenario.Status,
            version1.Id,
            scenario.CreatedAt
        );
    }

    public async Task<ScenarioVersionSummaryDto> CreateVersionAsync(
        int userId,
        int scenarioId,
        CreateScenarioVersionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Kiểm tra Scenario có tồn tại không
        var scenario = await repository.GetScenarioWithVersionsAsync(scenarioId, cancellationToken);

        if (scenario is null)
        {
            throw new NotFoundException($"Scenario with ID {scenarioId} was not found.");
        }

        // 2. Quyền sở hữu: Chỉ tác giả tạo Scenario (hoặc admin sau này) mới được tạo version mới
        if (scenario.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to create a new version for this scenario.");
        }

        // 3. Quy tắc nghiệp vụ: Chỉ cho phép tạo version mới khi KHÔNG có bản nháp (Draft) nào đang mở
        var hasActiveDraft = scenario.Versions.Any(v => !v.IsPublished);
        if (hasActiveDraft)
        {
            throw new ConflictException("A draft version already exists for this scenario. Please publish or discard it before creating a new version.");
        }

        // 4. Tìm version mới nhất hiện tại để tính VersionNo mới và sao chép dữ liệu (nếu có yêu cầu)
        var latestVersion = await repository.GetLatestVersionWithAggregateAsync(scenarioId, cancellationToken);

        var nextVersionNo = (latestVersion?.VersionNo ?? 0) + 1;
        var now = DateTimeOffset.UtcNow;
        var summaryOverride = request.Summary?.Trim();
        var copiedSummary = request.CopyFromLatest ? latestVersion?.Summary?.Trim() : null;

        var newVersion = new ScenarioVersion
        {
            ScenarioId = scenarioId,
            VersionNo = nextVersionNo,
            Summary = !string.IsNullOrWhiteSpace(summaryOverride)
                ? summaryOverride
                : !string.IsNullOrWhiteSpace(copiedSummary)
                    ? copiedSummary
                    : $"Version {nextVersionNo}",
            Difficulty = latestVersion?.Difficulty ?? ScenarioDifficulty.Intermediate,
            IsPublished = false,
            Status = ScenarioStatus.Draft,
            CreatedAt = now
        };

        if (latestVersion != null && request.CopyFromLatest)
        {
            // Sao chép các trường thông tin cơ bản
            newVersion.BusinessContext = latestVersion.BusinessContext;
            newVersion.BusinessProblem = latestVersion.BusinessProblem;
            newVersion.BusinessObjectives = latestVersion.BusinessObjectives;
            newVersion.BARole = latestVersion.BARole;
            newVersion.Assignment = string.Empty;
            newVersion.InScope = latestVersion.InScope;
            newVersion.OutOfScope = latestVersion.OutOfScope;
            newVersion.KnownConstraints = latestVersion.KnownConstraints;
            newVersion.Difficulty = latestVersion.Difficulty;
            newVersion.IsFreeAccess = await IsUserAdminAsync(scenario.CreatedByUserId, cancellationToken);

            var sourceSprints = ScenarioSprintMapper.ResolveSprints(latestVersion);
            foreach (var sprint in sourceSprints)
            {
                newVersion.Sprints.Add(ScenarioSprintMapper.ToEntity(0, sprint));
            }

            // Sao chép KnowledgeItems
            var oldToNewKnowledge = new Dictionary<int, KnowledgeItem>();
            foreach (var k in latestVersion.KnowledgeItems)
            {
                var newK = new KnowledgeItem
                {
                    Code = k.Code,
                    Title = k.Title,
                    Content = k.Content,
                    Category = k.Category,
                    Importance = k.Importance,
                    CreatedAt = now
                };
                newVersion.KnowledgeItems.Add(newK);
                oldToNewKnowledge[k.Id] = newK;
            }

            // Sao chép Personas và quyền truy cập Knowledge
            foreach (var p in latestVersion.StakeholderPersonas)
            {
                var newP = new StakeholderPersona
                {
                    Name = p.Name,
                    StakeholderRole = p.StakeholderRole,
                    Department = p.Department,
                    Responsibilities = p.Responsibilities,
                    Goals = p.Goals,
                    PainPoints = p.PainPoints,
                    BusinessConcerns = p.BusinessConcerns,
                    CommunicationStyle = p.CommunicationStyle,
                    Personality = p.Personality,
                    DisplayOrder = p.DisplayOrder,
                    IsActive = p.IsActive
                };

                foreach (var acc in p.StakeholderKnowledgeAccess)
                {
                    if (oldToNewKnowledge.TryGetValue(acc.KnowledgeItemId, out var targetK))
                    {
                        newP.StakeholderKnowledgeAccess.Add(new StakeholderKnowledgeAccess
                        {
                            KnowledgeItem = targetK,
                            AccessType = acc.AccessType,
                            DisclosureRule = acc.DisclosureRule,
                            IsHidden = acc.IsHidden
                        });
                    }
                }

                newVersion.StakeholderPersonas.Add(newP);
            }

            // Sao chép BusinessRules
            foreach (var r in latestVersion.BusinessRules)
            {
                newVersion.BusinessRules.Add(new ScenarioBusinessRule
                {
                    Code = r.Code,
                    Title = r.Title,
                    Description = r.Description,
                    IsHidden = r.IsHidden,
                    Importance = r.Importance
                });
            }

            // Sao chép ScenarioEvents
            foreach (var e in latestVersion.ScenarioEvents)
            {
                newVersion.ScenarioEvents.Add(new ScenarioEvent
                {
                    Title = e.Title,
                    Description = e.Description,
                    TriggerType = e.TriggerType,
                    TriggerConditionJson = e.TriggerConditionJson,
                    EventContent = e.EventContent,
                    DisplayOrder = e.DisplayOrder,
                    IsActive = e.IsActive
                });
            }
        }
        else
        {
            newVersion.BusinessContext = string.Empty;
            newVersion.BusinessProblem = string.Empty;
            newVersion.BusinessObjectives = string.Empty;
            newVersion.BARole = "Business Analyst";
            newVersion.Assignment = string.Empty;
        }

        scenario.UpdatedAt = now;

        await repository.AddScenarioVersionAsync(newVersion, cancellationToken);

        return new ScenarioVersionSummaryDto(
            newVersion.Id,
            newVersion.ScenarioId,
            newVersion.VersionNo,
            newVersion.Summary,
            newVersion.Difficulty,
            newVersion.IsPublished,
            newVersion.CreatedAt,
            newVersion.PublishedAt
        );
    }

    public async Task<DiscardDraftResultDto> DiscardDraftVersionAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        CancellationToken cancellationToken = default
    )
    {
        var version = await repository.GetVersionWithAggregateForUpdateAsync(versionId, cancellationToken);
        if (version is null)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        var isOwner = version.Scenario.CreatedByUserId == userId;
        var isAdmin = ScenarioReviewRoles.IsAdmin(userRoles);
        if (!isOwner && !isAdmin)
        {
            throw new UnauthorizedAccessException("You do not have permission to discard this draft version.");
        }

        if (version.IsPublished || version.Status == ScenarioStatus.Published)
        {
            throw new ConflictException("Published versions cannot be discarded.");
        }

        if (version.Status == ScenarioStatus.PendingReview)
        {
            throw new ConflictException("Version is pending expert review. Wait for a decision or ask an expert to reject it before discarding.");
        }

        if (version.Status is not (ScenarioStatus.Draft or ScenarioStatus.Rejected))
        {
            throw new ConflictException($"Only draft or rejected versions can be discarded (current status: {version.Status}).");
        }

        var scenarioId = version.ScenarioId;
        var discardedId = version.Id;

        var siblings = await scenarioDb.ScenarioVersions
            .AsNoTracking()
            .Where(v => v.ScenarioId == scenarioId && v.Id != versionId)
            .OrderByDescending(v => v.VersionNo)
            .Select(v => new { v.Id, v.VersionNo, v.Summary, v.IsPublished, v.Status })
            .ToListAsync(cancellationToken);

        var scenarioDeleted = siblings.Count == 0;

        // Bỏ tracking aggregate trước khi ExecuteDelete để tránh conflict.
        scenarioDb.ChangeTracker.Clear();

        await repository.DeleteVersionChildrenAsync(versionId, cancellationToken);

        await scenarioDb.ScenarioVersions
            .Where(v => v.Id == versionId)
            .ExecuteDeleteAsync(cancellationToken);

        if (scenarioDeleted)
        {
            await scenarioDb.Scenarios
                .Where(s => s.Id == scenarioId)
                .ExecuteDeleteAsync(cancellationToken);
        }
        else
        {
            await scenarioDb.Scenarios
                .Where(s => s.Id == scenarioId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(s => s.UpdatedAt, DateTimeOffset.UtcNow),
                    cancellationToken);
        }

        var fallback = siblings.FirstOrDefault(v => v.IsPublished || v.Status == ScenarioStatus.Published)
            ?? siblings.FirstOrDefault();

        return new DiscardDraftResultDto(
            discardedId,
            scenarioId,
            scenarioDeleted,
            fallback?.Id,
            fallback?.VersionNo,
            fallback?.Summary
        );
    }

    public async Task<ScenarioDefinitionDto> GetDefinitionAsync(
        int userId,
        int versionId,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    )
    {
        var version = await repository.GetVersionDefinitionAsync(versionId, cancellationToken);

        if (version is null)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        var roles = userRoles ?? [];
        var isOwner = version.Scenario.CreatedByUserId == userId;
        var isAdmin = ScenarioReviewRoles.IsAdmin(roles);
        var isExpert = ScenarioReviewRoles.IsExpert(roles);
        var isExpertReviewer =
            isExpert
            && (version.Status == ScenarioStatus.PendingReview || version.ReviewedByUserId == userId);
        if (!isOwner && !isAdmin && !isExpertReviewer)
        {
            throw new UnauthorizedAccessException("You do not have permission to view the full definition of this scenario.");
        }

        var personasDto = version.StakeholderPersonas.Select(p => new AuthoringPersonaDto(
            p.Id,
            p.Name,
            p.StakeholderRole,
            p.Department,
            p.Responsibilities,
            p.Goals,
            p.PainPoints,
            p.BusinessConcerns,
            p.CommunicationStyle,
            p.Personality,
            p.DisplayOrder,
            p.IsActive
        )).ToList();

        var knowledgeDto = version.KnowledgeItems.Select(k => new AuthoringKnowledgeItemDto(
            k.Id,
            k.Code,
            k.Title,
            k.Content,
            k.Category,
            k.Importance,
            k.StakeholderKnowledgeAccess.Select(a => new KnowledgeAccessDto(
                a.StakeholderPersonaId,
                a.StakeholderPersona.Name,
                a.AccessType,
                a.DisclosureRule,
                a.IsHidden
            )).ToList()
        )).ToList();

        var businessRulesDto = version.BusinessRules.Select(r => new AuthoringBusinessRuleDto(
            r.Id,
            r.Code,
            r.Title,
            r.Description,
            r.IsHidden,
            r.Importance
        )).ToList();

        var eventsDto = version.ScenarioEvents.Select(e => new AuthoringScenarioEventDto(
            e.Id,
            e.Title,
            e.Description,
            e.TriggerType,
            e.TriggerConditionJson,
            e.EventContent,
            e.DisplayOrder,
            e.IsActive
        )).ToList();

        var sprintsDto = ScenarioSprintMapper.ResolveSprints(version);

        return new ScenarioDefinitionDto(
            version.Id,
            version.ScenarioId,
            version.Scenario.Code,
            version.Scenario.Title,
            version.VersionNo,
            version.Summary,
            version.BusinessContext,
            version.BusinessProblem,
            version.BusinessObjectives,
            version.BARole,
            string.Empty,
            version.InScope,
            version.OutOfScope,
            version.KnownConstraints,
            version.Difficulty,
            version.IsFreeAccess,
            version.IsPublished,
            version.CreatedAt,
            version.PublishedAt,
            personasDto,
            knowledgeDto,
            businessRulesDto,
            eventsDto,
            version.Status.ToString(),
            version.ReviewComment,
            version.SubmittedForReviewAt,
            version.ReviewedAt,
            sprintsDto
        );
    }

    public async Task<ScenarioDefinitionDto> UpdateDefinitionAsync(
        int userId,
        int versionId,
        UpdateScenarioDefinitionRequest request,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    )
    {
        var version = await repository.GetVersionWithAggregateForUpdateAsync(versionId, cancellationToken);

        if (version is null)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        var roles = userRoles ?? [];
        var isAdmin = ScenarioReviewRoles.IsAdmin(roles);

        // 1. Chỉ tác giả được sửa. Admin chỉ xem catalogue — không chỉnh sửa kịch bản của người khác / mọi người.
        if (version.Scenario.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException(
                "Only the owning instructor can update this scenario version. Admin has view-only access.");
        }

        // 2. Tính bất biến: không sửa bản đã xuất bản hoặc đang chờ Expert duyệt
        if (version.IsPublished || version.Status == ScenarioStatus.PendingReview)
        {
            throw new ConflictException("Cannot modify a scenario version that has already been published or is pending expert review.");
        }

        // 3. Kiểm tra tính hợp lệ dữ liệu gửi lên
        // - Code của Knowledge Items không được trùng nhau
        var duplicateKnowledgeCode = request.KnowledgeItems
            .GroupBy(k => k.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicateKnowledgeCode != null)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["KnowledgeItems"] = [$"Duplicate KnowledgeItem code '{duplicateKnowledgeCode.Key}' found in definition."]
            });
        }

        // - Code của Personas không được trùng nhau
        var duplicatePersonaCode = request.Personas
            .GroupBy(p => p.Code.Trim(), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicatePersonaCode != null)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Personas"] = [$"Duplicate Persona code '{duplicatePersonaCode.Key}' found in definition."]
            });
        }

        var now = DateTimeOffset.UtcNow;

        // 4. Cập nhật thông tin tổng quan Version
        version.Summary = request.Summary?.Trim();
        version.BusinessContext = request.BusinessContext.Trim();
        version.BusinessProblem = request.BusinessProblem.Trim();
        version.BusinessObjectives = request.BusinessObjectives.Trim();
        version.BARole = request.BARole.Trim();
        // Scenario không còn dùng Assignment — luôn rỗng; lộ trình ở ScenarioSprints.
        version.Assignment = string.Empty;
        version.InScope = request.InScope?.Trim();
        version.OutOfScope = request.OutOfScope?.Trim();
        version.KnownConstraints = request.KnownConstraints?.Trim();
        version.Difficulty = request.Difficulty;
        // Không còn tick tay: Admin creator → library toàn hệ thống; Instructor → chỉ list của họ.
        version.IsFreeAccess = await IsUserAdminAsync(version.Scenario.CreatedByUserId, cancellationToken);
        version.Scenario.UpdatedAt = now;

        // 5. Thay thế toàn bộ Personas, Knowledge, Rules, Events trong một Transaction nhất quán
        repository.ClearVersionAggregate(version);

        // Tạo KnowledgeItems mới
        var codeToNewKnowledge = new Dictionary<string, KnowledgeItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var kReq in request.KnowledgeItems)
        {
            var kEntity = new KnowledgeItem
            {
                ScenarioVersionId = version.Id,
                Code = kReq.Code.Trim(),
                Title = kReq.Title.Trim(),
                Content = kReq.Content.Trim(),
                Category = kReq.Category,
                Importance = kReq.Importance,
                CreatedAt = now
            };
            version.KnowledgeItems.Add(kEntity);
            codeToNewKnowledge[kReq.Code.Trim()] = kEntity;
        }

        // Tạo Personas mới và gắn Access map qua code
        var codeToNewPersona = new Dictionary<string, StakeholderPersona>(StringComparer.OrdinalIgnoreCase);
        foreach (var pReq in request.Personas)
        {
            var pEntity = new StakeholderPersona
            {
                ScenarioVersionId = version.Id,
                Name = pReq.Name.Trim(),
                StakeholderRole = pReq.StakeholderRole.Trim(),
                Department = pReq.Department?.Trim(),
                Responsibilities = pReq.Responsibilities?.Trim(),
                Goals = pReq.Goals?.Trim(),
                PainPoints = pReq.PainPoints?.Trim(),
                BusinessConcerns = pReq.BusinessConcerns?.Trim(),
                CommunicationStyle = pReq.CommunicationStyle?.Trim(),
                Personality = pReq.Personality?.Trim(),
                DisplayOrder = pReq.DisplayOrder,
                IsActive = pReq.IsActive
            };
            version.StakeholderPersonas.Add(pEntity);
            codeToNewPersona[pReq.Code.Trim()] = pEntity;
        }

        // Tạo liên kết StakeholderKnowledgeAccess dựa trên Code
        foreach (var kReq in request.KnowledgeItems)
        {
            var targetKnowledge = codeToNewKnowledge[kReq.Code.Trim()];
            foreach (var accReq in kReq.AccessList)
            {
                if (codeToNewPersona.TryGetValue(accReq.PersonaCode.Trim(), out var targetPersona))
                {
                    targetPersona.StakeholderKnowledgeAccess.Add(new StakeholderKnowledgeAccess
                    {
                        KnowledgeItem = targetKnowledge,
                        AccessType = accReq.AccessType,
                        DisclosureRule = accReq.DisclosureRule?.Trim(),
                        IsHidden = accReq.IsHidden
                    });
                }
            }
        }

        // Tạo BusinessRules mới
        foreach (var rReq in request.BusinessRules)
        {
            version.BusinessRules.Add(new ScenarioBusinessRule
            {
                ScenarioVersionId = version.Id,
                Code = rReq.Code.Trim(),
                Title = rReq.Title.Trim(),
                Description = rReq.Description.Trim(),
                IsHidden = rReq.IsHidden,
                Importance = rReq.Importance
            });
        }

        // Tạo ScenarioEvents mới
        foreach (var eReq in request.Events)
        {
            version.ScenarioEvents.Add(new ScenarioEvent
            {
                ScenarioVersionId = version.Id,
                Title = eReq.Title.Trim(),
                Description = eReq.Description.Trim(),
                TriggerType = eReq.TriggerType,
                // Cột DB có CK ISJSON — mô tả tiếng Việt từ UI phải được bọc JSON hợp lệ.
                TriggerConditionJson = NormalizeTriggerConditionJson(eReq.TriggerConditionJson),
                EventContent = eReq.EventContent.Trim(),
                DisplayOrder = eReq.DisplayOrder,
                IsActive = eReq.IsActive
            });
        }

        if (request.Sprints is { Count: > 0 })
        {
            ReplaceSprintEntities(version, request.Sprints);
        }

        if (version.Status == ScenarioStatus.Rejected)
        {
            version.Status = ScenarioStatus.Draft;
            if (version.Scenario.Status == ScenarioStatus.Rejected)
            {
                version.Scenario.Status = ScenarioStatus.Draft;
            }
        }

        await repository.SaveChangesAsync(cancellationToken);

        // Trả về definition vừa cập nhật
        return await GetDefinitionAsync(userId, versionId, cancellationToken, userRoles);
    }

    private static string? NormalizeTriggerConditionJson(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = raw.Trim();
        try
        {
            using var _ = System.Text.Json.JsonDocument.Parse(trimmed);
            return trimmed;
        }
        catch (System.Text.Json.JsonException)
        {
            // UI thường gửi mô tả tiếng Việt — bọc thành JSON object hợp lệ cho CK ISJSON.
            return System.Text.Json.JsonSerializer.Serialize(new { note = trimmed });
        }
    }

    public async Task<ScenarioVersionSummaryDto> PublishVersionAsync(
        int userId,
        int versionId,
        CancellationToken cancellationToken = default
    )
    {
        var version = await repository.GetVersionForPublishAsync(versionId, cancellationToken);

        if (version is null)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        // 1. Quyền sở hữu
        if (version.Scenario.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to publish this scenario version.");
        }

        // 2. Nếu đã publish rồi thì báo conflict
        if (version.IsPublished)
        {
            throw new ConflictException("This scenario version has already been published.");
        }

        // 3. Kiểm tra các điều kiện tiên quyết (Validation Checklist)
        var errors = ValidateReadyForReview(version);

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        // 4. Đóng băng phiên bản và cập nhật trạng thái kịch bản
        var now = DateTimeOffset.UtcNow;
        version.IsPublished = true;
        version.Status = ScenarioStatus.Published;
        version.PublishedAt = now;
        version.Scenario.Status = ScenarioStatus.Published;
        version.Scenario.UpdatedAt = now;

        await repository.SaveChangesAsync(cancellationToken);

        return new ScenarioVersionSummaryDto(
            version.Id,
            version.ScenarioId,
            version.VersionNo,
            version.Summary,
            version.Difficulty,
            version.IsPublished,
            version.CreatedAt,
            version.PublishedAt
        );
    }

    public async Task<ScenarioReviewDecisionDto> SubmitForReviewAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        CancellationToken cancellationToken = default
    )
    {
        if (!ScenarioReviewRoles.IsInstructorOrAdmin(userRoles))
        {
            throw new UnauthorizedAccessException("Only an instructor or admin can submit a scenario for review.");
        }

        var version = await repository.GetVersionForPublishAsync(versionId, cancellationToken);
        if (version is null)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        var isOwner = version.Scenario.CreatedByUserId == userId;
        if (!isOwner && !ScenarioReviewRoles.IsAdmin(userRoles))
        {
            throw new UnauthorizedAccessException("You do not have permission to submit this scenario version.");
        }

        if (version.IsPublished || version.Status == ScenarioStatus.Published)
        {
            throw new ConflictException("This scenario version has already been published.");
        }

        if (version.Status == ScenarioStatus.PendingReview)
        {
            throw new ConflictException("This scenario version is already pending expert review.");
        }

        var errors = ValidateReadyForReview(version);
        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        var now = DateTimeOffset.UtcNow;
        version.Status = ScenarioStatus.PendingReview;
        version.SubmittedForReviewAt = now;
        version.ReviewedAt = null;
        version.ReviewedByUserId = null;
        if (version.Scenario.Status != ScenarioStatus.Published)
        {
            version.Scenario.Status = ScenarioStatus.PendingReview;
        }

        version.Scenario.UpdatedAt = now;
        await repository.SaveChangesAsync(cancellationToken);
        return ToDecision(version);
    }

    public async Task<ScenarioReviewDecisionDto> ApproveVersionAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        CancellationToken cancellationToken = default
    )
    {
        if (!ScenarioReviewRoles.IsExpert(userRoles))
        {
            throw new UnauthorizedAccessException("Only an expert can approve a scenario version.");
        }

        var version = await repository.GetVersionForPublishAsync(versionId, cancellationToken);
        if (version is null)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        if (version.IsPublished)
        {
            throw new ConflictException("This scenario version has already been published.");
        }

        if (version.Status != ScenarioStatus.PendingReview)
        {
            throw new ConflictException("Only a version pending expert review can be approved.");
        }

        var errors = ValidateReadyForReview(version);
        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        var now = DateTimeOffset.UtcNow;
        version.IsPublished = true;
        version.Status = ScenarioStatus.Published;
        version.PublishedAt = now;
        version.ReviewedAt = now;
        version.ReviewedByUserId = userId;
        version.ReviewComment = null;
        version.Scenario.Status = ScenarioStatus.Published;
        version.Scenario.UpdatedAt = now;

        // Admin tạo → Scenario Library toàn hệ thống; Instructor → chỉ list creator / giao bài.
        version.IsFreeAccess = await IsUserAdminAsync(version.Scenario.CreatedByUserId, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);
        return ToDecision(version);
    }

    public async Task<ScenarioReviewDecisionDto> RejectVersionAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        string comment,
        CancellationToken cancellationToken = default
    )
    {
        if (!ScenarioReviewRoles.IsExpert(userRoles))
        {
            throw new UnauthorizedAccessException("Only an expert can reject a scenario version.");
        }

        if (string.IsNullOrWhiteSpace(comment) || comment.Trim().Length < 10)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Comment"] = ["A rejection comment of at least 10 characters is required."]
            });
        }

        var version = await repository.GetVersionForPublishAsync(versionId, cancellationToken);
        if (version is null)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        if (version.Status != ScenarioStatus.PendingReview)
        {
            throw new ConflictException("Only a version pending expert review can be rejected.");
        }

        var now = DateTimeOffset.UtcNow;
        version.Status = ScenarioStatus.Rejected;
        version.IsPublished = false;
        version.ReviewComment = comment.Trim();
        version.ReviewedAt = now;
        version.ReviewedByUserId = userId;
        if (version.Scenario.Status != ScenarioStatus.Published)
        {
            version.Scenario.Status = ScenarioStatus.Rejected;
        }

        version.Scenario.UpdatedAt = now;
        await repository.SaveChangesAsync(cancellationToken);
        return ToDecision(version);
    }

    public async Task<IReadOnlyList<ScenarioReviewQueueItemDto>> ListPendingReviewsAsync(
        IReadOnlyCollection<string> userRoles,
        CancellationToken cancellationToken = default
    )
    {
        if (!ScenarioReviewRoles.IsExpert(userRoles))
        {
            throw new UnauthorizedAccessException("Only an expert can read the scenario review queue.");
        }

        var versions = await scenarioDb.ScenarioVersions
            .AsNoTracking()
            .Include(v => v.Scenario)
            .Include(v => v.StakeholderPersonas)
            .Include(v => v.KnowledgeItems)
            .Include(v => v.BusinessRules)
            .Include(v => v.ScenarioEvents)
            .Where(v => v.Status == ScenarioStatus.PendingReview)
            .OrderBy(v => v.SubmittedForReviewAt)
            .ToListAsync(cancellationToken);

        return versions.Select(ToQueueItem).ToList();
    }

    public async Task<IReadOnlyList<ScenarioReviewQueueItemDto>> ListReviewedByMeAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        CancellationToken cancellationToken = default
    )
    {
        if (!ScenarioReviewRoles.IsExpert(userRoles) && !ScenarioReviewRoles.IsAdmin(userRoles))
        {
            throw new UnauthorizedAccessException("Only an expert can read their reviewed scenario history.");
        }

        var versions = await scenarioDb.ScenarioVersions
            .AsNoTracking()
            .Include(v => v.Scenario)
            .Include(v => v.StakeholderPersonas)
            .Include(v => v.KnowledgeItems)
            .Include(v => v.BusinessRules)
            .Include(v => v.ScenarioEvents)
            .Where(v =>
                v.ReviewedByUserId == userId
                && (v.Status == ScenarioStatus.Published || v.Status == ScenarioStatus.Rejected))
            .OrderByDescending(v => v.ReviewedAt ?? v.CreatedAt)
            .ToListAsync(cancellationToken);

        return versions.Select(ToQueueItem).ToList();
    }

    public async Task<IReadOnlyList<ScenarioReviewQueueItemDto>> ListMyVersionsAsync(
        int userId,
        CancellationToken cancellationToken = default
    )
    {
        // Không Include KnowledgeItems: một số seed dùng Category legacy (BusinessRule/Security)
        // khiến EF không map được enum và /mine trả 500. Count() không đọc giá trị Category.
        var versions = await scenarioDb.ScenarioVersions
            .AsNoTracking()
            .Include(v => v.Scenario)
            .Include(v => v.StakeholderPersonas)
            .Include(v => v.BusinessRules)
            .Include(v => v.ScenarioEvents)
            .Where(v => v.Scenario.CreatedByUserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        if (versions.Count == 0)
        {
            return [];
        }

        var versionIds = versions.Select(v => v.Id).ToList();
        var knowledgeCounts = await scenarioDb.KnowledgeItems
            .AsNoTracking()
            .Where(k => versionIds.Contains(k.ScenarioVersionId))
            .GroupBy(k => k.ScenarioVersionId)
            .Select(g => new { VersionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.VersionId, x => x.Count, cancellationToken);

        return versions.Select(v =>
        {
            knowledgeCounts.TryGetValue(v.Id, out var knowledgeCount);
            return new ScenarioReviewQueueItemDto(
                v.Id,
                v.ScenarioId,
                v.Scenario.Code,
                v.Scenario.Title,
                v.Scenario.Description,
                v.VersionNo,
                v.Status.ToString(),
                v.Summary,
                v.BusinessContext,
                v.BusinessProblem,
                v.BusinessObjectives,
                v.BARole,
                v.Assignment,
                v.InScope,
                v.OutOfScope,
                v.KnownConstraints,
                v.StakeholderPersonas.Count,
                knowledgeCount,
                v.BusinessRules.Count,
                v.ScenarioEvents.Count,
                v.Scenario.CreatedByUserId,
                v.SubmittedForReviewAt,
                v.ReviewComment,
                v.ReviewedAt,
                v.ReviewedByUserId
            );
        }).ToList();
    }

    private static Dictionary<string, string[]> ValidateReadyForReview(ScenarioVersion version)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(version.BusinessContext))
            errors["BusinessContext"] = ["Business context is required before submitting for review."];

        if (string.IsNullOrWhiteSpace(version.BusinessProblem))
            errors["BusinessProblem"] = ["Business problem is required before submitting for review."];

        if (string.IsNullOrWhiteSpace(version.BusinessObjectives))
            errors["BusinessObjectives"] = ["Business objectives are required before submitting for review."];

        if (string.IsNullOrWhiteSpace(version.BARole))
            errors["BARole"] = ["BA role is required before submitting for review."];

        var sprintCount = version.Sprints?.Count ?? 0;
        if (sprintCount == 0 && ScenarioSprintMapper.LooksLikeSprintJson(version.Assignment))
        {
            sprintCount = AssignmentSprintCodec.Parse(version.Assignment).Count;
        }

        if (sprintCount == 0)
            errors["Sprints"] = ["At least one sprint is required before submitting for review."];

        var activePersonas = version.StakeholderPersonas.Where(p => p.IsActive).ToList();
        if (activePersonas.Count == 0)
        {
            errors["Personas"] = ["At least one active stakeholder persona is required before submitting for review."];
        }
        else
        {
            var personasWithoutKnowledge = activePersonas
                .Where(p => !p.StakeholderKnowledgeAccess.Any())
                .Select(p => p.Name)
                .ToList();

            if (personasWithoutKnowledge.Count > 0)
            {
                errors["KnowledgeAccess"] = [
                    $"The following active persona(s) must have at least one assigned knowledge item: {string.Join(", ", personasWithoutKnowledge)}"
                ];
            }
        }

        return errors;
    }

    private static ScenarioReviewDecisionDto ToDecision(ScenarioVersion version) =>
        new(
            version.Id,
            version.ScenarioId,
            version.Scenario.Code,
            version.Scenario.Title,
            version.VersionNo,
            version.Status.ToString(),
            version.IsPublished,
            version.ReviewComment,
            version.SubmittedForReviewAt,
            version.ReviewedAt,
            version.ReviewedByUserId
        );

    private static ScenarioReviewQueueItemDto ToQueueItem(ScenarioVersion version) =>
        new(
            version.Id,
            version.ScenarioId,
            version.Scenario.Code,
            version.Scenario.Title,
            version.Scenario.Description,
            version.VersionNo,
            version.Status.ToString(),
            version.Summary,
            version.BusinessContext,
            version.BusinessProblem,
            version.BusinessObjectives,
            version.BARole,
            version.Assignment,
            version.InScope,
            version.OutOfScope,
            version.KnownConstraints,
            version.StakeholderPersonas.Count,
            version.KnowledgeItems.Count,
            version.BusinessRules.Count,
            version.ScenarioEvents.Count,
            version.Scenario.CreatedByUserId,
            version.SubmittedForReviewAt,
            version.ReviewComment,
            version.ReviewedAt,
            version.ReviewedByUserId
        );

    public async Task<ScenarioContextDto> GetContextAsync(
        int userId,
        IReadOnlyCollection<string> userRoles,
        int versionId,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Lấy version + scenario cha (không cần Include các bảng liên quan)
        var version = await scenarioDb.ScenarioVersions
            .AsNoTracking()
            .Include(v => v.Scenario)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

        if (version is null)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        // 2. Phân quyền xem context:
        //    Admin / Expert: draft phục vụ duyệt; Admin xem mọi published (view-only).
        //    Instructor owner: scenario mình tạo.
        //    Learner: published + (IsFreeAccess HOẶC đã được giao bài cho lớp/cá nhân).
        var isAdmin = ScenarioReviewRoles.IsAdmin(userRoles);
        var isExpert = ScenarioReviewRoles.IsExpert(userRoles);
        var isOwner = version.Scenario.CreatedByUserId == userId;
        var canView = false;

        if (!version.IsPublished)
        {
            canView = isAdmin || isExpert || isOwner;
        }
        else if (isAdmin || isExpert || isOwner || version.IsFreeAccess)
        {
            canView = true;
        }
        else
        {
            canView = await assignmentService.IsLearnerAssignedToScenarioVersionAsync(
                userId,
                versionId,
                cancellationToken);
        }

        if (!canView)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        // 3. Chỉ trả các trường context "core" — KHÔNG trả personas/knowledge/rules/events
        return new ScenarioContextDto(
            version.Id,
            version.ScenarioId,
            version.Scenario.Code,
            version.Scenario.Title,
            version.VersionNo,
            version.Summary,
            version.BusinessContext,
            version.BusinessProblem,
            version.BusinessObjectives,
            version.BARole,
            version.Assignment,
            version.InScope,
            version.OutOfScope,
            version.KnownConstraints,
            version.Difficulty.ToString(),
            version.PublishedAt,
            version.IsFreeAccess
        );
    }

    public async Task<ScenarioSprintListDto> GetSprintsAsync(
        int userId,
        int versionId,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    )
    {
        var version = await RequireVersionForSprintReadAsync(userId, versionId, userRoles, cancellationToken);
        var sprints = ScenarioSprintMapper.ResolveSprints(version);
        return new ScenarioSprintListDto(version.Id, version.ScenarioId, sprints);
    }

    public async Task<ScenarioSprintListDto> ReplaceSprintsAsync(
        int userId,
        int versionId,
        ReplaceScenarioSprintsRequest request,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    )
    {
        var version = await RequireVersionForSprintWriteAsync(userId, versionId, userRoles, cancellationToken);
        if (request.Sprints is null || request.Sprints.Count == 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Sprints"] = ["At least one sprint is required."]
            });
        }

        ReplaceSprintEntities(version, request.Sprints);
        version.Assignment = string.Empty;

        version.Scenario.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return new ScenarioSprintListDto(
            version.Id,
            version.ScenarioId,
            ScenarioSprintMapper.ToDtoList(version.Sprints));
    }

    public async Task<ScenarioSprintListDto> AddSprintAsync(
        int userId,
        int versionId,
        UpsertScenarioSprintRequest request,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    )
    {
        var version = await RequireVersionForSprintWriteAsync(userId, versionId, userRoles, cancellationToken);
        EnsureSprintsLoadedFromLegacy(version);
        var nextNo = version.Sprints.Count == 0 ? 1 : version.Sprints.Max(s => s.SprintNo) + 1;
        version.Sprints.Add(ScenarioSprintMapper.ToEntity(version.Id, nextNo, request));
        version.Scenario.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return new ScenarioSprintListDto(
            version.Id,
            version.ScenarioId,
            ScenarioSprintMapper.ToDtoList(version.Sprints));
    }

    public async Task<ScenarioSprintListDto> UpdateSprintAsync(
        int userId,
        int versionId,
        int sprintNo,
        UpsertScenarioSprintRequest request,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    )
    {
        var version = await RequireVersionForSprintWriteAsync(userId, versionId, userRoles, cancellationToken);
        EnsureSprintsLoadedFromLegacy(version);
        var entity = version.Sprints.FirstOrDefault(s => s.SprintNo == sprintNo);
        if (entity is null)
        {
            throw new NotFoundException($"Sprint {sprintNo} was not found on version {versionId}.");
        }

        entity.Focus = request.Focus?.Trim() ?? string.Empty;
        entity.DurationValue = request.DurationValue > 0 ? request.DurationValue : 1;
        entity.DurationUnit = ScenarioSprintMapper.NormalizeUnit(request.DurationUnit);
        entity.TemplateCodes = ScenarioSprintMapper.JoinTemplateCodes(request.TemplateCodes);
        version.Scenario.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return new ScenarioSprintListDto(
            version.Id,
            version.ScenarioId,
            ScenarioSprintMapper.ToDtoList(version.Sprints));
    }

    public async Task<ScenarioSprintListDto> DeleteSprintAsync(
        int userId,
        int versionId,
        int sprintNo,
        CancellationToken cancellationToken = default,
        IReadOnlyCollection<string>? userRoles = null
    )
    {
        var version = await RequireVersionForSprintWriteAsync(userId, versionId, userRoles, cancellationToken);
        EnsureSprintsLoadedFromLegacy(version);
        if (version.Sprints.Count <= 1)
        {
            throw new ConflictException("Cannot delete the last sprint. Keep at least one sprint.");
        }

        var entity = version.Sprints.FirstOrDefault(s => s.SprintNo == sprintNo);
        if (entity is null)
        {
            throw new NotFoundException($"Sprint {sprintNo} was not found on version {versionId}.");
        }

        scenarioDb.ScenarioSprints.Remove(entity);
        version.Sprints.Remove(entity);
        var ordered = version.Sprints.OrderBy(s => s.SprintNo).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].SprintNo = i + 1;
        }

        version.Scenario.UpdatedAt = DateTimeOffset.UtcNow;
        await repository.SaveChangesAsync(cancellationToken);
        return new ScenarioSprintListDto(
            version.Id,
            version.ScenarioId,
            ScenarioSprintMapper.ToDtoList(version.Sprints));
    }

    private void ReplaceSprintEntities(ScenarioVersion version, IReadOnlyList<UpsertScenarioSprintRequest> sprints)
    {
        if (version.Sprints.Count > 0)
        {
            scenarioDb.ScenarioSprints.RemoveRange(version.Sprints);
            version.Sprints.Clear();
        }

        for (var i = 0; i < sprints.Count; i++)
        {
            version.Sprints.Add(ScenarioSprintMapper.ToEntity(version.Id, i + 1, sprints[i]));
        }
    }

    /// <summary>Nếu bảng trống nhưng Assignment còn JSON/legacy — hydrate vào collection trước khi sửa.</summary>
    private void EnsureSprintsLoadedFromLegacy(ScenarioVersion version)
    {
        if (version.Sprints.Count > 0)
        {
            return;
        }

        var parsed = AssignmentSprintCodec.Parse(version.Assignment);
        if (parsed.Count == 0 || (parsed.Count == 1 && string.IsNullOrWhiteSpace(parsed[0].Focus)))
        {
            return;
        }

        foreach (var dto in parsed)
        {
            version.Sprints.Add(ScenarioSprintMapper.ToEntity(version.Id, dto));
        }
    }

    private async Task<ScenarioVersion> RequireVersionForSprintReadAsync(
        int userId,
        int versionId,
        IReadOnlyCollection<string>? userRoles,
        CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionDefinitionAsync(versionId, cancellationToken)
            ?? throw new NotFoundException($"Scenario version with ID {versionId} was not found.");

        var roles = userRoles ?? [];
        var isOwner = version.Scenario.CreatedByUserId == userId;
        var isAdmin = ScenarioReviewRoles.IsAdmin(roles);
        var isExpert = ScenarioReviewRoles.IsExpert(roles);
        if (!isOwner && !isAdmin && !isExpert && !version.IsPublished)
        {
            throw new NotFoundException($"Scenario version with ID {versionId} was not found.");
        }

        return version;
    }

    private async Task<ScenarioVersion> RequireVersionForSprintWriteAsync(
        int userId,
        int versionId,
        IReadOnlyCollection<string>? userRoles,
        CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionWithAggregateForUpdateAsync(versionId, cancellationToken)
            ?? throw new NotFoundException($"Scenario version with ID {versionId} was not found.");

        if (version.Scenario.CreatedByUserId != userId)
        {
            throw new UnauthorizedAccessException(
                "Only the owning instructor can update sprints. Admin has view-only access.");
        }

        if (version.IsPublished || version.Status == ScenarioStatus.PendingReview)
        {
            throw new ConflictException(
                "Cannot modify sprints on a published or pending-review scenario version.");
        }

        return version;
    }

    private async Task<bool> IsUserAdminAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByIdAsync(userId, cancellationToken);
        return user is not null && ScenarioReviewRoles.IsAdmin(user.Roles);
    }
}
