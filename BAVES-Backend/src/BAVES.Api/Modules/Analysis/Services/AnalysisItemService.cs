using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using BAVES.Api.Modules.Analysis.Models.DTOs;
using BAVES.Api.Modules.Analysis.Models.Entities;
using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Analysis.Models.Requests;
using BAVES.Api.Modules.Analysis.Repositories.Interfaces;
using BAVES.Api.Modules.Analysis.Services.Interfaces;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Analysis.Services;

/// <summary>
/// Triển khai nghiệp vụ Analysis Item.
/// IAnalysisItemRepository: truy vấn bảng analysis.AnalysisItems và analysis.MessageEvidenceLinks
/// ISimulationSessionService: kiểm tra session thuộc về learner + trạng thái phiên + đọc tin nhắn bằng chứng
/// IScenarioCatalogService: đọc hồ sơ StakeholderPersona và AllowedKnowledge cho luồng xác nhận AI
/// IAiRuntimeClient: gọi sang Python AI Runtime (POST /v1/analysis/confirm)
/// </summary>
public sealed class AnalysisItemService(
    IAnalysisItemRepository analysisItemRepository,
    ISimulationSessionService simulationSessionService,
    IScenarioCatalogService scenarioCatalogService,
    IAiRuntimeClient aiRuntimeClient
) : IAnalysisItemService
{
    private static string GetCodePrefix(AnalysisItemType itemType) => itemType switch
    {
        AnalysisItemType.Finding => "FND",
        AnalysisItemType.Requirement => "REQ",
        AnalysisItemType.BusinessRule => "BR",
        AnalysisItemType.Constraint => "CON",
        AnalysisItemType.Risk => "RSK",
        AnalysisItemType.OpenQuestion => "OQ",
        AnalysisItemType.Conflict => "CFL",
        AnalysisItemType.Assumption => "ASM",
        _ => "ITM"
    };

    private const int MaxCodeGenerationAttempts = 1000;

    public async Task<IReadOnlyList<AnalysisItemDto>?> GetItemsBySessionAsync(
        int learnerId,
        int sessionId,
        AnalysisItemType? itemType,
        AnalysisItemStatus? status,
        CancellationToken cancellationToken = default)
    {
        var sessionExistsAndOwned = await simulationSessionService.SessionExistsAndOwnedByAsync(
            learnerId, sessionId, cancellationToken);

        if (!sessionExistsAndOwned)
        {
            return null;
        }

        var items = await analysisItemRepository.GetItemsBySessionAsync(
            sessionId, itemType, status, cancellationToken);

        return items;
    }

    public async Task<AnalysisItemDto?> CreateItemAsync(
        int learnerId,
        int sessionId,
        CreateAnalysisItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ItemType is null || !Enum.IsDefined(request.ItemType.Value))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["ItemType"] = ["ItemType is required and must be one of: Finding, Requirement, BusinessRule, Constraint, Risk, OpenQuestion, Conflict, Assumption."]
            });
        }

        if (request.Priority.HasValue && !Enum.IsDefined(request.Priority.Value))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Priority"] = ["Priority must be one of: Low, Medium, High, Critical."]
            });
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Title"] = ["Title is required and cannot be blank."]
            });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Description"] = ["Description is required and cannot be blank."]
            });
        }

        var metadataJson = NormalizeMetadataJson(request.MetadataJson);
        var itemType = request.ItemType.Value;

        var session = await simulationSessionService.GetSessionByIdAsync(
            learnerId, sessionId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(
                $"Cannot create an analysis item because the session is '{session.Status}'.");
        }

        int? parentAnalysisItemId = null;
        if (request.ParentAnalysisItemId is > 0)
        {
            var parentItem = await analysisItemRepository.GetItemByIdAsync(
                sessionId, request.ParentAnalysisItemId.Value, cancellationToken);
            if (parentItem is null)
            {
                throw new NotFoundException(
                    $"Parent analysis item '{request.ParentAnalysisItemId.Value}' was not found in this session.");
            }

            parentAnalysisItemId = parentItem.Id;
        }

        var code = await GenerateUniqueCodeAsync(sessionId, itemType, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var item = new AnalysisItem
        {
            SessionId = sessionId,
            ParentAnalysisItemId = parentAnalysisItemId,
            ItemType = itemType,
            Code = code,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = AnalysisItemStatus.Draft,
            MetadataJson = metadataJson,
            CreatedByUserId = learnerId,
            CreatedAt = now,
            UpdatedAt = null
        };

        analysisItemRepository.AddItem(item);

        try
        {
            await analysisItemRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException(
                $"Analysis item code '{code}' already exists in this session. Please retry.");
        }

        return new AnalysisItemDto(
            item.Id,
            item.SessionId,
            item.ParentAnalysisItemId,
            item.ItemType,
            item.Code,
            item.Title,
            item.Description,
            item.Priority,
            item.Status,
            item.MetadataJson,
            item.CreatedByUserId,
            item.CreatedAt,
            item.UpdatedAt
        );
    }

    public async Task<AnalysisItemDto?> UpdateItemAsync(
        int learnerId,
        int sessionId,
        int itemId,
        UpdateAnalysisItemRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Priority.HasValue && !Enum.IsDefined(request.Priority.Value))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Priority"] = ["Priority must be one of: Low, Medium, High, Critical."]
            });
        }

        if (request.Status.HasValue && !Enum.IsDefined(request.Status.Value))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Status"] = ["Status must be one of: Draft, Confirmed, Resolved, Archived."]
            });
        }

        if (request.Status == AnalysisItemStatus.Archived)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Status"] = ["Status cannot be set to Archived via PATCH. Use DELETE /api/sessions/{sessionId}/analysis-items/{itemId} to archive it."]
            });
        }

        if (request.Title is not null && string.IsNullOrWhiteSpace(request.Title))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Title"] = ["Title cannot be blank."]
            });
        }

        if (request.Description is not null && string.IsNullOrWhiteSpace(request.Description))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Description"] = ["Description cannot be blank."]
            });
        }

        var session = await simulationSessionService.GetSessionByIdAsync(
            learnerId, sessionId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(
                $"Cannot update an analysis item because the session is '{session.Status}'.");
        }

        var item = await analysisItemRepository.GetItemByIdAsync(
            sessionId, itemId, cancellationToken);

        if (item is null)
        {
            return null;
        }

        if (item.Status == AnalysisItemStatus.Archived)
        {
            throw new ConflictException(
                "Cannot update an analysis item because it has been archived.");
        }

        if (request.ParentAnalysisItemId.HasValue)
        {
            if (request.ParentAnalysisItemId.Value <= 0)
            {
                item.ParentAnalysisItemId = null;
            }
            else
            {
                if (request.ParentAnalysisItemId.Value == item.Id)
                {
                    throw new RequestValidationException(new Dictionary<string, string[]>
                    {
                        ["ParentAnalysisItemId"] = ["An analysis item cannot be its own parent."]
                    });
                }

                var parentItem = await analysisItemRepository.GetItemByIdAsync(
                    sessionId, request.ParentAnalysisItemId.Value, cancellationToken);
                if (parentItem is null)
                {
                    throw new NotFoundException(
                        $"Parent analysis item '{request.ParentAnalysisItemId.Value}' was not found in this session.");
                }

                item.ParentAnalysisItemId = parentItem.Id;
            }
        }

        if (request.Title is not null)
        {
            item.Title = request.Title.Trim();
        }

        if (request.Description is not null)
        {
            item.Description = request.Description.Trim();
        }

        if (request.Priority.HasValue)
        {
            item.Priority = request.Priority.Value;
        }

        if (request.Status.HasValue)
        {
            item.Status = request.Status.Value;
        }

        if (request.MetadataJson is not null)
        {
            item.MetadataJson = NormalizeMetadataJson(request.MetadataJson);
        }

        item.UpdatedAt = DateTimeOffset.UtcNow;

        await analysisItemRepository.SaveChangesAsync(cancellationToken);

        return new AnalysisItemDto(
            item.Id,
            item.SessionId,
            item.ParentAnalysisItemId,
            item.ItemType,
            item.Code,
            item.Title,
            item.Description,
            item.Priority,
            item.Status,
            item.MetadataJson,
            item.CreatedByUserId,
            item.CreatedAt,
            item.UpdatedAt
        );
    }

    public async Task<AnalysisItemDto?> ArchiveItemAsync(
        int learnerId,
        int sessionId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        var session = await simulationSessionService.GetSessionByIdAsync(
            learnerId, sessionId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(
                $"Cannot delete an analysis item because the session is '{session.Status}'.");
        }

        var item = await analysisItemRepository.GetItemByIdAsync(
            sessionId, itemId, cancellationToken);

        if (item is null)
        {
            return null;
        }

        if (item.Status == AnalysisItemStatus.Archived)
        {
            throw new ConflictException(
                "Cannot delete an analysis item because it has already been archived.");
        }

        analysisItemRepository.ArchiveItem(item);

        await analysisItemRepository.SaveChangesAsync(cancellationToken);

        return new AnalysisItemDto(
            item.Id,
            item.SessionId,
            item.ParentAnalysisItemId,
            item.ItemType,
            item.Code,
            item.Title,
            item.Description,
            item.Priority,
            item.Status,
            item.MetadataJson,
            item.CreatedByUserId,
            item.CreatedAt,
            item.UpdatedAt
        );
    }

    public async Task<IReadOnlyList<EvidenceLinkDetailDto>?> GetEvidenceLinksAsync(
        int learnerId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        var item = await analysisItemRepository.GetItemEntityByIdAsync(itemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(
            learnerId, item.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        var links = await analysisItemRepository.GetEvidenceLinksByItemIdAsync(item.Id, cancellationToken);
        if (links.Count == 0)
        {
            return Array.Empty<EvidenceLinkDetailDto>();
        }

        var messageIds = links.Select(l => l.MessageId).Distinct().ToList();
        var messages = await simulationSessionService.GetMessagesByIdsAsync(item.SessionId, messageIds, cancellationToken);
        var messageById = messages.ToDictionary(m => m.Id);

        return links.Select(l =>
        {
            messageById.TryGetValue(l.MessageId, out var msg);
            return new EvidenceLinkDetailDto(
                l.Id,
                l.AnalysisItemId,
                l.MessageId,
                l.RelationshipType,
                msg?.SenderType.ToString(),
                msg?.Content,
                l.CreatedAt
            );
        }).ToList();
    }

    public async Task<EvidenceLinkDto?> AddEvidenceAsync(
        int learnerId,
        int itemId,
        CreateEvidenceLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.MessageId <= 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["MessageId"] = ["MessageId must be a positive integer."]
            });
        }

        if (!Enum.IsDefined(request.RelationshipType))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["RelationshipType"] = ["RelationshipType must be one of: Supports, Contradicts, Clarifies, Confirms."]
            });
        }

        var item = await analysisItemRepository.GetItemEntityByIdAsync(
            itemId, cancellationToken);

        if (item is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(
            learnerId, item.SessionId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(
                $"Cannot add evidence because the session is '{session.Status}'.");
        }

        if (item.Status == AnalysisItemStatus.Archived)
        {
            throw new ConflictException(
                "Cannot add evidence because the analysis item has been archived.");
        }

        var messageBelongsToSession = await simulationSessionService
            .MessageBelongsToSessionAsync(request.MessageId, item.SessionId, cancellationToken);

        if (!messageBelongsToSession)
        {
            throw new NotFoundException(
                $"Message '{request.MessageId}' was not found in this session.");
        }

        var linkExists = await analysisItemRepository.EvidenceLinkExistsAsync(
            item.Id, request.MessageId, request.RelationshipType, cancellationToken);

        if (linkExists)
        {
            throw new ConflictException(
                "This evidence link already exists for the analysis item.");
        }

        var link = new MessageEvidenceLink
        {
            MessageId = request.MessageId,
            AnalysisItemId = item.Id,
            RelationshipType = request.RelationshipType,
            CreatedAt = DateTimeOffset.UtcNow
        };

        analysisItemRepository.AddEvidenceLink(link);

        try
        {
            await analysisItemRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException(
                "This evidence link already exists for the analysis item. Please retry.");
        }

        return new EvidenceLinkDto(
            link.Id,
            link.AnalysisItemId,
            link.MessageId,
            link.RelationshipType,
            link.CreatedAt
        );
    }

    public async Task<EvidenceLinkDto?> RemoveEvidenceAsync(
        int learnerId,
        int itemId,
        int linkId,
        CancellationToken cancellationToken = default)
    {
        var item = await analysisItemRepository.GetItemEntityByIdAsync(
            itemId, cancellationToken);

        if (item is null)
        {
            return null;
        }

        var session = await simulationSessionService.GetSessionByIdAsync(
            learnerId, item.SessionId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(
                $"Cannot delete evidence because the session is '{session.Status}'.");
        }

        if (item.Status == AnalysisItemStatus.Archived)
        {
            throw new ConflictException(
                "Cannot delete evidence because the analysis item has been archived.");
        }

        var link = await analysisItemRepository.GetEvidenceLinkAsync(
            item.Id, linkId, cancellationToken);

        if (link is null)
        {
            return null;
        }

        var dto = new EvidenceLinkDto(
            link.Id,
            link.AnalysisItemId,
            link.MessageId,
            link.RelationshipType,
            link.CreatedAt
        );

        analysisItemRepository.RemoveEvidenceLink(link);

        await analysisItemRepository.SaveChangesAsync(cancellationToken);

        return dto;
    }

    public async Task<AnalysisConfirmationResultDto?> ConfirmItemAsync(
        int learnerId,
        int itemId,
        ConfirmAnalysisItemRequest request,
        string? headerProvider,
        string? headerModel,
        string? headerApiKey,
        CancellationToken cancellationToken = default)
    {
        var hasAgentId = request.AgentId is > 0;
        var hasPersonaId = request.PersonaId is > 0;
        if (!hasAgentId && !hasPersonaId)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["AgentId"] = ["Either AgentId or PersonaId must be provided as a positive integer."]
            });
        }

        // 1. Lấy AnalysisItem
        var item = await analysisItemRepository.GetItemEntityByIdAsync(itemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        // 2. Kiểm tra Session thuộc về Learner và còn Active
        var session = await simulationSessionService.GetSessionByIdAsync(
            learnerId, item.SessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(
                $"Cannot confirm an analysis item because the session is '{session.Status}'.");
        }

        if (item.Status == AnalysisItemStatus.Archived)
        {
            throw new ConflictException(
                "Cannot confirm an analysis item because it has been archived.");
        }

        // 3. Xác định Stakeholder Agent trong phiên mô phỏng
        var targetAgent = hasAgentId
            ? session.Agents.FirstOrDefault(a => a.Id == request.AgentId!.Value)
            : session.Agents.FirstOrDefault(a => a.PersonaId == request.PersonaId!.Value);

        if (targetAgent is null)
        {
            throw new NotFoundException(
                hasAgentId
                    ? $"Stakeholder agent '{request.AgentId}' was not found in this session."
                    : $"Stakeholder persona '{request.PersonaId}' was not found in this session.");
        }

        // 4. Lấy hồ sơ Persona và AllowedKnowledge từ module Scenario
        var persona = await scenarioCatalogService.GetPersonaInterviewContextAsync(
            targetAgent.PersonaId, cancellationToken);
        if (persona is null)
        {
            throw new NotFoundException(
                $"Stakeholder persona '{targetAgent.PersonaId}' was not found.");
        }

        var allowedKnowledge = await scenarioCatalogService.GetAllowedKnowledgeAsync(
            targetAgent.PersonaId, cancellationToken);

        // 5. Tổng hợp nội dung bằng chứng (Evidence Content) từ các tin nhắn đã gắn liên kết
        var links = await analysisItemRepository.GetEvidenceLinksByItemIdAsync(item.Id, cancellationToken);
        var messageIds = links.Select(l => l.MessageId).Distinct().ToList();
        var messages = await simulationSessionService.GetMessagesByIdsAsync(
            item.SessionId, messageIds, cancellationToken);
        var messageById = messages.ToDictionary(m => m.Id);

        var evidenceBuilder = new StringBuilder();
        foreach (var link in links)
        {
            if (messageById.TryGetValue(link.MessageId, out var msg))
            {
                var speakerLabel = msg.SenderType == MessageSenderType.Learner
                    ? "Learner (BA)"
                    : $"Stakeholder ({persona.Name})";
                evidenceBuilder.AppendLine(
                    $"[Message #{msg.Id} | {link.RelationshipType} | {speakerLabel}]: {msg.Content}");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.EvidenceNote))
        {
            evidenceBuilder.AppendLine($"[Ghi chú bổ sung của Learner]: {request.EvidenceNote.Trim()}");
        }

        var evidenceContent = evidenceBuilder.ToString().Trim();

        // 6. Chuẩn bị kết nối AI BYOK
        var provider = !string.IsNullOrWhiteSpace(headerProvider) ? headerProvider.Trim() : "OpenAI";
        var model = !string.IsNullOrWhiteSpace(headerModel) ? headerModel.Trim() : null;
        var apiKey = !string.IsNullOrWhiteSpace(headerApiKey) ? headerApiKey.Trim() : null;

        var runtimeRequest = new AnalysisConfirmRuntimeRequest(
            persona,
            new AnalysisConfirmItemPayloadDto(
                item.Id,
                item.ItemType.ToString(),
                item.Title,
                item.Description,
                item.Priority?.ToString() ?? "Medium"),
            evidenceContent,
            allowedKnowledge,
            new StakeholderAiConnectionDto(provider, model, apiKey)
        );

        // 7. Gọi sang AI Runtime (POST /v1/analysis/confirm)
        var runtimeResponse = await aiRuntimeClient.ConfirmAnalysisItemAsync(
            runtimeRequest, cancellationToken);

        var normalizedOutcome = NormalizeConfirmationOutcome(runtimeResponse.Outcome);
        var confirmedAt = DateTimeOffset.UtcNow;

        // 8. Cập nhật trạng thái AnalysisItem sang Confirmed nếu AI Stakeholder xác nhận (Confirmed)
        //    và lưu lịch sử xác nhận vào MetadataJson
        if (string.Equals(normalizedOutcome, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            item.Status = AnalysisItemStatus.Confirmed;
        }

        item.MetadataJson = MergeConfirmationIntoMetadata(
            item.MetadataJson,
            normalizedOutcome,
            targetAgent.Id,
            persona.PersonaId,
            persona.Name,
            persona.StakeholderRole,
            runtimeResponse.Reasoning,
            runtimeResponse.SuggestedClarification,
            messageIds,
            confirmedAt);
        item.UpdatedAt = confirmedAt;

        await analysisItemRepository.SaveChangesAsync(cancellationToken);

        return new AnalysisConfirmationResultDto(
            item.Id,
            item.Code,
            item.Title,
            item.Status,
            targetAgent.Id,
            persona.PersonaId,
            persona.Name,
            persona.StakeholderRole,
            normalizedOutcome,
            runtimeResponse.Reasoning,
            runtimeResponse.SuggestedClarification,
            messageIds,
            confirmedAt
        );
    }

    private static string NormalizeConfirmationOutcome(string? outcome)
    {
        if (string.Equals(outcome, "Confirmed", StringComparison.OrdinalIgnoreCase))
        {
            return "Confirmed";
        }

        if (string.Equals(outcome, "Disputed", StringComparison.OrdinalIgnoreCase))
        {
            return "Disputed";
        }

        return "InsufficientInformation";
    }

    private static string MergeConfirmationIntoMetadata(
        string? existingMetadataJson,
        string outcome,
        int agentId,
        int personaId,
        string personaName,
        string stakeholderRole,
        string reasoning,
        string? suggestedClarification,
        IReadOnlyList<int> evidenceMessageIds,
        DateTimeOffset confirmedAt)
    {
        JsonObject rootObj;
        if (!string.IsNullOrWhiteSpace(existingMetadataJson))
        {
            try
            {
                var parsed = JsonNode.Parse(existingMetadataJson);
                rootObj = parsed as JsonObject ?? new JsonObject { ["previousMetadata"] = parsed };
            }
            catch (JsonException)
            {
                rootObj = new JsonObject();
            }
        }
        else
        {
            rootObj = new JsonObject();
        }

        var msgArray = new JsonArray();
        foreach (var id in evidenceMessageIds)
        {
            msgArray.Add(id);
        }

        rootObj["lastConfirmation"] = new JsonObject
        {
            ["outcome"] = outcome,
            ["agentId"] = agentId,
            ["personaId"] = personaId,
            ["personaName"] = personaName,
            ["stakeholderRole"] = stakeholderRole,
            ["reasoning"] = reasoning,
            ["suggestedClarification"] = suggestedClarification,
            ["evidenceMessageIds"] = msgArray,
            ["confirmedAt"] = confirmedAt.ToString("O")
        };

        return rootObj.ToJsonString();
    }

    private static string? NormalizeMetadataJson(string? metadataJson)
    {
        if (metadataJson is null)
        {
            return null;
        }

        var trimmed = metadataJson.Trim();
        if (trimmed.Length == 0)
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(trimmed);
            var isObjectOrArray = document.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array;

            if (!isObjectOrArray)
            {
                throw new RequestValidationException(new Dictionary<string, string[]>
                {
                    ["MetadataJson"] = ["MetadataJson must be a JSON object or array."]
                });
            }
        }
        catch (JsonException)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["MetadataJson"] = ["MetadataJson is not valid JSON."]
            });
        }

        return trimmed;
    }

    private async Task<string> GenerateUniqueCodeAsync(
        int sessionId,
        AnalysisItemType itemType,
        CancellationToken cancellationToken)
    {
        var prefix = GetCodePrefix(itemType);
        var existingCount = await analysisItemRepository.CountItemsByTypeAsync(
            sessionId, itemType, cancellationToken);

        for (var sequence = existingCount + 1; sequence <= existingCount + MaxCodeGenerationAttempts; sequence++)
        {
            var candidate = $"{prefix}-{sequence:D3}";

            if (!await analysisItemRepository.CodeExistsAsync(sessionId, candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new ConflictException(
            "Could not generate a unique analysis item code for this session. Please retry.");
    }
}
