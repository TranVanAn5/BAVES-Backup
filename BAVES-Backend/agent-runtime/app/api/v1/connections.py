from fastapi import APIRouter, Depends
from app.core.auth import verify_token
from app.services.connection_service import ConnectionService

router = APIRouter(prefix="/connections", tags=["Connections"], dependencies=[Depends(verify_token)])

@router.post("/validate")
def validate_connection(payload: dict):
    return ConnectionService.validate_connection(payload)
