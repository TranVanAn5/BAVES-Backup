from fastapi import APIRouter, Depends
from app.core.auth import verify_token
from app.schemas.assessment import AssessmentRequest, AssessmentResponse
from app.services.assessor_service import AssessorService

router = APIRouter(prefix="/assessment", tags=["Assessment"], dependencies=[Depends(verify_token)])
service = AssessorService()

@router.post("/evaluate", response_model=AssessmentResponse)
def evaluate_submission(req: AssessmentRequest):
    return service.evaluate(req)
