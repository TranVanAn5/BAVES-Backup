from typing import List, Optional, Literal
from pydantic import BaseModel, Field

class KnowledgeItemDto(BaseModel):
    knowledgeItemId: int
    code: str
    title: str
    content: str
    category: str
    importance: str
    accessType: Literal["Knows", "Partial", "DoesNotKnow"] = "Knows"
    isHidden: bool = False
    disclosureRule: Optional[str] = None

class ScenarioContextDto(BaseModel):
    scenarioVersionId: int
    companyName: Optional[str] = "OmniMart"
    businessContext: str
    businessProblem: str
    assignment: str

class StakeholderPersonaDto(BaseModel):
    personaId: int
    name: str
    stakeholderRole: str
    department: Optional[str] = None
    responsibilities: Optional[str] = None
    goals: Optional[str] = None
    painPoints: Optional[str] = None
    businessConcerns: Optional[str] = None
    communicationStyle: Optional[str] = None
    personality: Optional[str] = None

class AgentStateDto(BaseModel):
    memorySummary: Optional[str] = ""
    trustLevel: int = 50
    patienceLevel: int = 50
    cooperationLevel: int = 50

class ChatMessageDto(BaseModel):
    senderType: Literal["Learner", "Stakeholder"]
    content: str
    createdAt: Optional[str] = None

class AiConnectionDto(BaseModel):
    provider: Literal["OpenAI", "Gemini", "Anthropic", "Mock"] = "Mock"
    model: Optional[str] = None
    apiKey: Optional[str] = None
    demoFallback: bool = False

class EventContextDto(BaseModel):
    title: str
    description: str

class StakeholderAgentRequest(BaseModel):
    requestId: str
    sessionId: int
    scenario: ScenarioContextDto
    persona: StakeholderPersonaDto
    allowedKnowledge: List[KnowledgeItemDto] = []
    agentState: AgentStateDto = Field(default_factory=AgentStateDto)
    history: List[ChatMessageDto] = []
    learnerMessage: str
    eventContext: List[EventContextDto] = []
    aiConnection: AiConnectionDto = Field(default_factory=AiConnectionDto)

class AgentStateUpdateDto(BaseModel):
    memorySummary: str
    trustLevel: int
    patienceLevel: int
    cooperationLevel: int

class UsageStatsDto(BaseModel):
    promptTokens: int = 0
    completionTokens: int = 0
    latencyMs: int = 0

class StakeholderReply(BaseModel):
    reply: str
    usedKnowledgeItemIds: List[int] = []
    stateUpdate: AgentStateUpdateDto

class StakeholderAgentResponse(BaseModel):
    requestId: str
    reply: str
    usedKnowledgeItemIds: List[int] = []
    stateUpdate: AgentStateUpdateDto
    promptVersion: str = "stakeholder@1.0.0"
    usage: UsageStatsDto = Field(default_factory=UsageStatsDto)
    errorCode: Optional[str] = None
    isFallback: bool = False
    error: Optional[str] = None

class SprintArtifactSectionDto(BaseModel):
    sectionKey: str
    title: str
    content: str

class SprintArtifactDto(BaseModel):
    artifactId: Optional[int] = None
    artifactType: Literal[
        "BAP",
        "StakeholderRegister",
        "BRD",
        "BusinessRulesCatalog",
        "SRS",
        "UserStories",
        "UseCaseSpecification",
        "ImpactAnalysis",
        "RTM",
        "Generic"
    ] = "Generic"
    templateCode: Optional[str] = None
    title: str
    versionNo: int = 1
    sections: List[SprintArtifactSectionDto] = []

class SprintReviewMessageDto(BaseModel):
    speaker: str
    content: str

class SprintReviewSummaryDto(BaseModel):
    sprintNo: int
    decision: Literal["Approved", "ApprovedWithRevisions", "NeedsRevision"] = "ApprovedWithRevisions"
    sprintScore: float = 80.0
    approvedPoints: List[str] = []
    actionItemsForNextSprint: List[str] = []
    compressedDigest: str
    nextSprintUnlocked: bool = True

class PriorSprintDigestDto(BaseModel):
    sprintNo: int
    score: Optional[float] = None
    decision: Optional[str] = None
    digest: Optional[str] = None
    actionItems: List[str] = []


class SprintReviewRequest(BaseModel):
    requestId: str = "sprint-rev-01"
    sessionId: int
    sprintNo: int = 1
    totalSprints: int = 4
    reviewPhase: Literal["Discuss", "FinalizeMid", "FinalizeFinal"] = "Discuss"
    revealNumericScoreToLearner: bool = False
    chairPersonas: List[StakeholderPersonaDto] = []
    artifacts: List[SprintArtifactDto] = []
    requiredDeliverableCodes: List[str] = []
    missingDeliverableCodes: List[str] = []
    newDeliverableCodes: List[str] = []
    priorArtifactsForComparison: List[SprintArtifactDto] = []
    interviewExcerpts: List[SprintReviewMessageDto] = []
    priorSprintDigests: List[PriorSprintDigestDto] = []
    discussionHistory: List[SprintReviewMessageDto] = []
    learnerReply: Optional[str] = None
    eventContext: List[EventContextDto] = []
    finalizeSprint: bool = True
    aiConnection: AiConnectionDto = Field(default_factory=AiConnectionDto)

class SprintReviewResponse(BaseModel):
    requestId: str
    sessionId: int
    sprintNo: int
    chairSpeakerName: str
    chairReply: str
    challengeQuestions: List[str] = []
    sprintSummary: Optional[SprintReviewSummaryDto] = None
    promptVersion: str = "sprint_review@1.0.0"
    usage: UsageStatsDto = Field(default_factory=UsageStatsDto)
    errorCode: Optional[str] = None
    isFallback: bool = False
    error: Optional[str] = None
