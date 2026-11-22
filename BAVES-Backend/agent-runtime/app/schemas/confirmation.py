from typing import List, Optional, Literal
from pydantic import BaseModel, Field
from app.schemas.stakeholder import (
    StakeholderPersonaDto,
    KnowledgeItemDto,
    AiConnectionDto,
    UsageStatsDto
)

class AnalysisItemDto(BaseModel):
    itemId: Optional[int] = None
    itemType: Literal[
        "Finding",
        "Requirement",
        "BusinessRule",
        "Constraint",
        "Risk",
        "OpenQuestion",
        "Conflict",
        "Assumption"
    ] = "Requirement"
    title: str
    description: str
    importance: Optional[str] = "Medium"

class AnalysisConfirmRequest(BaseModel):
    persona: StakeholderPersonaDto
    item: AnalysisItemDto
    evidenceContent: str
    allowedKnowledge: List[KnowledgeItemDto] = []
    aiConnection: AiConnectionDto = Field(default_factory=AiConnectionDto)

class AnalysisConfirmResponse(BaseModel):
    outcome: Literal["Confirmed", "Disputed", "InsufficientInformation"]
    reasoning: str
    suggestedClarification: Optional[str] = None
    promptVersion: Optional[str] = "v1"
    usage: Optional[UsageStatsDto] = None
    error: Optional[str] = None
