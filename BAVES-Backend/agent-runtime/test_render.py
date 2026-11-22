from pathlib import Path
from app.core.agent_loader import AgentDefinitionLoader
from app.schemas.stakeholder import (
    StakeholderAgentRequest,
    ScenarioContextDto,
    StakeholderPersonaDto,
    KnowledgeItemDto,
    AgentStateDto
)

def test_render():
    base_dir = Path(__file__).resolve().parent
    agents_dir = base_dir / "app" / "agents"
    loader = AgentDefinitionLoader(agents_dir)

    req = StakeholderAgentRequest(
        requestId="test-123",
        sessionId=1,
        scenario=ScenarioContextDto(
            scenarioVersionId=1,
            companyName="OmniMart",
            businessContext="Chuỗi bán lẻ đa kênh 25 cửa hàng.",
            businessProblem="Tồn kho bị lệch giữa web và quầy.",
            assignment="Khảo sát và đề xuất BRD."
        ),
        persona=StakeholderPersonaDto(
            personaId=1,
            name="Trần Minh Quân",
            stakeholderRole="Operations Manager",
            department="Vận hành",
            responsibilities="Quản lý kho và vận chuyển.",
            goals="Giảm đơn trùng.",
            painPoints="Thao tác đối soát thủ công.",
            businessConcerns="Chi phí tăng.",
            communicationStyle="Thẳng thắn, ngắn gọn.",
            personality="Thực tế, khó tính."
        ),
        allowedKnowledge=[
            KnowledgeItemDto(
                knowledgeItemId=101,
                code="K-OPS-01",
                title="Giữ tồn kho",
                content="Chỉ giữ tồn kho sau khi thanh toán thành công.",
                category="Process",
                importance="High",
                accessType="Knows",
                isHidden=False
            )
        ],
        agentState=AgentStateDto(
            memorySummary="Chưa có trao đổi nào trước đây.",
            trustLevel=40,
            patienceLevel=50,
            cooperationLevel=50
        ),
        learnerMessage="Chào anh, hiện tại đơn online được giữ hàng vào lúc nào ạ?"
    )

    prompt, version = loader.render_prompt("stakeholder", req.model_dump())
    print("=== RENDERED PROMPT ===")
    print(prompt[:400] + "...\n(truncated)")
    print(f"=== VERSION: {version} ===")
    assert "Trần Minh Quân" in prompt
    assert "K-OPS-01" in prompt
    assert "Quy tắc bắt buộc" in prompt # từ guardrails
    print("SUCCESS: Template loaded & rendered successfully with guardrails!")

if __name__ == "__main__":
    test_render()
