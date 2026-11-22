import json
from fastapi.testclient import TestClient
from app.main import app

client = TestClient(app)
TOKEN = "baves-internal-secret-token"
HEADERS = {"X-Internal-Token": TOKEN}

def test_health():
    res = client.get("/health")
    assert res.status_code == 200
    assert res.json()["status"] == "healthy"
    print("PASS: /health")

def test_validate_connection():
    res = client.post(
        "/v1/connections/validate",
        headers=HEADERS,
        json={"provider": "OpenAI", "apiKey": "sk-mock-key"}
    )
    assert res.status_code == 200
    assert res.json()["valid"] is True
    print("PASS: /v1/connections/validate")

def test_stakeholder_mock_response():
    payload = {
        "requestId": "req-test-01",
        "sessionId": 10,
        "scenario": {
            "scenarioVersionId": 1,
            "companyName": "OmniMart",
            "businessContext": "Bán lẻ đa kênh 25 cửa hàng.",
            "businessProblem": "Lệch tồn kho và đơn trùng.",
            "assignment": "Khảo sát và đề xuất BRD."
        },
        "persona": {
            "personaId": 1,
            "name": "Trần Minh Quân",
            "stakeholderRole": "Operations Manager",
            "department": "Vận hành",
            "responsibilities": "Quản lý kho",
            "goals": "Giảm đơn trùng",
            "painPoints": "Đối soát thủ công",
            "businessConcerns": "Chi phí tăng",
            "communicationStyle": "Ngắn gọn, khó tính",
            "personality": "Thực tế"
        },
        "allowedKnowledge": [
            {
                "knowledgeItemId": 1,
                "code": "K-OPS-01",
                "title": "Thời điểm giữ tồn kho",
                "content": "Chỉ giữ hàng sau khi thanh toán thành công.",
                "category": "Process",
                "importance": "High",
                "accessType": "Knows",
                "isHidden": False
            }
        ],
        "agentState": {
            "memorySummary": "Mới bắt đầu phỏng vấn.",
            "trustLevel": 40,
            "patienceLevel": 50,
            "cooperationLevel": 50
        },
        "history": [
            {"senderType": "Learner", "content": "Em chào anh Quân."},
            {"senderType": "Stakeholder", "content": "Chào em, có việc gì cần trao đổi nhanh không?"}
        ],
        "learnerMessage": "Hiện tại đơn online được giữ hàng thế nào anh?",
        "eventContext": [
            {
                "title": "Sự cố đồng bộ POS",
                "description": "Hệ thống POS tại cửa hàng bị delay đồng bộ tồn kho sang online."
            }
        ],
        "aiConnection": {
            "provider": "Mock"
        }
    }

    res = client.post("/v1/stakeholder/respond", headers=HEADERS, json=payload)
    assert res.status_code == 200
    data = res.json()
    assert data["requestId"] == "req-test-01"
    assert "Trần Minh Quân" in data["reply"]
    assert "Sự cố đồng bộ POS" in data["reply"]
    assert 1 in data["usedKnowledgeItemIds"]
    assert len(data["stateUpdate"]["memorySummary"]) > 0
    assert data["promptVersion"] == "stakeholder@1.0.0"
    print("PASS: /v1/stakeholder/respond (Mock mode with History & Events)")

def test_coach_feedback():
    payload = {
        "sessionId": 10,
        "baPlan": {
            "objective": "Khảo sát quy trình giữ kho OmniMart",
            "approach": "Phỏng vấn từng bước"
        },
        "recentMessages": [
            {"senderType": "Learner", "content": "Chào anh Quân, hiện tại đơn online được giữ hàng thế nào?"},
            {"senderType": "Stakeholder", "content": "Chúng tôi giữ hàng sau khi thanh toán."}
        ],
        "aiConnection": {"provider": "Mock"}
    }
    res = client.post("/v1/coach/feedback", headers=HEADERS, json=payload)
    assert res.status_code == 200
    data = res.json()
    assert data["sessionId"] == 10
    assert len(data["strengths"]) > 0
    assert len(data["gaps"]) > 0
    assert len(data["improvementHints"]) > 0
    print("PASS: /v1/coach/feedback (Stub mode)")

