from fastapi import APIRouter, Depends
from app.core.auth import verify_token
from app.schemas.stakeholder import (
    StakeholderAgentRequest,
    StakeholderAgentResponse,
    SprintReviewRequest,
    SprintReviewResponse
)
from app.services.stakeholder_service import StakeholderService

router = APIRouter(prefix="/stakeholder", tags=["Stakeholder"], dependencies=[Depends(verify_token)])
service = StakeholderService()

@router.post("/respond", response_model=StakeholderAgentResponse)
def respond_stakeholder(req: StakeholderAgentRequest):
    return service.respond(req)

@router.post("/sprint-review", response_model=SprintReviewResponse)
def sprint_review_meeting(req: SprintReviewRequest):
    return service.conduct_sprint_review(req)
