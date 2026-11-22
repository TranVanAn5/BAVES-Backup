import re
from typing import List, Tuple, Optional
from app.schemas.stakeholder import KnowledgeItemDto, ChatMessageDto

INJECTION_PATTERNS = [
    re.compile(r"ignore\s+(all\s+)?(previous|prior|above)\s+instructions", re.IGNORECASE),
    re.compile(r"bỏ\s+qua\s+(tất\s+cả\s+|mọi\s+)?(hướng\s+dẫn|quy\s+tắc|lệnh|ràng\s+buộc|lỗi)", re.IGNORECASE),
    re.compile(r"(system\s+prompt|system\s+instruction|developer\s+mode|jailbreak|dan\s+mode)", re.IGNORECASE),
    re.compile(r"(tiết\s+lộ|in\s+ra|liệt\s+kê|show\s+me|dump)\s+(toàn\s+bộ\s+|tất\s+cả\s+)?(hidden|bí\s+mật|canary|đáp\s+án|answer\s+key|allowedknowledge)", re.IGNORECASE),
    re.compile(r"(đóng\s+vai|giả\s+lập\s+làm|chuyển\s+sang\s+vai)\s+(nhân\s+vật\s+khác|giám\s+khảo|assessor)", re.IGNORECASE),
    re.compile(r"(viết\s+hộ|soạn\s+hộ|làm\s+thay)\s+(tôi\s+|em\s+)?(toàn\s+bộ\s+)?(brd|srs|bap|tài\s+liệu)", re.IGNORECASE),
    re.compile(r"(chấm\s+|cho\s+|duyệt\s+)(bài\s+này\s+|sprint\s+này\s+)?(100\s*điểm|100/100|điểm\s+tối\s+đa|approved\s+ngay)", re.IGNORECASE),
]

STOPWORDS = {
    "là", "và", "của", "có", "cho", "trong", "với", "được", "các", "những",
    "một", "này", "khi", "thì", "như", "thế", "nào", "anh", "chị", "em", "hỏi",
    "về", "việc", "hiện", "tại", "bên", "mình", "công", "ty", "omnimart"
}

class SecurityGuard:
    """Lớp bảo vệ chống Prompt Injection và kiểm soát rò rỉ kiến thức ẩn (Canary / Hidden Knowledge Isolation)."""

    @staticmethod
    def detect_prompt_injection(text: Optional[str]) -> Tuple[bool, Optional[str]]:
        if not text:
            return False, None
        for pattern in INJECTION_PATTERNS:
            match = pattern.search(text)
            if match:
                return True, f"Phát hiện mẫu câu lệnh vượt quyền / Prompt Injection: '{match.group(0)}'"
        return False, None

    @staticmethod
    def filter_disclosable_knowledge(
        allowed_knowledge: List[KnowledgeItemDto],
        learner_message: str,
        history: List[ChatMessageDto],
        trust_level: int = 50
    ) -> List[KnowledgeItemDto]:
        """
        Thực thi chính sách bảo mật tri thức ở tầng Code trước khi đưa vào LLM:
        1. Loại bỏ tuyệt đối mọi thẻ có accessType == 'DoesNotKnow'.
        2. Với thẻ có isHidden == True (bao gồm Canary facts):
           - Nếu câu hỏi của Learner là câu hỏi chung chung ("kể hết mọi thứ anh biết", "có gì mới không")
             hoặc không chứa từ khóa liên quan tới chủ đề của thẻ ẩn/disclosureRule -> Giữ ẩn (không nạp vào context).
           - Chỉ mở khóa khi câu hỏi của Learner chạm đúng từ khóa chuyên môn của thẻ đó và trust_level >= 30.
        """
        is_injected, _ = SecurityGuard.detect_prompt_injection(learner_message)
        if is_injected:
            return []

        msg_lower = (learner_message or "").lower()
        history_text = " ".join([m.content.lower() for m in history[-4:]]) if history else ""
        combined_inquiry = f"{msg_lower} {history_text}"

        disclosable: List[KnowledgeItemDto] = []
        for item in allowed_knowledge:
            if item.accessType == "DoesNotKnow":
                continue

            if not item.isHidden:
                disclosable.append(item)
                continue

            # Kiểm tra thẻ ẩn (Hidden / Canary Knowledge Gate)
            if trust_level < 30:
                continue

            # Nếu thẻ là Canary bảo mật (code bắt đầu bằng CANARY) -> Không bao giờ tự mở trừ khi có mã xác thực đặc biệt
            if item.code.upper().startswith("CANARY"):
                continue

            # Trích xuất từ khóa có nghĩa từ title, category và disclosureRule
            ref_text = f"{item.title} {item.disclosureRule or ''} {item.content[:80]}".lower()
            tokens = [
                tok.strip(".,;:!?()[]\"'")
                for tok in ref_text.split()
                if len(tok.strip(".,;:!?()[]\"'")) >= 3 and tok.strip(".,;:!?()[]\"'") not in STOPWORDS
            ]

            matched_keywords = [tok for tok in set(tokens) if tok in combined_inquiry]
            if len(matched_keywords) >= 2:
                disclosable.append(item)

        return disclosable
