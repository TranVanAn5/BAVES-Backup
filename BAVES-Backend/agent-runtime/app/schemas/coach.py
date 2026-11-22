from typing import List, Optional
from pydantic import BaseModel, Field
from app.schemas.stakeholder import ChatMessageDto, AiConnectionDto, UsageStatsDto

class BaPlanContextDto(BaseModel):
    objective: Optional[str] = None
    approach: Optional[str] = None
    plannedQuestionsCount: Optional[int] = 0

class CoachFeedbackRequest(BaseModel):
    sessionId: int
    baPlan: Optional[BaPlanContextDto] = None
    recentMessages: List[ChatMessageDto] = []
    aiConnection: AiConnectionDto = Field(default_factory=AiConnectionDto)

class CoachFeedbackResponse(BaseModel):
    sessionId: int
    strengths: List[str] = []
    gaps: List[str] = []
    unclearPoints: List[str] = []
    improvementHints: List[str] = []
    promptVersion: Optional[str] = "v1"
    usage: Optional[UsageStatsDto] = None
    error: Optional[str] = None
