from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.api.v1.health import router as health_router
from app.api.v1.api_router import api_router

app = FastAPI(
    title="BA-VES AI Agent Runtime",
    version="0.1.0",
    description="Python AI Runtime Service for Virtual Elicitation Sandbox (BA-VES)"
)

# Cấu hình CORS cho phép giao tiếp linh hoạt
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Đăng ký các Router theo tầng kiến trúc rõ ràng
app.include_router(health_router)
app.include_router(api_router, prefix="/v1")
