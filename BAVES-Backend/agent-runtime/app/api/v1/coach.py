from fastapi import APIRouter, Depends
from app.core.auth import verify_token
from app.schemas.coach import CoachFeedbackRequest, CoachFeedbackResponse
from app.services.coach_service import CoachService

router = APIRouter(prefix="/coach", tags=["Coach"], dependencies=[Depends(verify_token)])
service = CoachService()

@router.post("/feedback", response_model=CoachFeedbackResponse)
def get_coach_feedback(req: CoachFeedbackRequest):
    return service.get_feedback(req)
