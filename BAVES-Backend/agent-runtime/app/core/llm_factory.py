import json
import re
from typing import Any, List
from fastapi import HTTPException

class _FaultSimulatedChatModel:
    """Mô hình giả lập lỗi phục vụ kiểm thử chịu lỗi (Failure & Resilience Testing - Sprint 4)."""

    def __init__(self, api_key: str):
        self.api_key = api_key
        self.call_count = 0

    def invoke(self, messages: List[Any]) -> Any:
        self.call_count += 1
        key = self.api_key.lower()
        if "invalid-key" in key or "expired-key" in key:
            raise RuntimeError("401 Unauthorized: invalid_api_key provided (sk-secret1234567890)")
        if "rate-limit" in key or "quota-exceeded" in key:
            raise RuntimeError("429 Too Many Requests: insufficient_quota / rate_limit_exceeded")
        if "timeout" in key:
            raise TimeoutError("Request timed out after 45s waiting for LLM provider")
        if "malformed-unrecoverable" in key:
            class _BadMsg:
                content = "Đây không phải là JSON hợp lệ và không có dấu ngoặc nhọn"
            return _BadMsg()
        if "malformed-recoverable" in key:
            class _RecoverableMsg:
                content = 'Dưới đây là kết quả:\n```json\n{"reply": "Đã tự động sửa lỗi JSON thành công.", "usedKnowledgeItemIds": [], "stateUpdate": {"memorySummary": "Đã phục hồi JSON.", "trustLevel": 55, "patienceLevel": 50, "cooperationLevel": 55,},}\n```\nCảm ơn!'
            return _RecoverableMsg()
        raise RuntimeError("503 Service Unavailable: upstream LLM failure")

class LLMFactory:
    """Factory để khởi tạo LangChain Chat Model tùy thuộc provider (OpenAI / Gemini) và xử lý JSON an toàn."""

    @staticmethod
    def get_chat_model(provider: str, model: str | None, api_key: str | None, temperature: float = 0.2):
        if api_key and (api_key.lower().startswith("sk-test-") or api_key.lower().startswith("sk-mock-")):
            return _FaultSimulatedChatModel(api_key)

        import os

        if provider == "OpenAI":
            from langchain_openai import ChatOpenAI
            key = api_key or os.getenv("OPENAI_API_KEY")
            return ChatOpenAI(
                model=model or "gpt-4o-mini",
                api_key=key,
                temperature=temperature
            )
        elif provider == "Gemini":
            from langchain_google_genai import ChatGoogleGenerativeAI
            key = api_key or os.getenv("GEMINI_API_KEY")
            # Tự động ánh xạ các model cũ hoặc đã bị khai tử sang model 2.5 Flash ổn định
            target_model = model or "gemini-2.5-flash"
            if target_model in ["gemini-1.5-flash", "gemini-1.5-pro", "gemini-2.0-flash"]:
                target_model = "gemini-2.5-flash"
            return ChatGoogleGenerativeAI(
                model=target_model,
                google_api_key=key,
                temperature=temperature
            )
        else:
            raise HTTPException(status_code=400, detail=f"Unsupported AI provider: {provider}")

    @staticmethod
    def parse_json_response(res_text: str) -> dict[str, Any]:
        """
        Trích xuất và tự động sửa lỗi (Auto-Repair) các lỗi cú pháp JSON phổ biến từ LLM:
        - Loại bỏ markdown fences (```json ... ```)
        - Trích xuất khối {...} nếu LLM viết thêm lời dẫn ở đầu/cuối
        - Xóa dấu phẩy thừa (trailing commas) trước } hoặc ]
        """
        if not res_text or not res_text.strip():
            raise ValueError("InvalidOutput: Empty JSON response from LLM")

        text = res_text.strip()
        if text.startswith("```json"):
            text = text[7:]
        elif text.startswith("```"):
            text = text[3:]
        if text.endswith("```"):
            text = text[:-3]
        text = text.strip()

        try:
            return json.loads(text)
        except json.JSONDecodeError:
            pass

        # Bước Auto-Repair 1: Tìm cặp ngoặc nhọn ngoài cùng {...}
        start_idx = text.find("{")
        end_idx = text.rfind("}")
        if start_idx != -1 and end_idx != -1 and end_idx > start_idx:
            candidate = text[start_idx : end_idx + 1]
            # Bước Auto-Repair 2: Loại bỏ trailing commas trước } hoặc ]
            candidate_fixed = re.sub(r",\s*([}\]])", r"\1", candidate)
            try:
                return json.loads(candidate_fixed)
            except json.JSONDecodeError as err:
                raise ValueError(f"InvalidOutput: JSONDecodeError after auto-repair - {str(err)}") from err

        raise ValueError("InvalidOutput: Malformed output does not contain a JSON object")
