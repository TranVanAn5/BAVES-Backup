from fastapi import APIRouter, Depends
from app.core.auth import verify_token
from app.schemas.confirmation import AnalysisConfirmRequest, AnalysisConfirmResponse
from app.services.analysis_service import AnalysisService

router = APIRouter(prefix="/analysis", tags=["Analysis"], dependencies=[Depends(verify_token)])
service = AnalysisService()

@router.post("/confirm", response_model=AnalysisConfirmResponse)
def confirm_analysis_item(req: AnalysisConfirmRequest):
    return service.confirm_item(req)
