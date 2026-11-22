import time
from app.core.config import AGENTS_DIR
from app.core.agent_loader import AgentDefinitionLoader
from app.core.llm_factory import LLMFactory
from app.core.resilience import classify_ai_error
from app.core.security_guard import SecurityGuard
from app.schemas.assessment import (
    AssessmentRequest,
    AssessmentResponse,
    CriterionScoreDto,
    UsageStatsDto
)

class AssessorService:
    """Service xử lý nghiệp vụ AI Assessor chấm điểm tài liệu hoặc tổng kết dự án (Map-Reduce) theo Rubric BABOK."""

    def __init__(self, loader: AgentDefinitionLoader | None = None):
        self.loader = loader or AgentDefinitionLoader(AGENTS_DIR)

    def evaluate(self, req: AssessmentRequest) -> AssessmentResponse:
        start_time = time.time()
        context = req.model_dump()
        system_prompt, prompt_version = self.loader.render_prompt("assessor", context)

        # Kiểm tra Indirect Prompt Injection bên trong nội dung tài liệu nộp lên
        raw_text = self._collect_combined_text(req)
        is_injected, injection_reason = SecurityGuard.detect_prompt_injection(raw_text)
        if is_injected:
            is_map_reduce = len(req.sprintSummaries) > 0 or req.processMetrics is not None
            eval_mode = "MapReduceProjectFinal" if is_map_reduce else "SingleArtifact"
            latency = int((time.time() - start_time) * 1000)
            penalized_scores = [
                CriterionScoreDto(
                    criterionCode=c.code,
                    score=0.0,
                    maxScore=c.maxScore,
                    comment="Bị hủy điểm tiêu chí do phát hiện câu lệnh Prompt Injection can thiệp hệ thống chấm điểm trong tài liệu."
                )
                for c in req.rubric.criteria
            ]
            return AssessmentResponse(
                submissionId=req.submissionId,
                overallScore=0.0,
                passed=False,
                criterionScores=penalized_scores,
                strengths=[],
                weaknesses=["Tài liệu chứa nội dung cố tình thao túng hệ thống chấm điểm tự động."],
                criticalErrors=[f"Vi phạm bảo mật (Artifact Prompt Injection): {injection_reason}"],
                evaluationMode=eval_mode,
                expertReviewAvailable=True,
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency),
                errorCode="PromptInjectionBlocked",
                error=injection_reason
            )

        provider = req.aiConnection.provider
        if provider == "Mock" or not req.aiConnection.apiKey:
            return self._mock_evaluate(req, prompt_version, start_time)

        return self._llm_evaluate(req, system_prompt, prompt_version, start_time)

    def _collect_combined_text(self, req: AssessmentRequest) -> str:
        parts: list[str] = []
        if req.artifact:
            parts.extend([s.content for s in req.artifact.sections])
        for art in req.artifacts:
            parts.extend([s.content for s in art.sections])
        for sp in req.sprintSummaries:
            parts.append(sp.compressedDigest)
        return " ".join(parts).lower()

    def _mock_evaluate(
        self,
        req: AssessmentRequest,
        prompt_version: str,
        start_time: float,
        is_fallback: bool = False
    ) -> AssessmentResponse:
        combined_text = self._collect_combined_text(req)
        is_map_reduce = len(req.sprintSummaries) > 0 or req.processMetrics is not None
        eval_mode = "MapReduceProjectFinal" if is_map_reduce else "SingleArtifact"

        scores: list[CriterionScoreDto] = []
        weighted_sum = 0.0
        total_weight = 0.0
        critical_errors: list[str] = []

        # Kiểm tra mâu thuẫn nghiệp vụ (Contradiction Check)
        if any(term in combined_text for term in ["không giới hạn cod", "khong gioi han cod", "cod trên 5 triệu", "cod tren 5 trieu", "cod > 5"]):
            critical_errors.append("Vi phạm quy tắc BR-03: Đơn hàng COD không được vượt quá 3.000.000 VNĐ.")

        sprint_avg = (
            sum(sp.sprintScore for sp in req.sprintSummaries) / len(req.sprintSummaries)
            if req.sprintSummaries else None
        )

        process_bonus = 0.0
        if req.processMetrics and req.processMetrics.totalKnowledgeCount > 0:
            coverage = req.processMetrics.revealedKnowledgeCount / req.processMetrics.totalKnowledgeCount
            if coverage >= 0.7 and req.processMetrics.confirmedRequirementsCount >= 2:
                process_bonus = 4.0

        for crit in req.rubric.criteria:
            base_score = sprint_avg if sprint_avg is not None else 80.0
            comment = f"Nội dung đáp ứng cơ bản tiêu chí {crit.name}."
            if any(kw in combined_text for kw in ["pos", "kho", "cod", "hoàn tiền", "giữ hàng", "sprint", "bap", "srs", "rtm"]):
                base_score = max(base_score, 86.0) + process_bonus
                comment = f"Hồ sơ phân tích tốt các góc độ nghiệp vụ và quá trình của tiêu chí {crit.name}."

            score_val = min(crit.maxScore, round(base_score, 1))

            if critical_errors and crit.code in ["FEASIBILITY", "CORRECTNESS"]:
                score_val = max(40.0, score_val - 30.0)
                comment += " (Bị trừ điểm do vi phạm quy tắc kinh doanh)."

            scores.append(CriterionScoreDto(
                criterionCode=crit.code,
                score=score_val,
                maxScore=crit.maxScore,
                comment=comment
            ))
            weight = crit.weight if crit.weight > 0 else 1.0
            weighted_sum += (score_val / (crit.maxScore or 100.0)) * 100.0 * weight
            total_weight += weight

        overall = round(weighted_sum / total_weight, 1) if total_weight > 0 else 85.0
        latency = int((time.time() - start_time) * 1000)

        strengths = [
            "Tài liệu trình bày cấu trúc rõ ràng, phân tách mạch lạc giữa Functional và Non-functional requirements.",
            "Có trích dẫn bằng chứng cụ thể từ các buổi phỏng vấn stakeholder."
        ]
        if is_map_reduce:
            strengths.append(f"Hoàn thành tốt lộ trình {len(req.sprintSummaries)} Sprints với sự tiến hóa phiên bản tài liệu nhất quán.")

        return AssessmentResponse(
            submissionId=req.submissionId,
            overallScore=overall,
            passed=overall >= 60.0 and len(critical_errors) == 0,
            criterionScores=scores,
            strengths=strengths,
            weaknesses=[
                "Chưa làm rõ điều kiện xử lý ngoại lệ khi đồng bộ POS gặp sự cố mạng kéo dài.",
                "Cần bổ sung bảng thuật ngữ (Glossary) để chuẩn hóa các khái niệm đơn hàng."
            ],
            criticalErrors=critical_errors,
            evaluationMode=eval_mode,
            expertReviewAvailable=True,
            promptVersion=prompt_version,
            usage=UsageStatsDto(promptTokens=280 if is_map_reduce else 350, completionTokens=200, latencyMs=latency),
            isFallback=is_fallback
        )

    def _llm_evaluate(
        self,
        req: AssessmentRequest,
        system_prompt: str,
        prompt_version: str,
        start_time: float
    ) -> AssessmentResponse:
        is_map_reduce = len(req.sprintSummaries) > 0 or req.processMetrics is not None
        eval_mode = "MapReduceProjectFinal" if is_map_reduce else "SingleArtifact"
        try:
            from langchain_core.messages import SystemMessage, HumanMessage
            llm = LLMFactory.get_chat_model(
                provider=req.aiConnection.provider,
                model=req.aiConnection.model,
                api_key=req.aiConnection.apiKey,
                temperature=0.1
            )

            json_instruction = (
                "\n\nBẮT BUỘC: Hãy trả lời định dạng JSON hợp lệ với cấu trúc sau:\n"
                "{\n"
                '  "overallScore": 85.0,\n'
                '  "passed": true,\n'
                '  "criterionScores": [\n'
                '    {"criterionCode": "COMPLETENESS", "score": 85.0, "maxScore": 100.0, "comment": "nhận xét"}\n'
                '  ],\n'
                '  "strengths": ["điểm mạnh 1", "điểm mạnh 2"],\n'
                '  "weaknesses": ["thiếu sót 1"],\n'
                '  "criticalErrors": ["lỗi vi phạm nếu có"]\n'
                "}"
            )

            messages = [
                SystemMessage(content=system_prompt + json_instruction),
                HumanMessage(content="Hãy thẩm định toàn bộ hồ sơ bài nộp trên và chấm điểm theo Rubric.")
            ]

            ai_msg = llm.invoke(messages)
            parsed = LLMFactory.parse_json_response(ai_msg.content)
            latency = int((time.time() - start_time) * 1000)

            criterion_scores = [
                CriterionScoreDto(**c) for c in parsed.get("criterionScores", [])
            ]

            weight_map = {c.code: (c.weight if c.weight > 0 else 1.0) for c in req.rubric.criteria}
            if criterion_scores:
                w_sum = sum((cs.score / (cs.maxScore or 100.0)) * 100.0 * weight_map.get(cs.criterionCode, 1.0) for cs in criterion_scores)
                w_total = sum(weight_map.get(cs.criterionCode, 1.0) for cs in criterion_scores)
                computed_overall = round(w_sum / w_total, 1) if w_total > 0 else float(parsed.get("overallScore", 0.0))
            else:
                computed_overall = float(parsed.get("overallScore", 0.0))

            critical_errors = parsed.get("criticalErrors", [])

            return AssessmentResponse(
                submissionId=req.submissionId,
                overallScore=computed_overall,
                passed=computed_overall >= 60.0 and len(critical_errors) == 0,
                criterionScores=criterion_scores,
                strengths=parsed.get("strengths", []),
                weaknesses=parsed.get("weaknesses", []),
                criticalErrors=critical_errors,
                evaluationMode=eval_mode,
                expertReviewAvailable=True,
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency)
            )
        except Exception as e:
            if req.aiConnection.demoFallback:
                return self._mock_evaluate(req, prompt_version, start_time, is_fallback=True)

            err_code, err_msg = classify_ai_error(e)
            latency = int((time.time() - start_time) * 1000)
            return AssessmentResponse(
                submissionId=req.submissionId,
                overallScore=0.0,
                passed=False,
                criterionScores=[],
                strengths=[],
                weaknesses=["Gặp sự cố kết nối AI khi chấm bài."],
                criticalErrors=[f"Lỗi hệ thống ({err_code}): {err_msg}"],
                evaluationMode=eval_mode,
                expertReviewAvailable=True,
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency),
                errorCode=err_code,
                error=err_msg
            )
