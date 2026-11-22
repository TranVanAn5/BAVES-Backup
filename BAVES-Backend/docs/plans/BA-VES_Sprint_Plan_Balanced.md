# BA-VES: Kế hoạch 4 Sprint cân bằng cho 2 Backend

Mục tiêu: sau 4 tuần có **1 scenario hoàn chỉnh (OmniMart)** chạy end-to-end để demo GVHD.
Ngày lập: 07/10/2026. Mỗi Sprint = 1 tuần, mỗi người khoảng **30 giờ/Sprint** (6 giờ x 5 ngày).

## 1. Nguyên tắc cân bằng

- **Dev A:** .NET core flow (Identity, Session, Planning, Analysis, Artifact, Expert).
- **Dev B:** nội dung scenario + Python AI runtime + phần .NET gắn với scenario/assessment.
- Cả hai đều viết cả .NET lẫn logic nghiệp vụ, không để một người chỉ làm API còn người kia chỉ làm prompt.
- Mỗi Sprint đều có **1 vertical slice** chạy được qua cả hai phía. Epic chỉ Done khi API .NET, SQL, AI runtime, phân quyền và integration test chạy cùng nhau.
- **Chốt contract AI trong 2 ngày đầu Sprint 1** và có AI stub (mock) để A không bị chặn khi B chưa xong.
- Đổi tên "Sprint 1/2" bên trong scenario OmniMart thành **Phase 1/2** để không nhầm với Sprint dự án.
- Hai ngày cuối Sprint 4 chỉ ổn định và rehearsal, không thêm tính năng.

## 2. Tổng quan phân bổ

| Sprint | Dev A (~30h) | Dev B (~30h) | Gate |
| --- | --- | --- | --- |
| 1 | SQL Server, roles/policy, catalogue read, Session, BA Plan | Contract + stub, scenario package + authoring/publish, Python runtime, BYOK, 1 interview | Login -> Scenario -> Context -> Session -> Plan -> hỏi 1 stakeholder -> AI trả lời |
| 2 | Async interview + transcript, Analysis + Evidence, Event state | 3 stakeholder + memory cô lập, Coach, Event effect, Confirmation AI | 3 stakeholder -> Coach -> Event -> Finding -> Evidence -> Requirement -> Confirmation |
| 3 | Artifact/Version/Submission, Assessment job + persistence, Review Request | Assessment graph, pre-check, rubric evaluation, calibration | BRD -> Freeze -> Submit -> AI Feedback có evidence -> Review Request |
| 4 | Expert Review, security/concurrency, deploy + demo DB | Failure/leakage/injection tests, golden eval, demo data + rehearsal | Expert chấm -> Final report -> demo chạy lặp lại được |

## 3. Sprint 1: Scenario Foundation và First AI Interview

**Goal:** Learner Login -> Browse Scenario -> Read Context -> Start Session -> Create BA Plan -> Interview được ít nhất 1 Stakeholder.

### Dev A (~30h): luồng learner phía .NET

| Việc | Giờ |
| --- | --- |
| API-01: chạy baseline SQL trên SQL Server thật, chạy lại 35 auth test, seed roles Learner/Instructor/Expert, policy và ownership helper | 8 |
| API-02 (read): `GET /industries`, `/applications`, `/scenarios`, `/scenario-versions/{id}/context` (DTO chỉ chứa thông tin được phép biết, không lộ hidden knowledge) | 5 |
| API-03: `POST /sessions` (pin ScenarioVersion, khởi tạo agent), `GET /sessions/{id}`, `GET /sessions/{id}/agents` | 7 |
| API-03: `GET/PUT /sessions/{id}/plan`, CRUD planned questions | 6 |
| API-04 (.NET side): `AiRuntimeClient`, endpoint hỏi stakeholder đồng bộ (smoke), lưu message | 4 |

### Dev B (~30h): scenario content, authoring và AI runtime

| Việc | Giờ |
| --- | --- |
| Chốt `StakeholderAgentRequest/Response` cùng A, dựng AI stub trả JSON cố định | 3 |
| Seed OmniMart: 3 persona, ~30 knowledge item (có mức hidden), 8 business rule, 2 event, 1 BRD template, 1 rubric | 8 |
| API-02 (write): `POST /scenarios`, `POST /scenarios/{id}/versions`, `GET/PUT /scenario-versions/{id}/definition`, `POST .../publish` (publish = đóng băng version) | 8 |
| Python FastAPI skeleton + `/health`, LangChain model adapter, BYOK key đi theo request và không log | 5 |
| Stakeholder prompt + lọc knowledge theo persona, structured response | 6 |

### Gate Sprint 1
`Login -> Scenario -> Context -> Session -> BA Plan -> Ask Stakeholder -> AI Response`, chạy với AI thật ít nhất 1 lượt.

> Nếu B bị quá tải: cắt authoring (`PUT definition`) xuống chỉ còn seed bằng script, làm đầy đủ ở Sprint 2.

## 4. Sprint 2: Multi-Agent Simulation và BA Analysis

**Goal:** Phỏng vấn đủ 3 stakeholder, nhận coach feedback, gặp business change, ghi Finding/Requirement, gắn evidence, xác nhận lại.