def test_confirm_analysis_item():
    payload = {
        "persona": {
            "personaId": 1,
            "name": "Trần Minh Quân",
            "stakeholderRole": "Operations Manager"
        },
        "item": {
            "itemId": 101,
            "itemType": "Requirement",
            "title": "Hệ thống phải tự động giữ hàng trong kho sau khi đặt",
            "description": "Giữ tồn kho tối đa 30 phút đối với đơn COD."
        },
        "evidenceContent": "Anh Quân chia sẻ: Chúng tôi giữ hàng trong kho cho đến khi giao xong.",
        "allowedKnowledge": [],
        "aiConnection": {"provider": "Mock"}
    }
    res = client.post("/v1/analysis/confirm", headers=HEADERS, json=payload)
    assert res.status_code == 200
    data = res.json()
    assert data["outcome"] == "Confirmed"
    assert "Trần Minh Quân" in data["reasoning"]
    print("PASS: /v1/analysis/confirm (Stub mode)")

def test_all_three_stakeholders():
    # 1. Test IT Lead (Nguyễn Quốc Tuấn)
    payload_it = {
        "requestId": "req-it-01",
        "sessionId": 10,
        "scenario": {
            "scenarioVersionId": 1,
            "companyName": "OmniMart",
            "businessContext": "Bán lẻ đa kênh.",
            "businessProblem": "Lệch tồn kho.",
            "assignment": "Khảo sát hệ thống POS."
        },
        "persona": {
            "personaId": 2,
            "name": "Nguyễn Quốc Tuấn",
            "stakeholderRole": "IT Lead",
            "department": "Công nghệ thông tin",
            "responsibilities": "Hệ thống POS và ERP"
        },
        "allowedKnowledge": [
            {
                "knowledgeItemId": 2,
                "code": "K-IT-01",
                "title": "Cơ chế đồng bộ POS",
                "content": "POS đồng bộ định kỳ 30 phút.",
                "category": "System",
                "importance": "Critical",
                "accessType": "Knows",
                "isHidden": False
            }
        ],
        "agentState": {"trustLevel": 50, "patienceLevel": 60, "cooperationLevel": 50},
        "history": [],
        "learnerMessage": "Anh Tuấn cho em hỏi chu kỳ đồng bộ POS bao lâu?",
        "eventContext": [],
        "aiConnection": {"provider": "Mock"}
    }
    res_it = client.post("/v1/stakeholder/respond", headers=HEADERS, json=payload_it)
    assert res_it.status_code == 200
    assert "Nguyễn Quốc Tuấn" in res_it.json()["reply"]
    print("PASS: /v1/stakeholder/respond (IT Lead Persona)")

    # 2. Test Sales Lead (Lê Hoàng Yến)
    payload_sal = {
        "requestId": "req-sal-01",
        "sessionId": 10,
        "scenario": {
            "scenarioVersionId": 1,
            "companyName": "OmniMart",
            "businessContext": "Bán lẻ đa kênh.",
            "businessProblem": "Trải nghiệm giao trễ.",
            "assignment": "Cải thiện chính sách giao 2h."
        },
        "persona": {
            "personaId": 3,
            "name": "Lê Hoàng Yến",
            "stakeholderRole": "Sales Lead",
            "department": "Kinh doanh & Bán lẻ",
            "responsibilities": "Chăm sóc khách hàng"
        },
        "allowedKnowledge": [
            {
                "knowledgeItemId": 3,
                "code": "K-SAL-01",
                "title": "Chính sách voucher 50k",
                "content": "Đền bù voucher 50k khi hủy đơn sai.",
                "category": "Policy",
                "importance": "Medium",
                "accessType": "Knows",
                "isHidden": False
            }
        ],
        "agentState": {"trustLevel": 50, "patienceLevel": 50, "cooperationLevel": 50},
        "history": [],
        "learnerMessage": "Chị Yến ơi khách phàn nàn việc hủy đơn nhiều không chị?",
        "eventContext": [],
        "aiConnection": {"provider": "Mock"}
    }
    res_sal = client.post("/v1/stakeholder/respond", headers=HEADERS, json=payload_sal)
    assert res_sal.status_code == 200
    assert "Lê Hoàng Yến" in res_sal.json()["reply"]
    print("PASS: /v1/stakeholder/respond (Sales Lead Persona)")

