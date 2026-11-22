import re
from typing import Tuple

SECRET_PATTERNS = [
    re.compile(r"sk-[A-Za-z0-9_\-]{10,}"),
    re.compile(r"AIza[A-Za-z0-9_\-]{10,}"),
]

def redact_secrets(text: str) -> str:
    """Loại bỏ mọi API Key (OpenAI sk-..., Google AIza...) khỏi thông báo lỗi và log."""
    if not text:
        return ""
    sanitized = text
    for pattern in SECRET_PATTERNS:
        sanitized = pattern.sub("[REDACTED_API_KEY]", sanitized)
    return sanitized

def classify_ai_error(exc: Exception) -> Tuple[str, str]:
    """
    Phân loại ngoại lệ từ LLM Provider sang mã lỗi chuẩn hóa của BA-VES:
    - InvalidApiKey: Sai API Key, hết hạn key, 401/403 hoặc provider không hợp lệ
    - RateLimited: Hết hạn mức token/quota (429 / insufficient_quota / resource_exhausted)
    - Timeout: Quá thời gian chờ phản hồi từ LLM
    - InvalidOutput: LLM trả về JSON sai cấu trúc không thể tự sửa
    - ServiceUnavailable: Lỗi mạng hoặc máy chủ AI bên thứ ba gặp sự cố
    """
    raw_msg = redact_secrets(str(exc))
    lower_msg = raw_msg.lower()

    if any(k in lower_msg for k in [
        "invalid_api_key", "invalid api key", "incorrect api key", "authentication",
        "unauthorized", "401", "api_key_invalid", "unsupported ai provider", "permission_denied"
    ]):
        return "InvalidApiKey", f"API Key không hợp lệ hoặc đã hết hạn. Vui lòng kiểm tra lại trong Settings: {raw_msg}"

    if any(k in lower_msg for k in [
        "rate_limit", "rate limit", "ratelimit", "429", "quota", "insufficient_quota",
        "resource_exhausted", "too many requests", "token limit"
    ]):
        return "RateLimited", f"API Key đã hết hạn mức (Quota/Token) hoặc vượt giới hạn gọi mỗi phút. Vui lòng đổi API Key mới trong Settings để tiếp tục từ điểm dừng: {raw_msg}"

    if any(k in lower_msg for k in [
        "timeout", "timed out", "deadline exceeded", "readtimeout", "connecttimeout"
    ]):
        return "Timeout", f"Kết nối tới nhà cung cấp AI bị quá thời gian chờ (Timeout). Vui lòng bấm thử lại: {raw_msg}"

    if any(k in lower_msg for k in [
        "jsondecodeerror", "expecting value", "expecting property name", "invalid json", "malformed"
    ]):
        return "InvalidOutput", f"Phản hồi từ AI không đúng định dạng cấu trúc JSON yêu cầu: {raw_msg}"

    return "ServiceUnavailable", f"Dịch vụ AI tạm thời gián đoạn: {raw_msg}"
