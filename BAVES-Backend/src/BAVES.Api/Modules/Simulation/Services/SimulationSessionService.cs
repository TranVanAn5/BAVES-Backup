using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Enums;
using BAVES.Api.Modules.Scenario.Services;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Entities;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Models.Requests;
using BAVES.Api.Modules.Simulation.Repositories.Interfaces;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;

namespace BAVES.Api.Modules.Simulation.Services;

/// <summary>
/// Triển khai nghiệp vụ Simulation Session cho Learner.
/// Tạm thời tập trung vào nghiệp vụ "bắt đầu session" (start) — các nghiệp vụ
/// khác như gửi tin nhắn, xem transcript, submit sẽ được bổ sung ở các bước sau.
///
/// Dữ liệu scenario (version, stakeholder persona) được đọc qua IScenarioCatalogService
/// của module Scenario — không truy vấn trực tiếp bảng hay DbContext của module khác.
/// </summary>
public sealed class SimulationSessionService(
    ISimulationSessionRepository simulationRepository,
    IScenarioCatalogService scenarioCatalogService,
    IAiRuntimeClient aiRuntimeClient
) : ISimulationSessionService
{
    /// <summary>
    /// Số tin nhắn gần nhất được gửi làm lịch sử cho AI Runtime.
    /// </summary>
    private const int RecentHistorySize = 20;
    public async Task<SessionDto> StartSessionAsync(
        int learnerId,
        StartSessionRequest request,
        CancellationToken cancellationToken = default
    )
    {
        // 1. Validate request cơ bản
        if (request.ScenarioVersionId <= 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["ScenarioVersionId"] = ["ScenarioVersionId is required and must be a positive integer."]
            });
        }

        // 2. Lấy thông tin ScenarioVersion (module Scenario sở hữu bảng này).
        var version = await scenarioCatalogService.GetVersionInfoAsync(
            request.ScenarioVersionId, cancellationToken);

        if (version is null)
        {
            // Không tồn tại hoặc chưa publish với learner → 404 để tránh leak
            throw new NotFoundException(
                $"Scenario version with ID {request.ScenarioVersionId} was not found or is not available.");
        }

        // Quy tắc nghiệp vụ: Learner chỉ được tạo session trên version đã publish.
        // (BA Expert / Admin tạo session test nội bộ sẽ là endpoint riêng.)
        if (!version.IsPublished)
        {
            throw new NotFoundException(
                $"Scenario version with ID {request.ScenarioVersionId} was not found or is not available.");
        }

        // 3. Lấy danh sách stakeholder personas active của version này
        //    (dùng để khởi tạo SessionAgents ngay khi mở session)
        var activePersonas = await scenarioCatalogService.GetActivePersonasAsync(
            version.Id, cancellationToken);

        if (activePersonas.Count == 0)
        {
            // Không thể mở session nếu version không có persona active nào
            throw new ConflictException(
                "This scenario version has no active stakeholder personas. Cannot start a session.");
        }

        // 4. Tính AttemptNo = lần thử tiếp theo của learner trên version này
        //    (cách tính Max+1 nằm trong Repository)
        var nextAttemptNo = await simulationRepository.GetNextAttemptNoAsync(
            learnerId, version.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;

        // 5. Tạo SimulationSession ở trạng thái Active
        var session = new SimulationSession
        {
            ScenarioVersionId = version.Id,
            AssignmentId = null,                 // MVP: chưa gắn assignment
            LearnerId = learnerId,
            AttemptNo = nextAttemptNo,
            Status = SessionStatus.Active,
            StartedAt = now
        };

        // 6. Khởi tạo SessionAgents từ các personas active
        //    Mood/trust/patience/cooperation khởi đầu dùng giá trị mặc định trung tính
        //    để AI Runtime có thể điều chỉnh theo diễn biến cuộc phỏng vấn.
        foreach (var persona in activePersonas)
        {
            session.SessionAgents.Add(new SessionAgent
            {
                PersonaId = persona.Id,
                TrustLevel = 50,         // neutral — người được phỏng vấn chưa tin tưởng learner
                PatienceLevel = 70,       // còn khá kiên nhẫn
                CooperationLevel = 60,    // hợp tác ở mức trung bình
                IsActive = true,
                LastInteractionAt = null
            });
        }

        // Đánh dấu cần INSERT (chưa gọi DB), rồi lưu xuống DB.
        simulationRepository.AddSession(session);
        await simulationRepository.SaveChangesAsync(cancellationToken);

        // 7. Map sang DTO trả về
        var personaNameById = activePersonas.ToDictionary(p => p.Id);
        var agentDtos = session.SessionAgents
            .OrderBy(a => a.PersonaId)
            .Select(a =>
            {
                personaNameById.TryGetValue(a.PersonaId, out var persona);

                return new SessionAgentDto(
                    a.Id,
                    a.PersonaId,
                    persona?.Name ?? string.Empty,
                    persona?.StakeholderRole ?? string.Empty,
                    a.TrustLevel ?? 0,
                    a.PatienceLevel ?? 0,
                    a.CooperationLevel ?? 0
                );
            })
            .ToList();

        return new SessionDto(
            session.Id,
            session.ScenarioVersionId,
            session.AssignmentId,
            session.AttemptNo,
            session.Status,
            session.StartedAt,
            session.SubmittedAt,
            session.CompletedAt,
            agentDtos
        );
    }

    public async Task<SessionDto?> GetSessionByIdAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default //code se ko bị bao loi neu thieu tham so
            )
    {
        // Lấy session kèm SessionAgents trong 1 query (tránh N+1).
        var session = await simulationRepository.GetSessionWithAgentsAsync(
            learnerId, sessionId, cancellationToken);

        if (session == null)
        {
            // Không tồn tại hoặc không thuộc learner → trả null để Controller map 404
            return null;

        }
        //Lấy tên + role của các personas đã gắn vào session
        //(cần cho SessionAgentDto.Name và StakeholderRole)
        var personaIds = session.SessionAgents
            .Select(a => a.PersonaId)
            .Distinct()
            .ToList();
        var personas = await scenarioCatalogService.GetPersonasByIdsAsync(
            personaIds, cancellationToken);
        //Map SessionAgent entity → SessionAgentDto
        var agentDto = session.SessionAgents
            .OrderBy(a => a.PersonaId)
            .Select(a =>
            {
                //Nếu persona đã bị xóa khỏi DB thì trả về rỗng
                personas.TryGetValue(a.PersonaId, out var persona);
                return new SessionAgentDto(
                    a.Id,
                    a.PersonaId,
                    persona?.Name ?? string.Empty,
                    persona?.StakeholderRole ?? string.Empty,
                    a.TrustLevel ?? 0,
                    a.PatienceLevel ?? 0,
                    a.CooperationLevel ?? 0
                    );
            })
            .ToList();
        return new SessionDto(
            session.Id,
            session.ScenarioVersionId,
            session.AssignmentId,
            session.AttemptNo,
            session.Status,
            session.StartedAt,
            session.SubmittedAt,
            session.CompletedAt,
            agentDto
            );
    }

    public async Task<IReadOnlyList<SessionAgentDto>?> GetSessionAgentsAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default
    )
    {
        // Kiểm tra session có tồn tại và thuộc về Learner này không.
        var sessionExistsAndOwned = await simulationRepository.SessionExistsAndOwnedByAsync(
            learnerId, sessionId, cancellationToken);
        if (!sessionExistsAndOwned)
        {
            //không tồn tại hoặc ko thuộc learner trả null
            //controller sẽ map thành 404 Not fuond
            return null;
        }

        // Lấy danh sách SessionAgents thuộc session này.
        var agents = await simulationRepository.GetAgentsBySessionIdAsync(
            sessionId, cancellationToken);

        // Trường hợp: session tồn tại nhưng chưa có agent nào
        //trả về danh sách rỗng (không phải null) trả về 200
        if (agents.Count == 0)
        {
            return Array.Empty<SessionAgentDto>();
        }

        //Lấy tin personas để map Name + stakeholderRole
        var personaIds = agents
            .Select(a => a.PersonaId)
            .Distinct()
            .ToList();

        var personas = await scenarioCatalogService.GetPersonasByIdsAsync(
            personaIds, cancellationToken);

        //map từ entity SessionAgent -> DTO sessionAgentDto
        var result = agents
            .OrderBy(agent => agent.PersonaId)
            .Select(a =>
            {
                //TryGetValue: an tòa, không exception nếu có persona bị xóa
                personas.TryGetValue(a.PersonaId, out var persona);

                return new SessionAgentDto(
                    a.Id,
                    a.PersonaId,
                    persona?.Name ?? string.Empty,
                    persona?.StakeholderRole ?? string.Empty,
                    a.TrustLevel ?? 0,
                    a.PatienceLevel ?? 0,
                    a.CooperationLevel ?? 0
                    );
            })
            .ToList();
        return result;
    }

    public async Task<bool> SessionExistsAndOwnedByAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // Gọi xuống Repository (module Simulation sở hữu bảng này).
        return await simulationRepository.SessionExistsAndOwnedByAsync(
            learnerId, sessionId, cancellationToken);
    }

    public async Task<bool> MessageBelongsToSessionAsync(
        int messageId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // Gọi xuống Repository (module Simulation sở hữu bảng Messages).
        return await simulationRepository.MessageBelongsToSessionAsync(
            messageId, sessionId, cancellationToken);
    }

    public async Task<SendAgentMessageResultDto> SendAgentMessageAsync(
        int learnerId,
        int sessionId,
        int agentId,
        SendAgentMessageRequest request,
        AgentAiConnection aiConnection,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate nội dung tin nhắn.
        var content = request.Content?.Trim() ?? string.Empty;
        if (content.Length == 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Content"] = ["Content is required and must not be empty."]
            });
        }

        // 2. Session phải tồn tại và thuộc learner này → nếu không trả 404 (không lộ sự tồn tại).
        var session = await simulationRepository.GetSessionWithAgentsAsync(
            learnerId, sessionId, cancellationToken);

        if (session is null)
        {
            throw new NotFoundException(
                $"Simulation session '{sessionId}' was not found.");
        }

        // 3. Chỉ session Active mới được phỏng vấn tiếp.
        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(
                $"This session is '{session.Status}' and no longer accepts messages.");
        }

        // 4. Agent phải thuộc chính session này.
        var agent = await simulationRepository.GetAgentByIdAsync(
            sessionId, agentId, cancellationToken);

        if (agent is null)
        {
            throw new NotFoundException(
                $"Stakeholder agent '{agentId}' was not found in this session.");
        }

        // 5. Lấy persona + allowed knowledge (module Scenario là chủ sở hữu dữ liệu này).
        var persona = await scenarioCatalogService.GetPersonaInterviewContextAsync(
            agent.PersonaId, cancellationToken);

        if (persona is null)
        {
            throw new NotFoundException(
                $"Stakeholder persona '{agent.PersonaId}' was not found.");
        }

        var allowedKnowledge = await scenarioCatalogService.GetAllowedKnowledgeAsync(
            agent.PersonaId, cancellationToken);

        var scenarioContext = await scenarioCatalogService.GetScenarioInterviewContextAsync(
            session.ScenarioVersionId, cancellationToken);

        if (scenarioContext is null)
        {
            throw new NotFoundException(
                $"Scenario version '{session.ScenarioVersionId}' was not found.");
        }

        // 6. Lấy 20 tin nhắn gần nhất của agent để dựng lịch sử cho AI.
        var historyMessages = await simulationRepository.GetMessagesByAgentAsync(
            sessionId, agentId, skip: 0, take: RecentHistorySize, cancellationToken: cancellationToken);

        var history = historyMessages
            .Select(m => new StakeholderChatMessageDto(
                m.SenderType == MessageSenderType.Learner ? "Learner" : "Stakeholder",
                m.Content,
                m.CreatedAt.ToString("O")))
            .ToList();

        // 7. Dựng payload và gọi AI Runtime.
        //    Lỗi runtime (key sai, rate limit, timeout) được ném ra dưới dạng
        //    AiRuntimeException để GlobalExceptionHandler map sang 422/429/503.
        var aiRequest = new StakeholderAgentRequest(
            Guid.NewGuid().ToString(),
            sessionId,
            scenarioContext,
            persona,
            allowedKnowledge,
            new StakeholderAgentStateDto(
                agent.MemorySummary,
                agent.TrustLevel ?? 50,
                agent.PatienceLevel ?? 50,
                agent.CooperationLevel ?? 50),
            history,
            content,
            Array.Empty<StakeholderEventContextDto>(),
            new StakeholderAiConnectionDto(
                aiConnection.Provider,
                aiConnection.Model,
                aiConnection.ApiKey));

        var aiResponse = await aiRuntimeClient.RespondAsync(aiRequest, cancellationToken);

        // 8. Lưu tin nhắn learner + tin nhắn stakeholder.
        var now = DateTimeOffset.UtcNow;

        var learnerMessage = new Message
        {
            SessionId = sessionId,
            SessionAgentId = agentId,
            SenderType = MessageSenderType.Learner,
            Content = content,
            CreatedAt = now
        };

        var stakeholderMessage = new Message
        {
            SessionId = sessionId,
            SessionAgentId = agentId,
            SenderType = MessageSenderType.Stakeholder,
            Content = aiResponse.Reply,
            CreatedAt = now
        };

        simulationRepository.AddMessage(learnerMessage);
        simulationRepository.AddMessage(stakeholderMessage);

        // 9. Cập nhật trạng thái agent theo stateUpdate từ AI runtime.
        agent.MemorySummary = aiResponse.StateUpdate.MemorySummary;
        agent.TrustLevel = (byte)ClampLevel(aiResponse.StateUpdate.TrustLevel);
        agent.PatienceLevel = (byte)ClampLevel(aiResponse.StateUpdate.PatienceLevel);
        agent.CooperationLevel = (byte)ClampLevel(aiResponse.StateUpdate.CooperationLevel);
        agent.LastInteractionAt = now;

        await simulationRepository.SaveChangesAsync(cancellationToken);

        // 10. Trả kết quả cho client.
        return new SendAgentMessageResultDto(
            ToMessageDto(learnerMessage),
            ToMessageDto(stakeholderMessage),
            aiResponse.UsedKnowledgeItemIds);
    }

    public async Task<AgentMessagePageDto?> GetAgentMessagesAsync(
        int learnerId,
        int sessionId,
        int agentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Session phải thuộc learner này.
        var sessionExistsAndOwned = await simulationRepository.SessionExistsAndOwnedByAsync(
            learnerId, sessionId, cancellationToken);

        if (!sessionExistsAndOwned)
        {
            return null;
        }

        // Agent phải thuộc chính session này.
        var agent = await simulationRepository.GetAgentByIdAsync(
            sessionId, agentId, cancellationToken);

        if (agent is null)
        {
            return null;
        }

        var total = await simulationRepository.CountMessagesByAgentAsync(
            sessionId, agentId, cancellationToken);

        var skip = (page - 1) * pageSize;
        var messages = await simulationRepository.GetMessagesByAgentAsync(
            sessionId, agentId, skip, pageSize, cancellationToken);

        return new AgentMessagePageDto(
            messages.Select(ToMessageDto).ToList(),
            page,
            pageSize,
            total);
    }

    public async Task<SessionEventPageDto?> GetSessionEventsAsync(
        int learnerId,
        int sessionId,
        int page,
        int pageSize,
        bool isInstructorOrAdmin,
        CancellationToken cancellationToken = default)
    {
        // Instructor/Admin được phép xem session của learner (authorize bằng policy riêng);
        // Learner chỉ xem được session của chính mình.
        if (!await CanAccessSessionAsync(learnerId, sessionId, isInstructorOrAdmin, cancellationToken))
        {
            return null;
        }

        var total = await simulationRepository.CountEventsBySessionIdAsync(
            sessionId, cancellationToken);

        var skip = (page - 1) * pageSize;
        var events = await simulationRepository.GetEventsBySessionIdAsync(
            sessionId, skip, pageSize, cancellationToken);

        return new SessionEventPageDto(
            events.Select(ToSessionEventDto).ToList(),
            page,
            pageSize,
            total);
    }

    public Task<TriggerSessionEventResultDto> TriggerSessionEventAsync(
        int sessionId,
        int scenarioEventId,
        CancellationToken cancellationToken = default)
    {
        _ = sessionId;
        _ = scenarioEventId;
        _ = cancellationToken;
        // Event chỉ chạy theo cấu hình sẵn (VD AfterSprintEnds) — không kích hoạt thủ công.
        throw new ConflictException(
            "Manual event triggering is disabled. Configure event timing in the scenario (e.g. after a sprint ends).");
    }

    public async Task<IReadOnlyList<ScenarioEventAvailabilityDto>?> GetSessionEventCatalogAsync(
        int learnerId,
        int sessionId,
        bool isInstructorOrAdmin,
        CancellationToken cancellationToken = default)
    {
        // Lấy session để biết ScenarioVersion đang pin.
        var session = isInstructorOrAdmin
            ? await simulationRepository.GetSessionByIdAsync(sessionId, cancellationToken)
            : await simulationRepository.GetSessionWithAgentsAsync(
                learnerId, sessionId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        var scenarioEvents = await scenarioCatalogService.GetActiveEventsAsync(
            session.ScenarioVersionId, cancellationToken);

        if (scenarioEvents.Count == 0)
        {
            return Array.Empty<ScenarioEventAvailabilityDto>();
        }

        // Một query batch lấy các event đã trigger, sau đó tra cứu trong bộ nhớ
        // để tránh N+1.
        var triggered = await simulationRepository.GetSessionEventsByScenarioEventIdsAsync(
            sessionId,
            scenarioEvents.Select(e => e.Id).ToList(),
            cancellationToken);

        var triggeredByScenarioEventId = triggered
            .GroupBy(e => e.ScenarioEventId)
            .ToDictionary(g => g.Key, g => g.First().TriggeredAt);

        // Sprint đã finalize → mở khóa event AfterSprintEnds.
        var finalizedSprintNos = await simulationRepository.GetFinalizedSprintNosAsync(
            sessionId, cancellationToken);
        var finalizedSet = finalizedSprintNos.ToHashSet();

        return scenarioEvents
            .Select(e =>
            {
                var afterSprintNo = e.TriggerType == ScenarioEventTriggerType.AfterSprintEnds
                    ? EventTriggerConditionCodec.TryGetAfterSprintNo(e.TriggerConditionJson)
                    : null;
                var isUnlocked = e.TriggerType switch
                {
                    ScenarioEventTriggerType.AfterSprintEnds =>
                        afterSprintNo is int n && finalizedSet.Contains(n),
                    ScenarioEventTriggerType.Manual => false,
                    _ => true
                };
                var unlockHint = e.TriggerType switch
                {
                    ScenarioEventTriggerType.AfterSprintEnds when afterSprintNo is int n =>
                        $"Sau khi kết thúc Sprint {n}",
                    ScenarioEventTriggerType.Manual =>
                        "Đã tắt kích hoạt thủ công — cấu hình thời điểm trong kịch bản",
                    ScenarioEventTriggerType.MessageCount => "Theo số tin nhắn",
                    ScenarioEventTriggerType.TimeElapsed => "Theo thời gian đã trôi",
                    _ => null
                };

                return new ScenarioEventAvailabilityDto(
                    e.Id,
                    e.Title,
                    e.Description,
                    e.TriggerType,
                    e.DisplayOrder,
                    triggeredByScenarioEventId.ContainsKey(e.Id),
                    triggeredByScenarioEventId.TryGetValue(e.Id, out var triggeredAt)
                        ? triggeredAt
                        : null,
                    afterSprintNo,
                    isUnlocked,
                    unlockHint);
            })
            .ToList();
    }

    /// <summary>
    /// Xác định người gọi có quyền truy cập session hay không.
    /// Instructor/Admin: chỉ cần session tồn tại.
    /// Các role khác (Learner): session phải thuộc chính họ.
    /// </summary>
    public Task<int> CountAttemptsByAssignmentAsync(
        int learnerId,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        return simulationRepository.CountAttemptsByAssignmentAsync(learnerId, assignmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<AssignmentSessionSnapshotDto>> GetLearnerSessionsByAssignmentIdsAsync(
        int learnerId,
        IReadOnlyCollection<int> assignmentIds,
        CancellationToken cancellationToken = default)
    {
        var sessions = await simulationRepository.GetLearnerSessionsByAssignmentIdsAsync(
            learnerId,
            assignmentIds,
            cancellationToken);

        return sessions
            .Where(s => s.AssignmentId.HasValue)
            .Select(s => new AssignmentSessionSnapshotDto(
                s.Id,
                s.AssignmentId!.Value,
                s.AttemptNo,
                s.Status,
                s.StartedAt,
                s.SubmittedAt,
                s.CompletedAt))
            .ToList();
    }

    public async Task<IReadOnlyList<LearnerSessionSummaryDto>> GetSessionsByAssignmentIdAsync(
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var sessions = await simulationRepository.GetSessionsByAssignmentIdAsync(assignmentId, cancellationToken);
        return sessions.Select(s => new LearnerSessionSummaryDto(
            s.Id,
            s.LearnerId,
            s.ScenarioVersionId,
            s.AssignmentId,
            s.AttemptNo,
            s.Status,
            s.StartedAt,
            s.SubmittedAt,
            s.CompletedAt)).ToList();
    }

    public async Task<IReadOnlyList<LearnerSessionSummaryDto>> GetSessionsByLearnerIdsAsync(
        IReadOnlyCollection<int> learnerIds,
        CancellationToken cancellationToken = default)
    {
        var sessions = await simulationRepository.GetSessionsByLearnerIdsAsync(learnerIds, cancellationToken);
        return sessions.Select(s => new LearnerSessionSummaryDto(
            s.Id,
            s.LearnerId,
            s.ScenarioVersionId,
            s.AssignmentId,
            s.AttemptNo,
            s.Status,
            s.StartedAt,
            s.SubmittedAt,
            s.CompletedAt)).ToList();
    }

    public async Task<IReadOnlyList<AgentMessageDto>> GetMessagesByIdsAsync(
        int sessionId,
        IReadOnlyCollection<int> messageIds,
        CancellationToken cancellationToken = default)
    {
        var messages = await simulationRepository.GetMessagesByIdsAsync(sessionId, messageIds, cancellationToken);
        return messages.Select(ToMessageDto).ToList();
    }

    public async Task<IReadOnlyList<SessionHistoryItemDto>> GetLearnerSessionsHistoryAsync(
        int learnerId,
        SessionStatus? status,
        int? scenarioVersionId,
        int? assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await simulationRepository.GetLearnerSessionsHistoryAsync(
            learnerId, status, scenarioVersionId, assignmentId, cancellationToken);
    }

    public async Task<SessionDto?> CompleteSessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await simulationRepository.GetSessionWithAgentsAsync(
            learnerId, sessionId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.Status == SessionStatus.Abandoned)
        {
            throw new ConflictException(
                "Cannot complete a session that has already been abandoned.");
        }

        if (session.Status != SessionStatus.Completed)
        {
            session.Status = SessionStatus.Completed;
            session.CompletedAt = DateTimeOffset.UtcNow;
            await simulationRepository.SaveChangesAsync(cancellationToken);
        }

        return await GetSessionByIdAsync(learnerId, sessionId, cancellationToken);
    }

    public async Task<SessionDto?> AbandonSessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await simulationRepository.GetSessionWithAgentsAsync(
            learnerId, sessionId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        if (session.Status is SessionStatus.Submitted or SessionStatus.Completed)
        {
            throw new ConflictException(
                $"Cannot abandon a session that is already '{session.Status}'.");
        }

        if (session.Status != SessionStatus.Abandoned)
        {
            session.Status = SessionStatus.Abandoned;
            session.CompletedAt = DateTimeOffset.UtcNow;
            await simulationRepository.SaveChangesAsync(cancellationToken);
        }

        return await GetSessionByIdAsync(learnerId, sessionId, cancellationToken);
    }

    private async Task<bool> CanAccessSessionAsync(
        int learnerId,
        int sessionId,
        bool isInstructorOrAdmin,
        CancellationToken cancellationToken)
    {
        return isInstructorOrAdmin
            ? await simulationRepository.SessionExistsAsync(sessionId, cancellationToken)
            : await simulationRepository.SessionExistsAndOwnedByAsync(
                learnerId, sessionId, cancellationToken);
    }

    /// <summary>
    /// Chuyển entity SessionEvent sang DTO trả về cho client.
    /// </summary>
    private static SessionEventDto ToSessionEventDto(SessionEvent sessionEvent)
    {
        return new SessionEventDto(
            sessionEvent.Id,
            sessionEvent.SessionId,
            sessionEvent.ScenarioEventId,
            sessionEvent.TriggerSource,
            sessionEvent.TriggeredAt,
            sessionEvent.SequenceNo,
            sessionEvent.EventContentSnapshot);
    }

    /// <summary>
    /// Chuyển entity Message sang DTO trả về cho client.
    /// </summary>
    private static AgentMessageDto ToMessageDto(Message message)    {
        return new AgentMessageDto(
            message.Id,
            message.SessionId,
            message.SessionAgentId,
            message.SenderType,
            message.Content,
            message.CreatedAt);
    }

    /// <summary>
    /// Giới hạn mức trust/patience/cooperation trong khoảng 0..255 (byte),
    /// tránh lỗi khi AI runtime trả về giá trị ngoài khoảng hợp lệ.
    /// </summary>
    private static int ClampLevel(int value)
    {
        return Math.Clamp(value, 0, 255);
    }
}