def test_confirm_analysis_disputed_and_insufficient():
    # Test Insufficient
    payload_insufficient = {
        "persona": {"personaId": 1, "name": "Trần Minh Quân", "stakeholderRole": "Operations Manager"},
        "item": {"itemType": "Finding", "title": "Kho hàng", "description": "Mô tả ngắn"},
        "evidenceContent": "ngắn",
        "allowedKnowledge": [],
        "aiConnection": {"provider": "Mock"}
    }
    res_ins = client.post("/v1/analysis/confirm", headers=HEADERS, json=payload_insufficient)
    assert res_ins.status_code == 200
    assert res_ins.json()["outcome"] == "InsufficientInformation"
    print("PASS: /v1/analysis/confirm (InsufficientInformation state)")

    # Test Disputed
    payload_disputed = {
        "persona": {"personaId": 1, "name": "Trần Minh Quân", "stakeholderRole": "Operations Manager"},
        "item": {"itemType": "Requirement", "title": "Bán hàng không giới hạn", "description": "Không cần kiểm tra số lượng"},
        "evidenceContent": "Đoạn phỏng vấn về nhân sự và chấm công ngày nghỉ phép của công ty",
        "allowedKnowledge": [],
        "aiConnection": {"provider": "Mock"}
    }
    res_disp = client.post("/v1/analysis/confirm", headers=HEADERS, json=payload_disputed)
    assert res_disp.status_code == 200
    assert res_disp.json()["outcome"] == "Disputed"
    print("PASS: /v1/analysis/confirm (Disputed state)")

def test_assessment_evaluate():
    payload = {
        "submissionId": 101,
        "artifact": {
            "artifactId": 1,
            "title": "BRD - He Thong Quan Ly Don Hang OmniMart",
            "versionNo": 1,
            "sections": [
                {
                    "sectionKey": "scope",
                    "title": "Pham vi du an",
                    "content": "Tap trung cai thien quy trinh xu ly don hang POS va giu hang kho."
                },
                {
                    "sectionKey": "functional_requirements",
                    "title": "Yeu cau chuc nang",
                    "content": "He thong phai tu dong hoan tien trong 48h va doi soat COD theo quy dinh."
                }
            ]
        },
        "rubric": {
            "rubricId": 1,
            "title": "Rubric Danh Gia BRD OmniMart",
            "criteria": [
                {"code": "COMPLETENESS", "name": "Tinh day du", "weight": 1.0, "maxScore": 100.0},
                {"code": "FEASIBILITY", "name": "Tinh kha thi", "weight": 1.0, "maxScore": 100.0}
            ]
        },
        "evidenceList": [
            {
                "evidenceId": 1,
                "messageContent": "Giu hang trong kho sau khi thanh toan",
                "stakeholderName": "Tran Minh Quan",
                "linkedItemTitle": "Quy dinh giu kho"
            }
        ],
        "expectedKnowledgeIds": [1, 2, 3],
        "aiConnection": {"provider": "Mock"}
    }

    res = client.post("/v1/assessment/evaluate", headers=HEADERS, json=payload)
    assert res.status_code == 200
    data = res.json()
    assert data["submissionId"] == 101
    assert data["overallScore"] > 0
    assert len(data["criterionScores"]) == 2
    assert len(data["strengths"]) > 0
    print("PASS: /v1/assessment/evaluate (Stub mode)")

def test_assessment_evaluate_with_contradiction_error():
    # Bad submission that violates BR-03 (COD limit)
    payload_bad = {
        "submissionId": 102,
        "artifact": {
            "artifactId": 2,
            "title": "BRD - Vi pham Business Rule",
            "versionNo": 1,
            "sections": [
                {
                    "sectionKey": "payments",
                    "title": "Chinh sach thanh toan",
                    "content": "Cho phep khach hang thanh toan COD tren 5 trieu dong de kich cau mua sam."
                }
            ]
        },
        "rubric": {
            "rubricId": 1,
            "title": "Rubric Danh Gia BRD OmniMart",
            "criteria": [
                {"code": "COMPLETENESS", "name": "Tinh day du", "weight": 1.0, "maxScore": 100.0},
                {"code": "FEASIBILITY", "name": "Tinh kha thi", "weight": 1.0, "maxScore": 100.0}
            ]
        },
        "evidenceList": [],
        "expectedKnowledgeIds": [1, 2],
        "aiConnection": {"provider": "Mock"}
    }

    res = client.post("/v1/assessment/evaluate", headers=HEADERS, json=payload_bad)
    assert res.status_code == 200
    data = res.json()
    assert data["submissionId"] == 102
    assert data["passed"] is False
    assert len(data["criticalErrors"]) > 0
    assert any("BR-03" in err for err in data["criticalErrors"])

    # Verify Feasibility score was penalized
    feasibility_score = next(c for c in data["criterionScores"] if c["criterionCode"] == "FEASIBILITY")
    assert feasibility_score["score"] < 70.0
    print("PASS: /v1/assessment/evaluate (Contradiction detected, critical error flagged)")

