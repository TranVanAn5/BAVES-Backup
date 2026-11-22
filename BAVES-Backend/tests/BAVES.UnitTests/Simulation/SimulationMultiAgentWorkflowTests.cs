using BAVES.Api.Modules.Analysis.Models.DTOs;
using BAVES.Api.Modules.Analysis.Models.Entities;
using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Analysis.Models.Requests;
using BAVES.Api.Modules.Analysis.Repositories.Interfaces;
using BAVES.Api.Modules.Analysis.Services;
using BAVES.Api.Modules.Planning.Models.DTOs;
using BAVES.Api.Modules.Planning.Models.Enums;
using BAVES.Api.Modules.Planning.Repositories.Interfaces;
using BAVES.Api.Modules.Planning.Services;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Enums;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Entities;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Models.Requests;
using BAVES.Api.Modules.Simulation.Repositories.Interfaces;
using BAVES.Api.Modules.Simulation.Services;
using BAVES.Api.Shared.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace BAVES.UnitTests.Simulation;

/// <summary>
/// WF-03: Thực hiện Mô phỏng Doanh nghiệp & Phỏng vấn Thu thập Yêu cầu (Simulation & Multi-Agent Elicitation)
/// Owner: Nguyễn Tấn Thanh Thảo
/// </summary>
public class SimulationMultiAgentWorkflowTests
{
    private readonly Mock<ISimulationSessionRepository> _simRepoMock = new();
    private readonly Mock<IScenarioCatalogService> _scenarioCatalogMock = new();
    private readonly Mock<IAiRuntimeClient> _aiRuntimeClientMock = new();
    private readonly Mock<IAnalysisItemRepository> _analysisRepoMock = new();
    private readonly Mock<IPlanningRepository> _planningRepoMock = new();

    private readonly SimulationSessionService _sutSimulation;
    private readonly AnalysisItemService _sutAnalysis;
    private readonly PlanningService _sutPlanning;

    public SimulationMultiAgentWorkflowTests()
    {
        _sutSimulation = new SimulationSessionService(
            _simRepoMock.Object,
            _scenarioCatalogMock.Object,
            _aiRuntimeClientMock.Object
        );

        _sutAnalysis = new AnalysisItemService(
            _analysisRepoMock.Object,
            _sutSimulation,
            _scenarioCatalogMock.Object,
            _aiRuntimeClientMock.Object
        );

        _sutPlanning = new PlanningService(
            _planningRepoMock.Object,
            _sutSimulation
        );
    }

    #region F-SIM-01: Khởi tạo Phiên & Lập kế hoạch (Session & BA Plan)

