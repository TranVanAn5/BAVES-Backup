import time
import logging
from app.core.config import AGENTS_DIR
from app.core.agent_loader import AgentDefinitionLoader
from app.core.llm_factory import LLMFactory
from app.core.resilience import classify_ai_error
from app.core.security_guard import SecurityGuard
from app.schemas.stakeholder import (
    StakeholderAgentRequest,
    StakeholderAgentResponse,
    AgentStateUpdateDto,
    UsageStatsDto,
    SprintReviewRequest,
    SprintReviewResponse,
    SprintReviewSummaryDto
)

logger = logging.getLogger(__name__)

class StakeholderService:
    """Service xử lý nghiệp vụ phỏng vấn Stakeholder AI Agent và Hội đồng Chủ trì Sprint Review."""

    def __init__(self, loader: AgentDefinitionLoader | None = None):
        self.loader = loader or AgentDefinitionLoader(AGENTS_DIR)

    def _resolve_ai_connection(self, ai_connection):
        import os
        provider = (ai_connection.provider or "").strip() or os.getenv("DEFAULT_AI_PROVIDER", "Gemini")
        api_key = (ai_connection.apiKey or "").strip()
        if not api_key and provider != "Mock":
            if provider == "Gemini":
                api_key = os.getenv("GEMINI_API_KEY", "").strip()
            elif provider == "OpenAI":
                api_key = os.getenv("OPENAI_API_KEY", "").strip()
            else:
                api_key = os.getenv(f"{provider.upper()}_API_KEY", "").strip()

            # Nếu provider gửi sang (vd: mặc định 'OpenAI' từ BAVES.Api khi user không nhập BYOK)
            # không có key trong .env, tự động dùng DEFAULT_AI_PROVIDER (Gemini) đã cấu hình trong .env
            if not api_key:
                default_provider = os.getenv("DEFAULT_AI_PROVIDER", "Gemini").strip()
                default_key = (
                    os.getenv(f"{default_provider.upper()}_API_KEY", "").strip()
                    or os.getenv("GEMINI_API_KEY", "").strip()
                )
                if default_key:
                    provider = default_provider
                    api_key = default_key
                    ai_connection.model = None

        model = ai_connection.model
        if provider == "Gemini":
            if not model or model in ["gemini-1.5-flash", "gemini-1.5-pro", "gemini-2.0-flash"] or model.startswith("gpt-"):
                model = os.getenv("DEFAULT_AI_MODEL", "gemini-2.5-flash")
        elif provider == "OpenAI":
            if not model or model.startswith("gemini-"):
                model = "gpt-4o-mini"

        ai_connection.provider = provider
        ai_connection.apiKey = api_key
        ai_connection.model = model
        return provider, api_key, model

    def respond(self, req: StakeholderAgentRequest) -> StakeholderAgentResponse:
        start_time = time.time()

        # 1. Kiểm tra chống Prompt Injection ngay trên câu hỏi của Learner
        is_injected, injection_reason = SecurityGuard.detect_prompt_injection(req.learnerMessage)
        if is_injected:
            latency = int((time.time() - start_time) * 1000)
            return StakeholderAgentResponse(
                requestId=req.requestId,
                reply=(
                    f"[{req.persona.name}]: Tôi là {req.persona.stakeholderRole} của công ty. "
                    "Tôi chỉ trả lời các câu hỏi nghiệp vụ trong phạm vi công việc của bộ phận mình, "
                    "không thực hiện các yêu cầu nằm ngoài cuộc phỏng vấn khảo sát BA."
                ),
                usedKnowledgeItemIds=[],
                stateUpdate=AgentStateUpdateDto(
                    memorySummary=req.agentState.memorySummary or "",
                    trustLevel=max(0, req.agentState.trustLevel - 5),
                    patienceLevel=max(0, req.agentState.patienceLevel - 5),
                    cooperationLevel=req.agentState.cooperationLevel
                ),
                promptVersion="stakeholder@1.0.0",
                usage=UsageStatsDto(latencyMs=latency),
                errorCode="PromptInjectionBlocked",
                error=injection_reason
            )

        # 2. Lọc cứng tri thức ở tầng Code (Chặn DoesNotKnow, Canary và Hidden chưa đủ điều kiện mở)
        disclosable_knowledge = SecurityGuard.filter_disclosable_knowledge(
            allowed_knowledge=req.allowedKnowledge,
            learner_message=req.learnerMessage,
            history=req.history,
            trust_level=req.agentState.trustLevel
        )

        context = req.model_dump()
        sanitized_knowledge = []
        for k in disclosable_knowledge:
            k_dict = k.model_dump()
            if k.accessType == "Partial":
                first_line = (k.content or "").split("\n")[0].split(".")[0].strip()
                k_dict["content"] = (
                    f"[Chỉ biết bề nổi, không nắm chi tiết - hãy chỉ sang bộ phận phụ trách chính]: "
                    f"{k.title}. {first_line}."
                )
            sanitized_knowledge.append(k_dict)
        context["allowedKnowledge"] = sanitized_knowledge
        system_prompt, prompt_version = self.loader.render_prompt("stakeholder", context)

        provider, api_key, _ = self._resolve_ai_connection(req.aiConnection)
        if provider == "Mock" or not api_key:
            return self._mock_respond(req, disclosable_knowledge, prompt_version, start_time)

        return self._llm_respond(req, disclosable_knowledge, system_prompt, prompt_version, start_time)

    def _mock_respond(
        self,
        req: StakeholderAgentRequest,
        disclosable_knowledge: list,
        prompt_version: str,
        start_time: float,
        is_fallback: bool = False
    ) -> StakeholderAgentResponse:
        msg_lower = (req.learnerMessage or "").lower().strip()

        import re
        event_note = ""
        if req.eventContext:
            recent_event = req.eventContext[-1]
            event_note = f" (Lưu ý: Tình hình vừa có biến cố '{recent_event.title}' khiến chúng tôi phải điều chỉnh tiến độ)."

        persona_name = req.persona.name
        persona_role = req.persona.stakeholderRole
        dept = req.persona.department or "công ty"

        # Chấm điểm độ khớp từ khóa để chọn thẻ knowledge phù hợp nhất trong tập disclosable_knowledge
        scored_items = []
        for k in disclosable_knowledge:
            tokens = set(
                t.strip(".,?:;()\"'")
                for t in f"{k.title} {k.content}".lower().split()
                if len(t.strip(".,?:;()\"'")) >= 3
            )
            score = sum(1 for tok in tokens if tok in msg_lower)
            if score > 0:
                # Ưu tiên tuyệt đối thẻ 'Knows' của đúng bộ phận hơn thẻ 'Partial'
                priority_bonus = 100 if k.accessType == "Knows" else 0
                scored_items.append((score + priority_bonus, k))

        scored_items.sort(key=lambda x: x[0], reverse=True)

        # Nhận diện các câu chào hỏi, xưng hô, thăm dò lịch sự
        words = set(re.findall(r"\w+", msg_lower))
        greeting_words = {"chào", "hi", "hello", "alo"}
        greeting_phrases = [
            "anh ơi", "chị ơi", "em ơi", "bác ơi",
            "rảnh không", "rãnh không", "bận không", "có đó không", "có ở đây không",
            "khỏe không", "khoẻ không", "bạn là ai", "anh là ai", "chị là ai",
            "giới thiệu", "nói chuyện", "trao đổi một chút"
        ]
        is_greeting = any(w in greeting_words for w in words) or any(p in msg_lower for p in greeting_phrases)

        if scored_items:
            top_card = scored_items[0][1]
            used_ids = [top_card.knowledgeItemId]
            first_sentence = (top_card.content or "").split("\n")[0].split(".")[0].strip()
            prefix = "" if is_fallback else f"[{persona_name}]: "
            stub_reply = f"{prefix}{first_sentence}.{event_note}"
        elif is_greeting:
            used_ids = []
            stub_reply = (
                f"Chào bạn, tôi là {persona_name}. Tôi đang xử lý công việc bên {dept}, "
                f"có thể trao đổi nhanh với bạn vài phút. Bạn muốn hỏi về phần việc nào?"
            )
        else:
            used_ids = []
            stub_reply = (
                f"Về chỗ '{req.learnerMessage}' thì tôi chưa nắm rõ hoặc nằm ngoài phạm vi phụ trách của {dept}. "
                f"Tôi chủ yếu phụ trách {req.persona.responsibilities or 'vận hành bộ phận'}. "
                f"Bạn hỏi cụ thể hơn vào quy trình công việc xem sao."
            )

        latency = int((time.time() - start_time) * 1000)
        prev_summary = req.agentState.memorySummary or ""
        new_summary = f"{prev_summary} Đã trao đổi về '{req.learnerMessage[:40]}...'".strip()

        if is_fallback:
            return StakeholderAgentResponse(
                requestId=req.requestId,
                reply=stub_reply,
                usedKnowledgeItemIds=[],
                stateUpdate=AgentStateUpdateDto(
                    memorySummary=prev_summary,
                    trustLevel=req.agentState.trustLevel,
                    patienceLevel=req.agentState.patienceLevel,
                    cooperationLevel=req.agentState.cooperationLevel
                ),
                promptVersion=prompt_version,
                usage=UsageStatsDto(promptTokens=120, completionTokens=65, latencyMs=latency),
                isFallback=True
            )

        return StakeholderAgentResponse(
            requestId=req.requestId,
            reply=stub_reply,
            usedKnowledgeItemIds=used_ids,
            stateUpdate=AgentStateUpdateDto(
                memorySummary=new_summary[-300:],
                trustLevel=min(100, req.agentState.trustLevel + 1),
                patienceLevel=max(0, req.agentState.patienceLevel - 1) if not is_greeting else req.agentState.patienceLevel,
                cooperationLevel=req.agentState.cooperationLevel
            ),
            promptVersion=prompt_version,
            usage=UsageStatsDto(promptTokens=120, completionTokens=65, latencyMs=latency),
            isFallback=False
        )

    def _llm_respond(
        self,
        req: StakeholderAgentRequest,
        disclosable_knowledge: list,
        system_prompt: str,
        prompt_version: str,
        start_time: float
    ) -> StakeholderAgentResponse:
        try:
            from langchain_core.messages import SystemMessage, HumanMessage
            llm = LLMFactory.get_chat_model(
                provider=req.aiConnection.provider,
                model=req.aiConnection.model,
                api_key=req.aiConnection.apiKey,
                temperature=0.7
            )

            json_instruction = (
                "\n\nLƯU Ý QUAN TRỌNG TRƯỚC KHI TRẢ LỜI:\n"
                "- Trả lời CỰC KỲ NGẮN GỌN: chỉ 1 đến tối đa 3 câu ngắn trên 1 đoạn duy nhất (không tách nhiều đoạn).\n"
                f"- Thể hiện rõ tính cách của {req.persona.name} ({req.persona.stakeholderRole}): nói đúng trọng tâm nhưng KHÔNG kể lể chi tiết. "
                "Nếu câu hỏi rộng/chung chung, chỉ nói bề nổi hoặc hỏi vặn lại và CỐ TÌNH GIỮ LẠI các chi tiết nhỏ/con số cụ thể để learner phải tự hỏi đào sâu ở lượt sau.\n"
                "- Mỗi lượt chỉ tiết lộ tối đa 1 ý nhỏ (tối đa 1 thẻ knowledge).\n"
                "\nBẮT BUỘC: Hãy trả lời định dạng JSON với schema sau:\n"
                "{\n"
                '  "reply": "câu trả lời ngắn gọn (1-3 câu) đúng tính cách",\n'
                '  "usedKnowledgeItemIds": [danh sách id thẻ knowledge đã dùng, tối đa 1 id],\n'
                '  "stateUpdate": {\n'
                '    "memorySummary": "tóm tắt hội thoại",\n'
                '    "trustLevel": 50,\n'
                '    "patienceLevel": 50,\n'
                '    "cooperationLevel": 50\n'
                "  }\n"
                "}"
            )

            messages = [
                SystemMessage(content=system_prompt + json_instruction),
                HumanMessage(
                    content=(
                        f"Learner nhắn cho {req.persona.name} ({req.persona.stakeholderRole}): {req.learnerMessage}\n"
                        "(Hãy trả lời tự nhiên trong 1-3 câu ngắn đúng tính cách, không nói dài dòng, giữ lại chi tiết nhỏ nếu chưa được hỏi sâu)"
                    )
                )
            ]

            ai_msg = llm.invoke(messages)
            parsed = LLMFactory.parse_json_response(ai_msg.content)
            latency = int((time.time() - start_time) * 1000)

            allowed_ids = {k.knowledgeItemId for k in disclosable_knowledge}
            used_ids = [k_id for k_id in parsed.get("usedKnowledgeItemIds", []) if k_id in allowed_ids]

            return StakeholderAgentResponse(
                requestId=req.requestId,
                reply=parsed.get("reply", "Tôi chưa rõ ý bạn."),
                usedKnowledgeItemIds=used_ids,
                stateUpdate=AgentStateUpdateDto(
                    memorySummary=parsed.get("stateUpdate", {}).get("memorySummary") or req.agentState.memorySummary or "",
                    trustLevel=parsed.get("stateUpdate", {}).get("trustLevel", req.agentState.trustLevel),
                    patienceLevel=parsed.get("stateUpdate", {}).get("patienceLevel", req.agentState.patienceLevel),
                    cooperationLevel=parsed.get("stateUpdate", {}).get("cooperationLevel", req.agentState.cooperationLevel)
                ),
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency)
            )
        except Exception as e:
            err_code, err_msg = classify_ai_error(e)
            logger.warning(
                f"LLM invocation failed for session {req.sessionId} with [{err_code}]: {err_msg}."
            )
            # Nếu gặp lỗi model không tìm thấy (404/NOT_FOUND), thử tự động chuyển sang gemini-2.5-flash
            if req.aiConnection.provider == "Gemini" and req.aiConnection.model != "gemini-2.5-flash":
                try:
                    logger.info("Retrying with stable model gemini-2.5-flash...")
                    fallback_llm = LLMFactory.get_chat_model(
                        provider="Gemini",
                        model="gemini-2.5-flash",
                        api_key=req.aiConnection.apiKey,
                        temperature=0.7
                    )
                    ai_msg = fallback_llm.invoke(messages)
                    parsed = LLMFactory.parse_json_response(ai_msg.content)
                    latency = int((time.time() - start_time) * 1000)

                    allowed_ids = {k.knowledgeItemId for k in disclosable_knowledge}
                    used_ids = [k_id for k_id in parsed.get("usedKnowledgeItemIds", []) if k_id in allowed_ids]

                    return StakeholderAgentResponse(
                        requestId=req.requestId,
                        reply=parsed.get("reply", "Tôi chưa rõ ý bạn."),
                        usedKnowledgeItemIds=used_ids,
                        stateUpdate=AgentStateUpdateDto(
                            memorySummary=parsed.get("stateUpdate", {}).get("memorySummary") or req.agentState.memorySummary or "",
                            trustLevel=parsed.get("stateUpdate", {}).get("trustLevel", req.agentState.trustLevel),
                            patienceLevel=parsed.get("stateUpdate", {}).get("patienceLevel", req.agentState.patienceLevel),
                            cooperationLevel=parsed.get("stateUpdate", {}).get("cooperationLevel", req.agentState.cooperationLevel)
                        ),
                        promptVersion=prompt_version,
                        usage=UsageStatsDto(latencyMs=latency)
                    )
                except Exception as retry_err:
                    logger.warning(f"Retry with gemini-2.5-flash failed: {retry_err}")

            # Tự động chuyển sang câu trả lời tri thức nghiệp vụ để học viên không bị gián đoạn
            fallback_res = self._mock_respond(req, disclosable_knowledge, prompt_version, start_time, is_fallback=True)
            fallback_res.errorCode = err_code
            fallback_res.error = err_msg
            return fallback_res

    def conduct_sprint_review(self, req: SprintReviewRequest) -> SprintReviewResponse:
        start_time = time.time()

        # Kiểm tra Prompt Injection trong phần giải trình của BA hoặc trong nội dung tài liệu nộp lên
        combined_artifact_text = " ".join([s.content for a in req.artifacts for s in a.sections])
        is_injected, injection_reason = SecurityGuard.detect_prompt_injection(
            f"{req.learnerReply or ''} {combined_artifact_text}"
        )
        if is_injected:
            primary_chair = req.chairPersonas[0].name if req.chairPersonas else "Chủ trì cuộc họp"
            latency = int((time.time() - start_time) * 1000)
            return SprintReviewResponse(
                requestId=req.requestId,
                sessionId=req.sessionId,
                sprintNo=req.sprintNo,
                chairSpeakerName=primary_chair,
                chairReply="Hội đồng phát hiện nội dung không hợp lệ (câu lệnh can thiệp quy trình đánh giá). Yêu cầu bạn chỉnh sửa lại tài liệu nghiêm túc.",
                challengeQuestions=["Vui lòng loại bỏ các câu lệnh điều khiển hệ thống khỏi tài liệu và giải trình đúng trọng tâm nghiệp vụ."],
                sprintSummary=SprintReviewSummaryDto(
                    sprintNo=req.sprintNo,
                    decision="NeedsRevision",
                    sprintScore=30.0,
                    approvedPoints=[],
                    actionItemsForNextSprint=["Loại bỏ nội dung Prompt Injection và viết lại đặc tả nghiệp vụ thực tế."],
                    compressedDigest=f"Sprint {req.sprintNo}: Bị từ chối (NeedsRevision) do phát hiện Prompt Injection.",
                    nextSprintUnlocked=False
                ) if req.finalizeSprint else None,
                promptVersion="sprint_review@1.0.0",
                usage=UsageStatsDto(latencyMs=latency),
                errorCode="PromptInjectionBlocked",
                error=injection_reason
            )

        context = req.model_dump()
        system_prompt, prompt_version = self.loader.render_prompt("sprint_review", context)

        provider, api_key, _ = self._resolve_ai_connection(req.aiConnection)
        if provider == "Mock" or not api_key:
            return self._mock_sprint_review(req, prompt_version, start_time)

        return self._llm_sprint_review(req, system_prompt, prompt_version, start_time)

    def _mock_sprint_review(
        self,
        req: SprintReviewRequest,
        prompt_version: str,
        start_time: float,
        is_fallback: bool = False
    ) -> SprintReviewResponse:
        chair_names = ", ".join([p.name for p in req.chairPersonas]) if req.chairPersonas else "Hội đồng Stakeholder"
        primary_chair = req.chairPersonas[0].name if req.chairPersonas else "Chủ trì cuộc họp"
        artifact_titles = ", ".join([f"{a.title} ({a.artifactType} v{a.versionNo})" for a in req.artifacts]) or "Tài liệu Sprint"
        combined_text = " ".join([s.content for a in req.artifacts for s in a.sections]).lower()

        has_cod_violation = any(
            term in combined_text
            for term in ["không giới hạn cod", "cod trên 5 triệu", "cod > 5"]
        )

        missing = ", ".join(req.missingDeliverableCodes) if req.missingDeliverableCodes else ""
        if not req.finalizeSprint:
            questions = [
                "Trong artifact hiện tại, chỗ nào còn chưa khớp với quy trình thực tế phòng ban bạn mô tả khi phỏng vấn?",
                "Nếu phát sinh ngoại lệ (lệch tồn / trễ đồng bộ), tài liệu đã ghi trách nhiệm và bước xử lý cụ thể chưa?"
            ]
            missing_note = f" Hiện còn thiếu deliverable: {missing}." if missing else ""
            reply = (
                f"[{chair_names}]: Chúng tôi đã đối chiếu {artifact_titles} với yêu cầu sprint.{missing_note} "
                "Ở góc nhìn vận hành, vẫn còn vài điểm chưa đủ rõ — mời bạn giải trình các câu hỏi dưới đây."
            )
            latency = int((time.time() - start_time) * 1000)
            return SprintReviewResponse(
                requestId=req.requestId,
                sessionId=req.sessionId,
                sprintNo=req.sprintNo,
                chairSpeakerName=primary_chair,
                chairReply=reply,
                challengeQuestions=questions,
                sprintSummary=None,
                promptVersion=prompt_version,
                usage=UsageStatsDto(promptTokens=180, completionTokens=95, latencyMs=latency),
                isFallback=is_fallback
            )

        is_final = req.sprintNo >= req.totalSprints or req.reviewPhase == "FinalizeFinal"
        decision = "NeedsRevision" if has_cod_violation else "ApprovedWithRevisions"
        score = 55.0 if has_cod_violation else 85.0
        approved_points = [] if not is_final else [
            f"Đã hoàn thành bộ tài liệu {artifact_titles} cho Sprint {req.sprintNo}.",
            "Phạm vi bám sát nhu cầu vận hành các phòng ban liên quan."
        ]
        action_items = (
            [
                "Sửa quy tắc trần COD (tối đa 3.000.000 VNĐ) cho khớp kịch bản nghiệp vụ.",
                "Bổ sung bước xử lý khi lệch tồn kho giữa các kênh.",
                "Làm rõ trách nhiệm Ops / IT / Sales khi đồng bộ thất bại."
            ]
            if has_cod_violation
            else [
                f"Bổ sung chi tiết ngoại lệ vào phiên bản artifact trước Sprint {min(req.totalSprints, req.sprintNo + 1)}.",
                "Đối chiếu lại bằng chứng phỏng vấn với từng mục trong tài liệu.",
                "Làm rõ tiêu chí chấp nhận giữa các phòng ban liên quan."
            ]
        )
        if missing:
            action_items = [f"Bổ sung và nộp các deliverable còn thiếu: {missing}."] + action_items
        prior_note = ""
        if req.priorArtifactsForComparison:
            prior_note = (
                f" Đã so sánh {len(req.priorArtifactsForComparison)} artifact mang sang với bản sprint trước"
                f"{' — có dấu hiệu cải thiện.' if score >= 70 else ' — vẫn còn tồn đọng cần chỉnh.'}"
            )
        elif req.priorSprintDigests:
            last_prior = req.priorSprintDigests[-1]
            prior_note = (
                f" So với Sprint {last_prior.sprintNo}: "
                f"{'có dấu hiệu cải thiện các action item cũ' if score >= (last_prior.score or 0) else 'vẫn còn tồn đọng so với góp ý trước'}."
            )
        compressed_digest = (
            f"Sprint {req.sprintNo}/{req.totalSprints}: {decision}, score nội bộ {score}. "
            f"Cải thiện: {action_items[0]}.{prior_note}"
        )

        missing_prefix = f"Còn thiếu {missing}. " if missing else ""
        if is_final:
            reply = (
                f"[{chair_names} - Kết luận dự án]: {missing_prefix}"
                f"Hội đồng đánh giá tổng thể '{decision}' ({score}/100) trên các artifact đã nộp, "
                f"đã đối chiếu cải thiện qua các sprint trước và chốt nghiệm thu hồ sơ."
            )
        else:
            reply = (
                f"[{chair_names}]: {missing_prefix}"
                f"Chúng tôi đã chấm các artifact đã nộp ({artifact_titles}). "
                f"Chưa công bố điểm cuối kỳ — các điểm cần cải thiện: {action_items[0]} "
                f"{action_items[1] if len(action_items) > 1 else ''}{prior_note}"
            )
        latency = int((time.time() - start_time) * 1000)

        return SprintReviewResponse(
            requestId=req.requestId,
            sessionId=req.sessionId,
            sprintNo=req.sprintNo,
            chairSpeakerName=primary_chair,
            chairReply=reply,
            challengeQuestions=[],
            sprintSummary=SprintReviewSummaryDto(
                sprintNo=req.sprintNo,
                decision=decision,
                sprintScore=score,
                approvedPoints=approved_points,
                actionItemsForNextSprint=action_items,
                compressedDigest=compressed_digest,
                nextSprintUnlocked=True
            ),
            promptVersion=prompt_version,
            usage=UsageStatsDto(promptTokens=210, completionTokens=130, latencyMs=latency),
            isFallback=is_fallback
        )

    def _llm_sprint_review(
        self,
        req: SprintReviewRequest,
        system_prompt: str,
        prompt_version: str,
        start_time: float
    ) -> SprintReviewResponse:
        primary_chair = req.chairPersonas[0].name if req.chairPersonas else "Chủ trì cuộc họp"
        try:
            from langchain_core.messages import SystemMessage, HumanMessage
            llm = LLMFactory.get_chat_model(
                provider=req.aiConnection.provider,
                model=req.aiConnection.model,
                api_key=req.aiConnection.apiKey,
                temperature=0.3
            )

            json_instruction = (
                "\n\nBẮT BUỘC: Hãy trả lời định dạng JSON hợp lệ với schema sau:\n"
                "{\n"
                '  "chairSpeakerName": "tên người chủ trì",\n'
                '  "chairReply": "phát biểu nhận xét hoặc kết luận cuộc họp",\n'
                '  "challengeQuestions": ["câu hỏi chất vấn 1"],\n'
                '  "sprintSummary": {\n'
                f'    "sprintNo": {req.sprintNo},\n'
                '    "decision": "Approved | ApprovedWithRevisions | NeedsRevision",\n'
                '    "sprintScore": 85.0,\n'
                '    "approvedPoints": ["điểm đạt 1"],\n'
                '    "actionItemsForNextSprint": ["việc cần cập nhật ở Sprint sau"],\n'
                '    "compressedDigest": "tóm tắt nén dưới 150 từ cho Map-Reduce cuối dự án",\n'
                '    "nextSprintUnlocked": true\n'
                "  }\n"
                "}"
            )

            phase = req.reviewPhase or (
                "Discuss" if not req.finalizeSprint
                else ("FinalizeFinal" if req.sprintNo >= req.totalSprints else "FinalizeMid")
            )
            missing = ", ".join(req.missingDeliverableCodes) if req.missingDeliverableCodes else "(không thiếu)"
            user_prompt = (
                f"Hãy đánh giá artifact đã nộp cho Sprint {req.sprintNo} "
                f"(phase={phase}, revealScore={req.revealNumericScoreToLearner}, "
                f"missing={missing}, compareCount={len(req.priorArtifactsForComparison)}). "
                f"{'Chốt đánh giá: chấm artifact mới, nêu thiếu sót, so sánh cải thiện với bản sprint trước.' if req.finalizeSprint else 'Đặt câu hỏi phản biện dựa trên artifact đã nộp và deliverable còn thiếu.'}"
            )
            messages = [
                SystemMessage(content=system_prompt + json_instruction),
                HumanMessage(content=user_prompt)
            ]

            ai_msg = llm.invoke(messages)
            parsed = LLMFactory.parse_json_response(ai_msg.content)
            latency = int((time.time() - start_time) * 1000)

            summary_data = parsed.get("sprintSummary")
            summary_dto = SprintReviewSummaryDto(**summary_data) if (req.finalizeSprint and summary_data) else None

            return SprintReviewResponse(
                requestId=req.requestId,
                sessionId=req.sessionId,
                sprintNo=req.sprintNo,
                chairSpeakerName=parsed.get("chairSpeakerName", primary_chair),
                chairReply=parsed.get("chairReply", "Đã ghi nhận phần trình bày của bạn."),
                challengeQuestions=parsed.get("challengeQuestions", []),
                sprintSummary=summary_dto,
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency)
            )
        except Exception as e:
            if req.aiConnection.demoFallback:
                return self._mock_sprint_review(req, prompt_version, start_time, is_fallback=True)

            err_code, err_msg = classify_ai_error(e)
            latency = int((time.time() - start_time) * 1000)
            return SprintReviewResponse(
                requestId=req.requestId,
                sessionId=req.sessionId,
                sprintNo=req.sprintNo,
                chairSpeakerName=primary_chair,
                chairReply=f"Cuộc họp Sprint Review tạm gián đoạn do kết nối AI: {err_msg}",
                challengeQuestions=[],
                sprintSummary=None,
                promptVersion=prompt_version,
                usage=UsageStatsDto(latencyMs=latency),
                errorCode=err_code,
                error=err_msg
            )