def test_stakeholder_preserves_memory_on_ai_error():
    previous_memory = "Đã làm rõ quy trình giữ kho 30 phút cho đơn COD và giới hạn 3 triệu đồng."
    payload = {
        "requestId": "req-err-01",
        "sessionId": 10,
        "scenario": {
            "scenarioVersionId": 1,
            "companyName": "OmniMart",
            "businessContext": "Bán lẻ đa kênh.",
            "businessProblem": "Lệch tồn kho.",
            "assignment": "Khảo sát BRD."
        },
        "persona": {
            "personaId": 1,
            "name": "Trần Minh Quân",
            "stakeholderRole": "Operations Manager",
            "department": "Vận hành"
        },
        "allowedKnowledge": [],
        "agentState": {
            "memorySummary": previous_memory,
            "trustLevel": 65,
            "patienceLevel": 55,
            "cooperationLevel": 70
        },
        "history": [],
        "learnerMessage": "Anh cho em hỏi thêm về quy trình hoàn tiền?",
        "eventContext": [],
        "aiConnection": {
            "provider": "Anthropic",
            "apiKey": "sk-expired-or-out-of-quota-key"
        }
    }

    res = client.post("/v1/stakeholder/respond", headers=HEADERS, json=payload)
    assert res.status_code == 200
    data = res.json()
    assert data["error"] is not None
    assert data["stateUpdate"]["memorySummary"] == previous_memory
    assert data["stateUpdate"]["trustLevel"] == 65
    assert data["stateUpdate"]["patienceLevel"] == 55
    assert data["stateUpdate"]["cooperationLevel"] == 70
    assert data["usedKnowledgeItemIds"] == []
    print("PASS: /v1/stakeholder/respond (Preserves previous memorySummary & state on AI error)")

def test_sprint_review_meeting_discussion_and_finalize():
    # Phase 1: BA bấm "Triệu tập họp Sprint 3 Review", Hội đồng Chủ trì đặt câu hỏi chất vấn
    payload_discussion = {
        "requestId": "sprint-rev-s3-qna",
        "sessionId": 10,
        "sprintNo": 3,
        "totalSprints": 4,
        "chairPersonas": [
            {
                "personaId": 2,
                "name": "Nguyễn Quốc Tuấn",
                "stakeholderRole": "IT Lead",
                "department": "Công nghệ thông tin"
            },
            {
                "personaId": 1,
                "name": "Trần Minh Quân",
                "stakeholderRole": "Operations Manager",
                "department": "Vận hành"
            }
        ],
        "artifacts": [
            {
                "artifactType": "BRD",
                "title": "BRD OmniMart Cập nhật sau E1",
                "versionNo": 2,
                "sections": [
                    {"sectionKey": "constraints", "title": "Ràng buộc", "content": "Đồng bộ POS có thể trễ tối đa 45 phút khi đứt cáp."}
                ]
            },
            {
                "artifactType": "SRS",
                "title": "SRS Đặc tả Đồng bộ & Giữ kho",
                "versionNo": 1,
                "sections": [
                    {"sectionKey": "fr_sync", "title": "Chế độ Offline POS", "content": "Khi mất mạng, giữ safety stock buffer và đối soát COD tối đa 3 triệu."}
                ]
            }
        ],
        "eventContext": [
            {"title": "Event E1: Trễ đồng bộ POS 45 phút", "description": "Đứt cáp miền Trung"}
        ],
        "finalizeSprint": False,
        "aiConnection": {"provider": "Mock"}
    }
    res_qna = client.post("/v1/stakeholder/sprint-review", headers=HEADERS, json=payload_discussion)
    assert res_qna.status_code == 200
    data_qna = res_qna.json()
    assert data_qna["sprintNo"] == 3
    assert len(data_qna["challengeQuestions"]) > 0
    assert data_qna["sprintSummary"] is None

    # Phase 2: BA giải trình xong, Chủ trì chốt Biên bản Kết luận Sprint 3 & tạo compressedDigest
    payload_finalize = {
        **payload_discussion,
        "requestId": "sprint-rev-s3-final",
        "learnerReply": "Em đã bổ sung Safety Stock Buffer (BR-02) và cơ chế hàng đợi Offline cho POS ạ.",
        "finalizeSprint": True
    }
    res_fin = client.post("/v1/stakeholder/sprint-review", headers=HEADERS, json=payload_finalize)
    assert res_fin.status_code == 200
    data_fin = res_fin.json()
    assert data_fin["sprintSummary"] is not None
    assert data_fin["sprintSummary"]["decision"] == "ApprovedWithRevisions"
    assert data_fin["sprintSummary"]["nextSprintUnlocked"] is True
    assert len(data_fin["sprintSummary"]["compressedDigest"]) > 20
    print("PASS: /v1/stakeholder/sprint-review (Interactive Q&A + Finalize Sprint Digest)")

