import logging
from app.core.llm_factory import LLMFactory

logger = logging.getLogger(__name__)

class ConnectionService:
    """Service kiểm tra và xác thực kết nối AI Provider."""

    @staticmethod
    def validate_connection(payload: dict) -> dict:
        provider = payload.get("provider", "OpenAI")
        model = payload.get("model")
        api_key = (payload.get("apiKey") or "").strip()

        if not api_key:
            return {
                "valid": False,
                "provider": provider,
                "model": model,
                "message": "API Key không được để trống."
            }

        if provider == "Mock":
            return {
                "valid": True,
                "provider": provider,
                "model": model,
                "message": "Chế độ Mock Provider sẵn sàng."
            }

        # Nếu là key test nội bộ (Fault simulation)
        if api_key.lower().startswith("sk-test-") or api_key.lower().startswith("sk-mock-"):
            key_lower = api_key.lower()
            if "invalid" in key_lower or "expired" in key_lower:
                return {
                    "valid": False,
                    "provider": provider,
                    "model": model,
                    "message": "Khóa kiểm thử không hợp lệ hoặc đã hết hạn."
                }
            return {
                "valid": True,
                "provider": provider,
                "model": model,
                "message": f"Kết nối thử nghiệm thành công ({provider})."
            }

        try:
            from langchain_core.messages import HumanMessage
            llm = LLMFactory.get_chat_model(
                provider=provider,
                model=model,
                api_key=api_key,
                temperature=0.1
            )
            # Gửi tin nhắn ping ngắn gọn để kiểm tra xác thực và model
            llm.invoke([HumanMessage(content="ping")])

            chosen_model = model or ("gemini-2.5-flash" if provider == "Gemini" else "gpt-4o-mini")
            return {
                "valid": True,
                "provider": provider,
                "model": chosen_model,
                "message": f"Kết nối tới {provider} ({chosen_model}) thành công!"
            }

        except Exception as e:
            err_str = str(e)
            logger.warning(f"Connection validation failed for {provider}: {err_str}")

            if "404" in err_str or "not found" in err_str.lower():
                target_model = model or ("gemini-1.5-flash" if provider == "Gemini" else "gpt-4o-mini")
                msg = (
                    f"Model '{target_model}' không tồn tại hoặc API Key chưa kích hoạt Generative Language API. "
                    "Khuyến nghị: Lấy API Key trực tiếp từ Google AI Studio (https://aistudio.google.com/app/apikey) "
                    "hoặc thử đổi sang model 'gemini-2.0-flash'."
                )
            elif "400" in err_str or "API_KEY_INVALID" in err_str or "invalid_api_key" in err_str:
                msg = f"API Key của {provider} không hợp lệ. Vui lòng kiểm tra lại key đã sao chép."
            elif "429" in err_str or "RESOURCE_EXHAUSTED" in err_str or "quota" in err_str.lower() or "rate_limit" in err_str.lower():
                msg = f"API Key đã vượt quá hạn mức sử dụng (Quota / Rate Limit) của {provider}."
            elif "timeout" in err_str.lower() or "timed out" in err_str.lower():
                msg = f"Kết nối tới máy chủ {provider} bị quá thời gian (Timeout). Vui lòng thử lại."
            else:
                msg = f"Lỗi kết nối {provider}: {err_str[:200]}"

            return {
                "valid": False,
                "provider": provider,
                "model": model,
                "message": msg
            }
