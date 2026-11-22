import time
from app.core.config import AGENTS_DIR
from app.core.agent_loader import AgentDefinitionLoader
from app.core.llm_factory import LLMFactory
from app.schemas.coach import (
    CoachFeedbackRequest,
    CoachFeedbackResponse,
    UsageStatsDto
)

class CoachService:
    """Service xử lý nghiệp vụ AI Coach phản hồi và hướng dẫn kỹ năng BA."""

    def __init__(self, loader: AgentDefinitionLoader | None = None):
        self.loader = loader or AgentDefinitionLoader(AGENTS_DIR)

    def get_feedback(self, req: CoachFeedbackRequest) -> CoachFeedbackResponse:
        start_time = time.time()
        context = req.model_dump()
        system_prompt, prompt_version = self.loader.render_prompt("coach", context)

        provider = req.aiConnection.provider
        if provider == "Mock" or not req.aiConnection.apiKey:
            return self._mock_feedback(req, prompt_version, start_time)

        return self._llm_feedback(req, system_prompt, prompt_version, start_time)

    def _mock_feedback(
        self,
        req: CoachFeedbackRequest,
        prompt_version: str,
        start_time: float
    ) -> CoachFeedbackResponse:
        strengths = [
            "Bạn đã đặt câu hỏi mở giúp stakeholder chia sẻ rộng hơn về vấn đề vận hành.",
            "Duy trì thái độ lắng nghe tích cực và ghi nhận kịp thời khó khăn của bộ phận."
        ]
        gaps = [
            "Chưa làm rõ điều kiện kích hoạt giữ hàng trong kho và thời gian timeout giải phóng tồn.",
            "Chưa hỏi sâu về tác động tài chính nếu đơn hàng COD bị hoàn trả nhiều lần."
        ]
        unclear_points = [
            "Khái niệm 'đơn hàng nghi vấn' chưa được định nghĩa bằng các tiêu chí cụ thể.",
            "Chưa xác nhận lại chu kỳ đồng bộ dữ liệu giữa POS và ERP (5 phút hay theo lô cuối ngày)."
        ]
        hints = [
            "Nên dùng kỹ thuật tóm tắt (Paraphrasing) để chốt lại những điểm đã thống nhất trước khi chuyển chủ đề.",
            "Hãy đặt thêm câu hỏi 5 Whys để tìm ra nguyên nhân gốc rễ của việc trễ hạn xử lý đơn."
        ]

        latency = int((time.time() - start_time) * 1000)
        return CoachFeedbackResponse(
            sessionId=req.sessionId,
            strengths=strengths,
            gaps=gaps,
            unclearPoints=unclear_points,
            improvementHints=hints,
            promptVersion=prompt_version,
            usage=UsageStatsDto(promptTokens=150, completionTokens=120, latencyMs=latency)
        )

    def _llm_feedback(
        self,
        req: CoachFeedbackRequest,
        system_prompt: str,
        prompt_version: str,
        start_time: float
    ) -> CoachFeedbackResponse:
        try:
            from langchain_core.messages import SystemMessage, HumanMessage
            llm = LLMFactory.get_chat_model(
                provider=req.aiConnection.provider,
                model=req.aiConnection.model,
                api_key=req.aiConnection.apiKey,
                temperature=0.3
            )

            json_instruction = (
                "\n\nBẮT BUỘC: Hãy trả lời định dạng JSON hợp lệ với cấu trúc sau:\n"
                "{\n"
                '  "strengths": ["nhận xét điểm mạnh 1", "nhận xét điểm mạnh 2"],\n'
                '  "gaps": ["khoảng trống nghiệp vụ chưa hỏi 1", "khoảng trống 2"],\n'
                '  "unclearPoints": ["khái niệm mập mờ 1"],\n'
                '  "improvementHints": ["gợi ý kỹ năng 1", "gợi ý kỹ năng 2"]\n'
                "}"
            )

            messages = [
                SystemMessage(content=system_prompt + json_instruction),
                HumanMessage(content="Hãy phân tích đoạn hội thoại trên và đưa ra nhận xét kỹ năng khơi gợi yêu cầu cho tôi.")
            ]

            ai_msg = llm.invoke(messages)
            parsed = LLMFactory.parse_json_response(ai_msg.content)
            latency = int((time.time() - start_time) * 1000)

            return CoachFeedbackResponse(
                sessionId=req.sessionId,
                strengths=parsed.get("strengths", []),
                gaps=parsed.get("gaps", []),
                unclearPoints=parsed.get("unclearPoints", []),
                improvementHints=parsed.get("improvementHints", []),
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency)
            )
        except Exception as e:
            latency = int((time.time() - start_time) * 1000)
            return CoachFeedbackResponse(
                sessionId=req.sessionId,
                strengths=["Đã duy trì được mạch phỏng vấn cơ bản."],
                gaps=["Chưa thể phân tích chi tiết do gặp sự cố kết nối AI."],
                unclearPoints=[],
                improvementHints=["Vui lòng kiểm tra lại cấu hình kết nối AI Key."],
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency),
                error=str(e)
            )