def test_assessment_map_reduce_project_final():
    payload_final = {
        "submissionId": 200,
        "sprintSummaries": [
            {
                "sprintNo": 1,
                "decision": "Approved",
                "sprintScore": 88.0,
                "compressedDigest": "Sprint 1: BAP v1 và Stakeholder Register v1 đầy đủ mục tiêu và phân nhóm."
            },
            {
                "sprintNo": 2,
                "decision": "ApprovedWithRevisions",
                "sprintScore": 84.0,
                "compressedDigest": "Sprint 2: BRD v1 và Business Rules v1 làm rõ giữ kho và trần COD 3 triệu."
            },
            {
                "sprintNo": 3,
                "decision": "ApprovedWithRevisions",
                "sprintScore": 86.0,
                "compressedDigest": "Sprint 3: BRD v2, SRS v1, UserStories v1 xử lý tốt sự cố trễ mạng POS E1."
            },
            {
                "sprintNo": 4,
                "decision": "Approved",
                "sprintScore": 90.0,
                "compressedDigest": "Sprint 4: ImpactAnalysis v1, SRS v2, RTM v1 đáp ứng tải Flash Sale E2 300 đơn/phút."
            }
        ],
        "processMetrics": {
            "revealedKnowledgeCount": 26,
            "totalKnowledgeCount": 30,
            "confirmedRequirementsCount": 8,
            "disputedRequirementsCount": 0,
            "avgStakeholderTrust": 78.5,
            "sprintsCompleted": 4
        },
        "rubric": {
            "rubricId": 1,
            "title": "Rubric Tổng kết Dự án OmniMart",
            "criteria": [
                {"code": "COMPLETENESS", "name": "Tính đầy đủ", "weight": 0.5, "maxScore": 100.0},
                {"code": "FEASIBILITY", "name": "Tính khả thi", "weight": 0.5, "maxScore": 100.0}
            ]
        },
        "aiConnection": {"provider": "Mock"}
    }

    res = client.post("/v1/assessment/evaluate", headers=HEADERS, json=payload_final)
    assert res.status_code == 200
    data = res.json()
    assert data["submissionId"] == 200
    assert data["evaluationMode"] == "MapReduceProjectFinal"
    assert data["expertReviewAvailable"] is True
    assert data["passed"] is True
    assert data["overallScore"] >= 85.0
    print("PASS: /v1/assessment/evaluate (Map-Reduce Final Project Evaluation + Optional Expert Flag)")

if __name__ == "__main__":
    test_health()
    test_validate_connection()
    test_stakeholder_mock_response()
    test_all_three_stakeholders()
    test_stakeholder_preserves_memory_on_ai_error()
    test_sprint_review_meeting_discussion_and_finalize()
    test_assessment_map_reduce_project_final()
    test_coach_feedback()
    test_confirm_analysis_item()
    test_confirm_analysis_disputed_and_insufficient()
    test_assessment_evaluate()
    test_assessment_evaluate_with_contradiction_error()
    print("ALL INTEGRATION TESTS PASSED 100%!")
