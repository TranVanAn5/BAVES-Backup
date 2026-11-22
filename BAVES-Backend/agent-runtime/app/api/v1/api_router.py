from fastapi import APIRouter
from app.api.v1.stakeholder import router as stakeholder_router
from app.api.v1.coach import router as coach_router
from app.api.v1.analysis import router as analysis_router
from app.api.v1.assessment import router as assessment_router
from app.api.v1.connections import router as connections_router

api_router = APIRouter()
api_router.include_router(stakeholder_router)
api_router.include_router(coach_router)
api_router.include_router(analysis_router)
api_router.include_router(assessment_router)
api_router.include_router(connections_router)
