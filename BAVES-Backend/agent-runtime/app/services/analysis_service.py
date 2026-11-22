import time
from app.core.config import AGENTS_DIR
from app.core.agent_loader import AgentDefinitionLoader
from app.core.llm_factory import LLMFactory
from app.schemas.confirmation import (
    AnalysisConfirmRequest,
    AnalysisConfirmResponse,
    UsageStatsDto
)

class AnalysisService:
    """Service xử lý nghiệp vụ thẩm định và xác nhận hạng mục phân tích (Findings/Requirements)."""

    def __init__(self, loader: AgentDefinitionLoader | None = None):
        self.loader = loader or AgentDefinitionLoader(AGENTS_DIR)

    def confirm_item(self, req: AnalysisConfirmRequest) -> AnalysisConfirmResponse:
        start_time = time.time()
        context = req.model_dump()
        system_prompt, prompt_version = self.loader.render_prompt("confirmation", context)

        provider = req.aiConnection.provider
        if provider == "Mock" or not req.aiConnection.apiKey:
            return self._mock_confirm(req, prompt_version, start_time)

        return self._llm_confirm(req, system_prompt, prompt_version, start_time)

    def _mock_confirm(
        self,
        req: AnalysisConfirmRequest,
        prompt_version: str,
        start_time: float
    ) -> AnalysisConfirmResponse:
        evidence = (req.evidenceContent or "").strip().lower()

        # Stub logic: kiểm tra bằng chứng
        if not evidence or len(evidence) < 10:
            outcome = "InsufficientInformation"
            reasoning = (
                f"Tôi là {req.persona.name} ({req.persona.stakeholderRole}). "
                "Đoạn bằng chứng bạn cung cấp quá ngắn hoặc không có thông tin xác thực để tôi đối chiếu."
            )
            clarification = "Vui lòng phỏng vấn thêm và trích dẫn câu trả lời cụ thể của tôi trước khi gửi xác nhận."
        elif any(keyword in evidence for keyword in ["kho", "hủy", "pos", "đơn", "giữ", "hoàn", "tiền"]):
            outcome = "Confirmed"
            reasoning = (
                f"Tôi là {req.persona.name} ({req.persona.stakeholderRole}). "
                f"Nội dung hạng mục '{req.item.title}' hoàn toàn khớp với những gì tôi đã chia sẻ trong buổi phỏng vấn."
            )
            clarification = None
        else:
            outcome = "Disputed"
            reasoning = (
                f"Tôi là {req.persona.name} ({req.persona.stakeholderRole}). "
                f"Nội dung '{req.item.title}' chưa phản ánh đúng thực tế vận hành mà tôi đã trao đổi."
            )
            clarification = "Hãy rà soát lại các quy định và trao đổi lại với tôi để thống nhất đúng thuật ngữ."

        latency = int((time.time() - start_time) * 1000)
        return AnalysisConfirmResponse(
            outcome=outcome,
            reasoning=reasoning,
            suggestedClarification=clarification,
            promptVersion=prompt_version,
            usage=UsageStatsDto(promptTokens=100, completionTokens=80, latencyMs=latency)
        )

    def _llm_confirm(
        self,
        req: AnalysisConfirmRequest,
        system_prompt: str,
        prompt_version: str,
        start_time: float
    ) -> AnalysisConfirmResponse:
        try:
            from langchain_core.messages import SystemMessage, HumanMessage
            llm = LLMFactory.get_chat_model(
                provider=req.aiConnection.provider,
                model=req.aiConnection.model,
                api_key=req.aiConnection.apiKey,
                temperature=0.2
            )

            json_instruction = (
                "\n\nBẮT BUỘC: Hãy trả lời định dạng JSON hợp lệ với cấu trúc sau:\n"
                "{\n"
                '  "outcome": "Confirmed | Disputed | InsufficientInformation",\n'
                '  "reasoning": "giải thích từ góc nhìn nhân vật của bạn",\n'
                '  "suggestedClarification": "gợi ý làm rõ nếu có, hoặc null"\n'
                "}"
            )

            messages = [
                SystemMessage(content=system_prompt + json_instruction),
                HumanMessage(content=f"Learner gửi yêu cầu xác nhận hạng mục '{req.item.title}'. Bạn hãy thẩm định.")
            ]

            ai_msg = llm.invoke(messages)
            parsed = LLMFactory.parse_json_response(ai_msg.content)
            latency = int((time.time() - start_time) * 1000)

            return AnalysisConfirmResponse(
                outcome=parsed.get("outcome", "InsufficientInformation"),
                reasoning=parsed.get("reasoning", "Đã xem xét hạng mục."),
                suggestedClarification=parsed.get("suggestedClarification"),
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency)
            )
        except Exception as e:
            latency = int((time.time() - start_time) * 1000)
            return AnalysisConfirmResponse(
                outcome="InsufficientInformation",
                reasoning=f"Gặp lỗi kỹ thuật khi xác nhận qua AI: {str(e)}",
                suggestedClarification="Vui lòng thử lại sau.",
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency),
                error=str(e)
            )