    [Fact]
    public async Task TC_SIM_01_StartSessionAsync_InitializesSessionWithActiveStatusAndAgents()
    {
        // Arrange
        const int learnerId = 42;
        const int scenarioVersionId = 10;

        _scenarioCatalogMock.Setup(s => s.GetVersionInfoAsync(scenarioVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(scenarioVersionId, 1, 1, ScenarioDifficulty.Intermediate, IsPublished: true));

        var personas = new List<ScenarioPersonaInfoDto>
        {
            new(101, "Ops Manager", "Operations Manager", 1),
            new(102, "IT Lead", "Lead Architect", 2),
            new(103, "CS Lead", "Customer Service Manager", 3)
        };
        _scenarioCatalogMock.Setup(s => s.GetActivePersonasAsync(scenarioVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(personas);

        _simRepoMock.Setup(r => r.GetNextAttemptNoAsync(learnerId, scenarioVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sutSimulation.StartSessionAsync(learnerId, new StartSessionRequest { ScenarioVersionId = scenarioVersionId });

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(SessionStatus.Active);
        result.AttemptNo.Should().Be(1);
        result.Agents.Should().HaveCount(3);
        result.Agents.Select(a => a.PersonaId).Should().Contain([101, 102, 103]);

        _simRepoMock.Verify(r => r.AddSession(It.Is<SimulationSession>(s =>
            s.LearnerId == learnerId &&
            s.ScenarioVersionId == scenarioVersionId &&
            s.Status == SessionStatus.Active &&
            s.SessionAgents.Count == 3
        )), Times.Once);
    }

    [Fact]
    public async Task TC_SIM_02_StartSessionAsync_ValidatesScenarioVersionPublishedAndActivePersonas()
    {
        // Arrange: Unapproved / unpublished draft version
        _scenarioCatalogMock.Setup(s => s.GetVersionInfoAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(99, 1, 1, ScenarioDifficulty.Beginner, IsPublished: false));

        var actUnpublished = () => _sutSimulation.StartSessionAsync(42, new StartSessionRequest { ScenarioVersionId = 99 });
        await actUnpublished.Should().ThrowAsync<NotFoundException>();

        // Arrange: Published version but has 0 active personas
        _scenarioCatalogMock.Setup(s => s.GetVersionInfoAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(100, 1, 1, ScenarioDifficulty.Beginner, IsPublished: true));
        _scenarioCatalogMock.Setup(s => s.GetActivePersonasAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScenarioPersonaInfoDto>());

        var actNoPersonas = () => _sutSimulation.StartSessionAsync(42, new StartSessionRequest { ScenarioVersionId = 100 });
        await actNoPersonas.Should().ThrowAsync<ConflictException>()
            .WithMessage("*no active stakeholder personas*");
    }

    [Fact]
    public async Task TC_SIM_03_Planning_CreateOrUpdateBAPlan_LinksObjectiveAndApproachToSession()
    {
        // Arrange
        const int learnerId = 42;
        const int sessionId = 1;

        _simRepoMock.Setup(r => r.SessionExistsAndOwnedByAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new UpsertPlanRequest(
            Objective: "Clarify order hold timeout and inventory reservation rules",
            Approach: "Interview Ops Manager first, then verify technical integration with IT Lead",
            Questions: [new PlannedQuestionInput(101, "Topic", "Need", "What is the threshold for holding stock?", PriorityLevel.High, 1)]
        );

        var expectedPlanDto = new BAPlanDto(
            Id: 5,
            SessionId: sessionId,
            Objective: request.Objective,
            Approach: request.Approach,
            Status: BAPlanStatus.Draft,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: null,
            Questions: []
        );

        _planningRepoMock.Setup(r => r.GetPlanEntityForUpdateAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BAVES.Api.Modules.Planning.Models.Entities.BAPlan?)null);
        _planningRepoMock.Setup(r => r.GetPlanBySessionAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPlanDto);

        // Act
        var result = await _sutPlanning.UpsertPlanAsync(learnerId, sessionId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Objective.Should().Be(request.Objective);
        result.Approach.Should().Be(request.Approach);
    }

    #endregion

    #region F-SIM-02: Phỏng vấn Tương tác Đa tác tử & Bảo vệ Bí mật ẩn

    [Fact]
    public async Task TC_SIM_04_SendAgentMessageAsync_LearnerSendsMessage_AgentRepliesAndUpdateTrustAndPatience()
    {
        // Arrange
        const int learnerId = 42;
        const int sessionId = 1;
        const int agentId = 10;
        const int personaId = 101;

        var agent = new SessionAgent
        {
            Id = agentId,
            SessionId = sessionId,
            PersonaId = personaId,
            TrustLevel = 50,
            PatienceLevel = 70,
            CooperationLevel = 60,
            IsActive = true
        };

        var session = new SimulationSession
        {
            Id = sessionId,
            LearnerId = learnerId,
            ScenarioVersionId = 1,
            Status = SessionStatus.Active,
            SessionAgents = new List<SessionAgent> { agent }
        };

        _simRepoMock.Setup(r => r.GetSessionWithAgentsAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _simRepoMock.Setup(r => r.GetAgentByIdAsync(sessionId, agentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agent);

        _scenarioCatalogMock.Setup(s => s.GetPersonaInterviewContextAsync(personaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StakeholderPersonaDto(personaId, "Sarah", "Operations Manager", "Ops", "Desc", "Goals", "Pains", "Concerns", "Direct", "Pragmatic"));
        _scenarioCatalogMock.Setup(s => s.GetAllowedKnowledgeAsync(personaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StakeholderKnowledgeItemDto>
            {
                new(1, "K-HOLD", "Stock Hold", "Stock is held for 15 minutes", "Inventory", "High", "Owns", IsHidden: false, null)
            });
        _scenarioCatalogMock.Setup(s => s.GetScenarioInterviewContextAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StakeholderScenarioContextDto(1, "OmniMart", "Context", "Problem", "BA"));

        _simRepoMock.Setup(r => r.GetMessagesByAgentAsync(sessionId, agentId, 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Message>());

        _aiRuntimeClientMock.Setup(a => a.RespondAsync(It.IsAny<StakeholderAgentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StakeholderAgentResponse(
                RequestId: "req-1",
                Reply: "Yes, stock is currently reserved for 15 minutes at checkout.",
                UsedKnowledgeItemIds: [1],
                StateUpdate: new StakeholderAgentStateDto(MemorySummary: null, TrustLevel: 55, PatienceLevel: 68, CooperationLevel: 60),
                PromptVersion: "v1",
                Usage: new StakeholderUsageDto(100, 50, 120),
                Error: null
            ));

        var request = new SendAgentMessageRequest { Content = "Could you tell me how long stock is reserved?" };
        var aiConnection = new AgentAiConnection("OpenAI", "gpt-4o-mini", "key");

        // Act
        var result = await _sutSimulation.SendAgentMessageAsync(learnerId, sessionId, agentId, request, aiConnection);

        // Assert
        result.Should().NotBeNull();
        result.StakeholderMessage.Content.Should().Contain("15 minutes");
        agent.TrustLevel.Should().Be(55);
        agent.PatienceLevel.Should().Be(68);

        _simRepoMock.Verify(r => r.AddMessage(It.Is<Message>(m => m.SenderType == MessageSenderType.Learner)), Times.Once);
        _simRepoMock.Verify(r => r.AddMessage(It.Is<Message>(m => m.SenderType == MessageSenderType.Stakeholder)), Times.Once);
        _simRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_SIM_05_KnowledgeLeakage_HiddenKnowledgeItem_NotDisclosedWhenConditionUnmet()
    {
        // Arrange
        const int personaId = 101;

        var knowledgeItems = new List<StakeholderKnowledgeItemDto>
        {
            new(1, "K-PUB", "Standard Policy", "Public policy info", "General", "Medium", "Owns", IsHidden: false, null),
            new(2, "K-SEC", "Executive Discount Threshold", "Secret 40% margin override", "Pricing", "Critical", "Owns", IsHidden: true, "Only reveal if trust >= 80")
        };

        _scenarioCatalogMock.Setup(s => s.GetAllowedKnowledgeAsync(personaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(knowledgeItems);

        // Act
        var allowed = await _scenarioCatalogMock.Object.GetAllowedKnowledgeAsync(personaId);

        // Assert
        var hiddenItem = allowed.FirstOrDefault(k => k.IsHidden);
        hiddenItem.Should().NotBeNull();
        hiddenItem!.DisclosureRule.Should().Contain("trust >= 80");
        hiddenItem.IsHidden.Should().BeTrue();
    }

    [Fact]
    public async Task TC_SIM_06_PersonaBoundary_AgentA_OnlyReceivesKnowledgeAssignedToPersonaA()
    {
        // Arrange: Agent A (Ops) has K-01; Agent B (IT) has K-02
        const int opsPersonaId = 101;
        const int itPersonaId = 102;

        var opsKnowledge = new List<StakeholderKnowledgeItemDto>
        {
            new(1, "K-OPS", "Warehouse Shift Schedule", "3 shifts daily", "Operations", "Medium", "Owns", false, null)
        };
        var itKnowledge = new List<StakeholderKnowledgeItemDto>
        {
            new(2, "K-IT", "POS Kafka Cluster Configuration", "Brokers 3, replication factor 2", "Architecture", "High", "Owns", false, null)
        };

        _scenarioCatalogMock.Setup(s => s.GetAllowedKnowledgeAsync(opsPersonaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(opsKnowledge);
        _scenarioCatalogMock.Setup(s => s.GetAllowedKnowledgeAsync(itPersonaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(itKnowledge);

        // Act
        var opsResult = await _scenarioCatalogMock.Object.GetAllowedKnowledgeAsync(opsPersonaId);
        var itResult = await _scenarioCatalogMock.Object.GetAllowedKnowledgeAsync(itPersonaId);

        // Assert
        opsResult.Should().ContainSingle(k => k.Code == "K-OPS");
        opsResult.Should().NotContain(k => k.Code == "K-IT");

        itResult.Should().ContainSingle(k => k.Code == "K-IT");
        itResult.Should().NotContain(k => k.Code == "K-OPS");
    }

    #endregion

    #region F-SIM-03: Tác động Sự kiện Đột xuất & Trích dẫn Dẫn chứng

    [Fact]
    public async Task TC_SIM_07_TriggerSessionEventAsync_ActiveSession_RecordsSessionEventSnapshot()
    {
        // Arrange
        const int sessionId = 1;
        const int scenarioEventId = 20;

        var session = new SimulationSession
        {
            Id = sessionId,
            ScenarioVersionId = 5,
            Status = SessionStatus.Active
        };

        var scenarioEvent = new ScenarioEventInfoDto(
            Id: scenarioEventId,
            ScenarioVersionId: 5,
            Title: "POS Connection Failure",
            Description: "Simulate network partition between stores and head office",
            TriggerType: ScenarioEventTriggerType.Manual,
            EventContent: "{\"affectedChannels\": [\"POS\", \"InStore\"]}",
            DisplayOrder: 1,
            IsActive: true
        );

        _simRepoMock.Setup(r => r.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _scenarioCatalogMock.Setup(s => s.GetEventByIdAsync(scenarioEventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenarioEvent);

        _simRepoMock.Setup(r => r.ExecuteInSerializableTransactionAsync(
            It.IsAny<Func<Task<TriggerSessionEventResultDto>>>(),
            It.IsAny<CancellationToken>()))
            .Returns<Func<Task<TriggerSessionEventResultDto>>, CancellationToken>((action, ct) => action());

        _simRepoMock.Setup(r => r.GetSessionEventByScenarioEventIdAsync(sessionId, scenarioEventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SessionEvent?)null);
        _simRepoMock.Setup(r => r.GetNextEventSequenceNoAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sutSimulation.TriggerSessionEventAsync(sessionId, scenarioEventId);

        // Assert
        result.Should().NotBeNull();
        result.AlreadyTriggered.Should().BeFalse();
        result.Event.ScenarioEventId.Should().Be(scenarioEventId);
        result.Event.EventContentSnapshot.Should().Be(scenarioEvent.EventContent);

        _simRepoMock.Verify(r => r.AddSessionEvent(It.Is<SessionEvent>(e =>
            e.SessionId == sessionId &&
            e.ScenarioEventId == scenarioEventId &&
            e.TriggerSource == EventTriggerSource.Instructor &&
            e.EventContentSnapshot == scenarioEvent.EventContent
        )), Times.Once);
    }

    [Fact]
    public async Task TC_SIM_08_TriggerSessionEventAsync_DuplicateTrigger_IsIdempotentAndReturnsAlreadyTriggered()
    {
        // Arrange
        const int sessionId = 1;
        const int scenarioEventId = 20;

        var session = new SimulationSession { Id = sessionId, ScenarioVersionId = 5, Status = SessionStatus.Active };
        var scenarioEvent = new ScenarioEventInfoDto(scenarioEventId, 5, "POS Outage", "Desc", ScenarioEventTriggerType.Manual, "{}", 1, true);

        var existingSessionEvent = new SessionEvent
        {
            Id = 10,
            SessionId = sessionId,
            ScenarioEventId = scenarioEventId,
            TriggerSource = EventTriggerSource.Instructor,
            TriggeredAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            SequenceNo = 1,
            EventContentSnapshot = "{}"
        };

        _simRepoMock.Setup(r => r.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(session);
        _scenarioCatalogMock.Setup(s => s.GetEventByIdAsync(scenarioEventId, It.IsAny<CancellationToken>())).ReturnsAsync(scenarioEvent);

        _simRepoMock.Setup(r => r.ExecuteInSerializableTransactionAsync(
            It.IsAny<Func<Task<TriggerSessionEventResultDto>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<TriggerSessionEventResultDto>>, CancellationToken>((action, ct) => action());

        _simRepoMock.Setup(r => r.GetSessionEventByScenarioEventIdAsync(sessionId, scenarioEventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSessionEvent);

        // Act
        var result = await _sutSimulation.TriggerSessionEventAsync(sessionId, scenarioEventId);

        // Assert
        result.Should().NotBeNull();
        result.AlreadyTriggered.Should().BeTrue();
        _simRepoMock.Verify(r => r.AddSessionEvent(It.IsAny<SessionEvent>()), Times.Never);
    }

    [Fact]
    public async Task TC_SIM_09_EvidenceTraceability_AddEvidence_RejectsMessageBelongingToDifferentSession()
    {
        // Arrange
        const int learnerId = 42;
        const int targetSessionId = 1;
        const int alienMessageId = 999;
        const int itemId = 10;

        var analysisItem = new AnalysisItem
        {
            Id = itemId,
            SessionId = targetSessionId,
            ItemType = AnalysisItemType.Requirement,
            Status = AnalysisItemStatus.Draft
        };

        _analysisRepoMock.Setup(r => r.GetItemEntityByIdAsync(itemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(analysisItem);

        _simRepoMock.Setup(r => r.GetSessionWithAgentsAsync(learnerId, targetSessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SimulationSession { Id = targetSessionId, LearnerId = learnerId, Status = SessionStatus.Active });

        // Message 999 does NOT belong to session 1
        _simRepoMock.Setup(r => r.MessageBelongsToSessionAsync(alienMessageId, targetSessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var request = new CreateEvidenceLinkRequest
        {
            MessageId = alienMessageId,
            RelationshipType = EvidenceRelationshipType.Supports
        };

        // Act
        var act = () => _sutAnalysis.AddEvidenceAsync(learnerId, itemId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*was not found in this session*");
    }

    #endregion

    #region End-to-End Workflow Tests

    [Fact]
    public async Task TC_SIM_E2E_01_FullSimulationFlow_StartSession_InterviewStakeholder_TriggerEvent_CreateRequirementWithEvidence()
    {
        // Arrange
        const int learnerId = 42;
        const int scenarioVersionId = 1;
        const int sessionId = 100;
        const int agentId = 5;

        // Step 1: Start Session
        _scenarioCatalogMock.Setup(s => s.GetVersionInfoAsync(scenarioVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioVersionInfoDto(scenarioVersionId, 1, 1, ScenarioDifficulty.Intermediate, true));
        _scenarioCatalogMock.Setup(s => s.GetActivePersonasAsync(scenarioVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ScenarioPersonaInfoDto> { new(101, "Ops Mgr", "Role", 1) });
        _simRepoMock.Setup(r => r.GetNextAttemptNoAsync(learnerId, scenarioVersionId, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var startedSession = await _sutSimulation.StartSessionAsync(learnerId, new StartSessionRequest { ScenarioVersionId = scenarioVersionId });
        startedSession.Status.Should().Be(SessionStatus.Active);

        // Step 2: Message Agent
        var agent = new SessionAgent { Id = agentId, SessionId = sessionId, PersonaId = 101, TrustLevel = 50, PatienceLevel = 70, IsActive = true };
        var sessionEntity = new SimulationSession { Id = sessionId, LearnerId = learnerId, ScenarioVersionId = scenarioVersionId, Status = SessionStatus.Active, SessionAgents = [agent] };
        _simRepoMock.Setup(r => r.GetSessionWithAgentsAsync(learnerId, sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(sessionEntity);
        _simRepoMock.Setup(r => r.GetAgentByIdAsync(sessionId, agentId, It.IsAny<CancellationToken>())).ReturnsAsync(agent);
        _simRepoMock.Setup(r => r.GetMessagesByAgentAsync(sessionId, agentId, 0, It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Message>());
        _scenarioCatalogMock.Setup(s => s.GetPersonaInterviewContextAsync(101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StakeholderPersonaDto(101, "Ops Mgr", "Role", "D", "R", "G", "P", "C", "S", "P"));
        _scenarioCatalogMock.Setup(s => s.GetAllowedKnowledgeAsync(101, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StakeholderKnowledgeItemDto> { new(1, "K-1", "T", "Content", "C", "I", "Knows", false, null) });
        _scenarioCatalogMock.Setup(s => s.GetScenarioInterviewContextAsync(scenarioVersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StakeholderScenarioContextDto(scenarioVersionId, "OmniMart", "Ctx", "Prob", "BA"));
        _aiRuntimeClientMock.Setup(a => a.RespondAsync(It.IsAny<StakeholderAgentRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StakeholderAgentResponse(
                RequestId: "req-2",
                Reply: "POS disconnects automatically after 3 retries.",
                UsedKnowledgeItemIds: [1],
                StateUpdate: new StakeholderAgentStateDto(MemorySummary: null, TrustLevel: 53, PatienceLevel: 70, CooperationLevel: 60),
                PromptVersion: "v1",
                Usage: new StakeholderUsageDto(100, 50, 120),
                Error: null
            ));

        var msgResult = await _sutSimulation.SendAgentMessageAsync(learnerId, sessionId, agentId, new SendAgentMessageRequest { Content = "How does retry work?" }, new AgentAiConnection("OpenAI", null, null));
        msgResult.StakeholderMessage.Content.Should().Contain("POS disconnects");

        // Step 3: Trigger Event
        _simRepoMock.Setup(r => r.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(sessionEntity);
        _scenarioCatalogMock.Setup(s => s.GetEventByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ScenarioEventInfoDto(10, scenarioVersionId, "Network Failure", "Desc", ScenarioEventTriggerType.Manual, "{}", 1, true));
        _simRepoMock.Setup(r => r.ExecuteInSerializableTransactionAsync(It.IsAny<Func<Task<TriggerSessionEventResultDto>>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task<TriggerSessionEventResultDto>>, CancellationToken>((a, ct) => a());
        var eventResult = await _sutSimulation.TriggerSessionEventAsync(sessionId, 10);
        eventResult.AlreadyTriggered.Should().BeFalse();

        // Step 4: Add Evidence Link to Requirement
        _scenarioCatalogMock.Setup(s => s.GetPersonasByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<int, ScenarioPersonaInfoDto> { [101] = new(101, "Ops Mgr", "Role", 1) });

        var reqItem = new AnalysisItem { Id = 1, SessionId = sessionId, ItemType = AnalysisItemType.Requirement, Status = AnalysisItemStatus.Draft };
        _analysisRepoMock.Setup(r => r.GetItemEntityByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(reqItem);
        _simRepoMock.Setup(r => r.MessageBelongsToSessionAsync(50, sessionId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _analysisRepoMock.Setup(r => r.EvidenceLinkExistsAsync(1, 50, EvidenceRelationshipType.Supports, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var evidenceResult = await _sutAnalysis.AddEvidenceAsync(learnerId, 1, new CreateEvidenceLinkRequest { MessageId = 50, RelationshipType = EvidenceRelationshipType.Supports });
        evidenceResult.Should().NotBeNull();
        evidenceResult!.RelationshipType.Should().Be(EvidenceRelationshipType.Supports);
    }

    [Fact]
    public async Task TC_SIM_E2E_02_SessionResume_LearnerExitsAndReEnters_HistoryAndAgentStatesPreserved()
    {
        // Arrange
        const int learnerId = 42;
        const int sessionId = 1;

        var agent = new SessionAgent
        {
            Id = 1,
            PersonaId = 101,
            TrustLevel = 85, // Accumulated trust
            PatienceLevel = 40,
            CooperationLevel = 90
        };

        var session = new SimulationSession
        {
            Id = sessionId,
            LearnerId = learnerId,
            ScenarioVersionId = 1,
            Status = SessionStatus.Active,
            StartedAt = DateTimeOffset.UtcNow.AddHours(-1),
            SessionAgents = new List<SessionAgent> { agent }
        };

        _simRepoMock.Setup(r => r.GetSessionWithAgentsAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        _scenarioCatalogMock.Setup(s => s.GetPersonasByIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<int, ScenarioPersonaInfoDto>
            {
                [101] = new(101, "Sarah Ops", "Operations Manager", 1)
            });

        // Act: Learner retrieves/resumes existing session
        var resumed = await _sutSimulation.GetSessionByIdAsync(learnerId, sessionId);

        // Assert
        resumed.Should().NotBeNull();
        resumed!.Status.Should().Be(SessionStatus.Active);
        resumed.Agents.Should().HaveCount(1);
        resumed.Agents.First().TrustLevel.Should().Be(85);
        resumed.Agents.First().PatienceLevel.Should().Be(40);
        resumed.Agents.First().CooperationLevel.Should().Be(90);
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public async Task TC_SIM_ST_01_SessionStateTransition_ActiveToCompleted()
    {
        // Arrange
        const int learnerId = 42;
        const int sessionId = 1;

        var session = new SimulationSession
        {
            Id = sessionId,
            LearnerId = learnerId,
            Status = SessionStatus.Active
        };

        _simRepoMock.Setup(r => r.GetSessionWithAgentsAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sutSimulation.CompleteSessionAsync(learnerId, sessionId);

        // Assert
        result.Should().NotBeNull();
        session.Status.Should().Be(SessionStatus.Completed);
        session.CompletedAt.Should().NotBeNull();
        _simRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_SIM_ST_02_SessionStateTransition_ActiveToAbandoned()
    {
        // Arrange
        const int learnerId = 42;
        const int sessionId = 1;

        var session = new SimulationSession
        {
            Id = sessionId,
            LearnerId = learnerId,
            Status = SessionStatus.Active
        };

        _simRepoMock.Setup(r => r.GetSessionWithAgentsAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sutSimulation.AbandonSessionAsync(learnerId, sessionId);

        // Assert
        result.Should().NotBeNull();
        session.Status.Should().Be(SessionStatus.Abandoned);
        session.CompletedAt.Should().NotBeNull();
        _simRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TC_SIM_ST_03_ClosedSessionGuard_CompletedOrAbandonedSession_RejectsNewMessagesAndEvents()
    {
        // Arrange: Session is Completed
        const int learnerId = 42;
        const int sessionId = 1;

        var completedSession = new SimulationSession
        {
            Id = sessionId,
            LearnerId = learnerId,
            Status = SessionStatus.Completed
        };

        _simRepoMock.Setup(r => r.GetSessionWithAgentsAsync(learnerId, sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedSession);
        _simRepoMock.Setup(r => r.GetSessionByIdAsync(sessionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedSession);

        // Act 1: Send message on completed session
        var actMsg = () => _sutSimulation.SendAgentMessageAsync(
            learnerId, sessionId, 1, new SendAgentMessageRequest { Content = "Hello" }, new AgentAiConnection("Mock", null, null));

        await actMsg.Should().ThrowAsync<ConflictException>()
            .WithMessage("*no longer accepts messages*");

        // Act 2: Trigger event on completed session
        var actEvent = () => _sutSimulation.TriggerSessionEventAsync(sessionId, 10);

        await actEvent.Should().ThrowAsync<ConflictException>()
            .WithMessage("*no longer accepts new events*");
    }

    #endregion
}