### Dev A (~30h)

| Việc | Giờ |
| --- | --- |
| API-04: `POST/GET /sessions/{id}/agents/{agentId}/messages`, `GET /ai/operations/{id}`, Idempotency-Key, AIOperation job state, retry, transcript paging | 10 |
| API-06: CRUD `analysis-items` (Finding, Requirement, BusinessRule, Constraint, Risk, OpenQuestion, Conflict, Assumption) | 8 |
| API-06: evidence link message -> finding -> requirement, quy tắc traceability | 6 |
| API-05 (state): `GET /events`, `POST /events/{id}/trigger` (chỉ 1 lần, chỉ Instructor), lưu SessionEvent, truyền effect sang runtime | 6 |

### Dev B (~30h)

| Việc | Giờ |
| --- | --- |
| Graph 3 stakeholder: memory riêng từng agent, không đọc memory hay hidden knowledge của agent khác | 10 |
| Coach workflow: strength / gap / unclear point / hint, cấm đưa đáp án hoặc viết thay learner; `coach-feedback` API + persistence | 8 |
| Event context overlay + agent đổi hành vi sau E1/E2 | 6 |
| Confirmation AI: Confirmed / Disputed / InsufficientInformation dựa trên evidence; `POST /analysis-items/{id}/confirmation` (A review phần persist) | 6 |

### Gate Sprint 2
`3 Stakeholders -> Coach -> Change Event -> Finding -> Evidence -> Requirement -> Confirmation`

## 5. Sprint 3: BRD, Submission và AI Assessment

**Goal:** Learner tạo BRD, freeze version, submit và nhận AI Practice Feedback có evidence.

### Dev A (~28h)

| Việc | Giờ |
| --- | --- |
| API-07: template, artifact, `PUT draft` (optimistic concurrency), `POST versions` (immutable) | 10 |
| API-07: `POST /sessions/{id}/submissions` (pin ArtifactVersion + Rubric, tạo AI operation), `GET /submissions/{id}` | 6 |
| API-08: job + persistence AI feedback, `GET /ai-feedback`, `POST /ai-feedback/retry`, `GET /report` | 8 |
| `POST /submissions/{id}/review-requests` | 4 |

### Dev B (~30h)

| Việc | Giờ |
| --- | --- |
| Assessment input builder (snapshot + plan + transcript + analysis + rubric + evaluation reference) | 6 |
| Deterministic pre-check (thiếu section, evidence không tồn tại, traceability đứt) | 6 |
| Assessment graph theo từng criterion: score gợi ý, evidence, observation, gap, suggestion, uncertainty | 10 |
| Validate output (message ID, AnalysisItem ID, section, score range), backend tính SuggestedScore thay LLM | 4 |
| Calibration trên BRD tốt / vừa / yếu | 4 |

### Gate Sprint 3
`BRD -> Freeze -> Submit -> AI Assessment -> Evidence Feedback -> Review Request`

## 6. Sprint 4: Expert Review, Hardening và Demo

**Goal:** Hoàn tất end-to-end bằng Human Review, ổn định để demo GVHD.

### Dev A (~30h)

| Việc | Giờ |
| --- | --- |
| API-08B: `GET /review-requests`, `POST /accept` (2 Expert accept cùng lúc chỉ 1 người thắng), `GET /case`, `POST /complete`, FinalScore do backend tính | 12 |
| Security: ownership test, reviewer khác không đọc được case, AI SuggestedScore không thành FinalScore | 6 |
| Concurrency và integration test chạy trên SQL Server | 6 |
| Docker/deploy, demo DB, script reset dữ liệu demo | 6 |

### Dev B (~30h)

| Việc | Giờ |
| --- | --- |
| Failure tests: sai API key, rate limit, timeout, structured output hỏng, worker restart | 8 |
| Prompt injection và knowledge leakage tests (canary hidden facts) | 8 |
| Golden questions, đo latency/token, chỉnh retrieval | 6 |
| Demo data, kịch bản demo, rehearsal 2 lần, bản fallback nếu LLM lỗi | 8 |

### Gate Sprint 4
Demo chạy lặp lại được từ Instructor publish đến Learner nhận kết quả Expert cuối.

## 7. Nếu trễ thì cắt theo thứ tự

1. Authoring đầy đủ (giữ publish bằng seed script).
2. Event E2 (giữ E1).
3. Thread Learner-Expert, SSE, PDF, cohort (đều là P1, không làm).
4. Giảm từ 3 stakeholder xuống 2, nhưng chỉ khi bắt buộc.

Không cắt: phân quyền, idempotency, tách evidence, Expert là người cho điểm cuối.

## 8. Rủi ro cần theo dõi

| Rủi ro | Cách giảm |
| --- | --- |
| SQL Server thật chưa xác nhận chạy | Làm trong ngày 1 của Sprint 1 |
| Contract AI đổi giữa chừng | Freeze trong 2 ngày đầu, đổi qua thỏa thuận hai bên |
| LLM lỗi hoặc chậm lúc demo | Có bản fallback, đã rehearsal |
| Hai bên "xong" riêng nhưng chưa gọi được nhau | Mỗi Sprint có vertical slice và integration test chung |
