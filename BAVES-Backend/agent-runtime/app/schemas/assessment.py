from typing import List, Optional, Literal
from pydantic import BaseModel, Field
from app.schemas.stakeholder import AiConnectionDto, UsageStatsDto

class RubricCriterionDto(BaseModel):
    criterionId: Optional[int] = None
    code: str
    name: str
    description: Optional[str] = None
    weight: float = 1.0
    maxScore: float = 100.0

class RubricDto(BaseModel):
    rubricId: Optional[int] = None
    title: str
    criteria: List[RubricCriterionDto] = []

class ArtifactSectionDto(BaseModel):
    sectionKey: str
    title: str
    content: str

class ArtifactSubmissionDto(BaseModel):
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
    ] = "BRD"
    title: str
    versionNo: int = 1
    sections: List[ArtifactSectionDto] = []

class AssessmentEvidenceDto(BaseModel):
    evidenceId: Optional[int] = None
    messageContent: str
    stakeholderName: Optional[str] = None
    linkedItemTitle: Optional[str] = None

class SprintAssessmentDigestDto(BaseModel):
    sprintNo: int
    decision: Literal["Approved", "ApprovedWithRevisions", "NeedsRevision"] = "ApprovedWithRevisions"
    sprintScore: float = 80.0
    compressedDigest: str

class ProcessMetricsDto(BaseModel):
    revealedKnowledgeCount: int = 0
    totalKnowledgeCount: int = 10
    confirmedRequirementsCount: int = 0
    disputedRequirementsCount: int = 0
    avgStakeholderTrust: float = 50.0
    sprintsCompleted: int = 1

class AssessmentRequest(BaseModel):
    submissionId: int
    artifact: Optional[ArtifactSubmissionDto] = None
    artifacts: List[ArtifactSubmissionDto] = []
    sprintSummaries: List[SprintAssessmentDigestDto] = []
    processMetrics: Optional[ProcessMetricsDto] = None
    rubric: RubricDto
    evidenceList: List[AssessmentEvidenceDto] = []
    expectedKnowledgeIds: List[int] = []
    aiConnection: AiConnectionDto = Field(default_factory=AiConnectionDto)

class CriterionScoreDto(BaseModel):
    criterionCode: str
    score: float
    maxScore: float = 100.0
    comment: str

class AssessmentResponse(BaseModel):
    submissionId: int
    overallScore: float
    passed: bool = True
    criterionScores: List[CriterionScoreDto] = []
    strengths: List[str] = []
    weaknesses: List[str] = []
    criticalErrors: List[str] = []
    evaluationMode: Literal["SingleArtifact", "MapReduceProjectFinal"] = "SingleArtifact"
    expertReviewAvailable: bool = True
    promptVersion: Optional[str] = "assessor@1.1.0"
    usage: Optional[UsageStatsDto] = None
    errorCode: Optional[str] = None
    isFallback: bool = False
    error: Optional[str] = None
