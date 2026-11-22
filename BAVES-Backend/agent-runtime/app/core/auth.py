from fastapi import Header, HTTPException, status
from app.core.config import INTERNAL_TOKEN

def verify_token(x_internal_token: str = Header(None)) -> str:
    """Xác thực token nội bộ giữa Backend A (.NET) và Backend B (Python Agent Runtime)."""
    if x_internal_token != INTERNAL_TOKEN:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid internal token"
        )
    return x_internal_token
