# BA-VES Sprint 1: Chi tiết theo Dev và Endpoint

**Sprint Goal:** Learner `Login -> Browse Scenario -> Read Context -> Start Session -> Create BA Plan -> Interview 1 Stakeholder -> nhận AI Response`.

- Thời lượng: 5 ngày làm việc, khoảng 30 giờ/dev.
- **Dev A:** luồng learner phía .NET.
- **Dev B:** scenario authoring + seed OmniMart + AI runtime (Python).
- Căn cứ source hiện tại: entity `Scenario`, `ScenarioVersion`, `StakeholderPersona`, `KnowledgeItem`, `SimulationSession`, `SessionAgent`, `Message`, `BAPlan`, `PlannedQuestion` đã có; Identity (`/api/auth/*`, `/api/users/me`) đã có. **Chưa có controller/service/repository nào** cho Content, Scenario, Simulation, Planning.

---

## 0. Quy ước chung (cả hai dev thống nhất ngày 1)

### 0.1 Cấu trúc module (theo README)
```text
Modules/<Module>/
  Controllers/  Services/(+Interfaces)  Repositories/(+Interfaces)
  Models/DTOs  Models/Requests  Models/Entities(đã có)
```
Mẫu tham chiếu: `Modules/Identity` (`AuthController`, `AuthService`, `AuthRepository`).
Đăng ký DI cho service/repository trong `DependencyInjection.cs` hoặc extension riêng mỗi module.

### 0.2 Roles và policy
| Role | Quyền |
| --- | --- |
| `Learner` | Xem scenario published, tạo và dùng session của chính mình |
| `Instructor` | Tạo/sửa/publish scenario của chính mình, xem scenario published |
| `Expert` | Chưa dùng ở Sprint 1 |
| `Admin` | Mọi thứ trong Sprint 1 |

Policy cần có: `LearnerOnly`, `InstructorOrAdmin`, `AnyAuthenticated`. Mọi controller nghiệp vụ gắn `[Authorize(Policy = ...)]`.

### 0.3 Lỗi và phân trang
- Dùng sẵn `NotFoundException`, `ConflictException`, `RequestValidationException`, `GlobalExceptionHandler` -> ProblemDetails.
- Resource của người khác trả **404** (không lộ sự tồn tại), thiếu quyền theo role trả **403**.
- List endpoint: `?page=1&pageSize=20`, trả `{ items, page, pageSize, total }`.

### 0.4 Quy tắc không lộ dữ liệu
DTO dành cho Learner **tuyệt đối không** có: `Goals`, `PainPoints`, `BusinessConcerns`, `Personality`, `KnowledgeItem.Content`, `BusinessRules`, `ScenarioEvents`, `StakeholderKnowledgeAccess`. Các trường này chỉ trả cho Instructor/Admin chủ scenario, và cho AI runtime (qua .NET, đã lọc).

### 0.5 BYOK
API key do desktop gửi kèm request qua header `X-AI-Provider`, `X-AI-Model`, `X-AI-Key`. Backend .NET chỉ chuyển tiếp cho runtime, **không lưu DB, không log**. Cần middleware/logging redact 3 header này.

---

## 1. Contract A <-> B (chốt trong 2 ngày đầu, trước khi code sâu)

### 1.1 .NET gọi Python runtime
`POST {AiRuntime:BaseUrl}/v1/stakeholder/respond` (header `X-Internal-Token` chung giữa hai service)

**StakeholderAgentRequest**
```json
{
  "requestId": "guid",
  "sessionId": 12,
  "scenario": {
    "scenarioVersionId": 3,
    "businessContext": "...",
    "businessProblem": "...",
    "assignment": "..."
  },
  "persona": {
    "personaId": 7,
    "name": "Operations Manager",
    "stakeholderRole": "Operations",
    "department": "Ops",
    "communicationStyle": "...",
    "personality": "...",
    "goals": "...",
    "painPoints": "...",
    "businessConcerns": "..."
  },
  "allowedKnowledge": [
    {
      "knowledgeItemId": 21,
      "code": "K-OPS-01",
      "title": "...",
      "content": "...",
      "category": "Process",
      "importance": "High",
      "accessType": "Knows",
      "isHidden": false,
      "disclosureRule": null
    }
  ],
  "agentState": { "memorySummary": "...", "trustLevel": 50, "patienceLevel": 60, "cooperationLevel": 50 },
  "history": [ { "senderType": "Learner|Stakeholder", "content": "...", "createdAt": "ISO8601" } ],
  "learnerMessage": "string",
  "eventContext": [],
  "aiConnection": { "provider": "OpenAI|Gemini|Anthropic|Mock", "model": "...", "apiKey": "..." }
}
```
**StakeholderAgentResponse**
```json
{
  "requestId": "guid",
  "reply": "string",
  "usedKnowledgeItemIds": [21],
  "stateUpdate": { "memorySummary": "...", "trustLevel": 52, "patienceLevel": 58, "cooperationLevel": 50 },
  "promptVersion": "stakeholder@1.0.0",
  "usage": { "promptTokens": 0, "completionTokens": 0, "latencyMs": 0 },
  "error": null
}
```
Lỗi runtime: HTTP 4xx/5xx kèm `{ "code": "InvalidApiKey|RateLimited|Timeout|InvalidOutput", "message": "..." }`.

**Quy tắc:** .NET chỉ gửi `allowedKnowledge` đã lọc theo `StakeholderKnowledgeAccess` của persona đó (bao gồm cờ `accessType`, `isHidden`, `disclosureRule` từ DB để prompt AI biết cách phản ứng đúng). Runtime không tự truy cập DB. Runtime phải kiểm tra `usedKnowledgeItemIds` là tập con của `allowedKnowledge`.

### 1.2 AI stub (B giao cho A trong ngày 2)
`POST /v1/stakeholder/respond` trả cố định `reply: "[stub] ..."` để A test sớm, không phụ thuộc LLM.

---

## 2. DEV A: Endpoint và công việc (~30h)

### A1. API-01 Identity và Authorization (8h)
Endpoint đã có, không viết lại: `POST /api/auth/register|login|refresh|logout`, `GET /api/users/me`.

| Việc | Chi tiết |
| --- | --- |
| A1.1 SQL Server thật (3h) | `docker compose up`, chạy `database/BA-VES-MVP.Initial.sql`, đặt connection string `DefaultConnection`, kiểm tra `GET /health` và đăng ký/đăng nhập thật |
| A1.2 Auth test (1h) | Chạy lại 35 test trong `BAVES.Auth.Tests`; thêm 1 test smoke chạy trên SQL Server nếu được |
| A1.3 Seed roles (1h) | Seed `Learner`, `Instructor`, `Expert`, `Admin` (idempotent) + user demo (Learner, Instructor, Expert) cho môi trường Development |
| A1.4 Policy + helper (3h) | Đăng ký policy ở 0.2; `ICurrentUser` (lấy `UserId`, role từ claims); `OwnershipGuard` dùng chung (`EnsureSessionOwner`) |

Role mặc định khi register là Learner. Instructor/Expert do seed hoặc admin cấp (không có endpoint tự nâng quyền).

### A2. API-02 Scenario Catalogue, phần read (5h)
Module: **Content** (industry/application) và **Scenario** (scenario/version). Đều `[Authorize]`.

| # | Endpoint | Role | Mô tả |
| --- | --- | --- | --- |
| 1 | `GET /api/industries` | Any | Industry đang Active, sắp xếp theo tên |
| 2 | `GET /api/applications?industryId=` | Any | Application Active, lọc theo industry |
| 3 | `GET /api/scenarios?applicationId=&page=&pageSize=` | Any | Chỉ scenario `Published`; mỗi item gồm `id, code, title, description, difficulty, latestPublishedVersionId` |
| 4 | `GET /api/scenario-versions/{id}/context` | Any | Chỉ khi version `IsPublished`. Trả DTO an toàn: `businessContext, businessProblem, businessObjectives, baRole, assignment, inScope, outOfScope, knownConstraints, difficulty` + danh sách stakeholder công khai (`personaId, name, stakeholderRole, department, responsibilities`) |

Response mẫu `GET /api/scenario-versions/{id}/context`:
```json
{
  "scenarioVersionId": 3, "scenarioId": 1, "title": "OmniMart ...", "versionNo": 1,
  "businessContext": "...", "businessProblem": "...", "businessObjectives": "...",
  "baRole": "...", "assignment": "...", "inScope": "...", "outOfScope": "...", "knownConstraints": "...",
  "stakeholders": [ { "personaId": 7, "name": "...", "stakeholderRole": "Operations", "department": "..." } ]
}
```
Lỗi: version không tồn tại hoặc chưa publish -> 404.

### A3. API-03 Session và BA Plan (13h)
Module: **Simulation** và **Planning**. Role `Learner` (Admin được xem).

| # | Endpoint | Mô tả |
| --- | --- | --- |
| 5 | `POST /api/sessions` | Body `{ "scenarioVersionId": 3 }`. Kiểm tra version published. Tạo `SimulationSession` (`LearnerId` = user hiện tại, `AttemptNo` = số lần cũ + 1, `Status = Active`), tạo `SessionAgent` cho **mỗi persona `IsActive`** trong version. Trả 201 + session + agents |
| 6 | `GET /api/sessions/{id}` | Chỉ chủ session. Trả `status, startedAt, scenarioVersionId, attemptNo` |
| 7 | `GET /api/sessions/{id}/agents` | Danh sách agent: `sessionAgentId, personaId, name, stakeholderRole, department, lastInteractionAt, isActive`. Không trả `MemorySummary`, trust/patience/cooperation |
| 8 | `GET /api/sessions/{id}/plan` | Trả plan + questions; 404 nếu chưa có plan (hoặc trả plan rỗng, chọn một và ghi vào Swagger) |
| 9 | `PUT /api/sessions/{id}/plan` | Upsert `Objective` (bắt buộc), `Approach`, `Status` (Draft/Ready). Chỉ khi session `Active` |
| 10 | `POST /api/sessions/{id}/plan/questions` | `{ stakeholderPersonaId?, topic?, informationNeed?, question, priority }`. `stakeholderPersonaId` phải thuộc cùng scenario version |
| 11 | `PATCH /api/planned-questions/{id}` | Sửa các trường trên + `status`; kiểm tra ownership qua plan -> session |
| 12 | `DELETE /api/planned-questions/{id}` | Xóa; 204 |

Quy tắc:
- Một session chỉ có một BAPlan. `PUT` lần đầu tạo mới.
- `DisplayOrder` tự gán khi thêm câu hỏi.
- Mọi endpoint dưới `/sessions/{id}` kiểm tra `LearnerId == currentUser`.

Phụ (nếu còn giờ): `GET /api/sessions?page=` liệt kê session của mình để Resume.

### A4. API-04 phía .NET, interview smoke (4h)
| # | Endpoint | Mô tả |
| --- | --- | --- |
| 13 | `POST /api/sessions/{sessionId}/agents/{agentId}/messages` | **Phiên bản đồng bộ cho Sprint 1** (Sprint 2 đổi thành async + Idempotency-Key). Body `{ "content": "..." }` |
| 14 | `GET /api/sessions/{sessionId}/agents/{agentId}/messages?page=` | Transcript của agent đó |

Luồng của endpoint 13:
```text
Check ownership & session Active
-> Load persona + allowed knowledge (qua StakeholderKnowledgeAccess)
-> Load 20 message gần nhất của agent
-> Build StakeholderAgentRequest (đọc header X-AI-*)
-> AiRuntimeClient.RespondAsync (timeout 30s, 1 retry khi 5xx)
-> Lưu Message(Learner) + Message(Stakeholder)
-> Cập nhật SessionAgent (MemorySummary, trust, patience, cooperation, LastInteractionAt)
-> Trả { learnerMessage, stakeholderMessage, usedKnowledgeItemIds }
```
Lỗi runtime map sang: `InvalidApiKey -> 422`, `RateLimited -> 429`, `Timeout/Unavailable -> 503` (không lưu message nửa chừng; lưu message learner và đánh dấu lỗi là việc Sprint 2).

Cần: `IAiRuntimeClient` + `HttpClient` typed, cấu hình `AiRuntime:BaseUrl`, `AiRuntime:InternalToken` trong `appsettings`, redact `X-AI-Key`.

### A5. Test và tài liệu (khoảng 0h dư, làm trong từng mục)
- Integration test cho endpoint 3-14 theo mẫu `AuthApiHost`/`AuthApiTests`.
- Cập nhật `BAVES.Api.http` với các request mẫu.
- Swagger XML comment cho từng action.

---

## 3. DEV B: Endpoint và công việc (~30h)

### B1. Contract và AI stub (3h)
Chốt mục 1 cùng A, dựng stub trả `[stub]` trong 2 ngày đầu, đưa A chạy thử.

### B2. Seed OmniMart (8h)
Script SQL trong `database/` (ví dụ `seed-omnimart.sql`) idempotent, gồm:
- 1 Industry (Retail), 1 Application (Order Management), 1 Scenario `OMNI-001` published, 1 ScenarioVersion published.
- 3 `StakeholderPersona`: Operations Manager, Sales/Customer Service Lead, IT Lead (đủ các trường goals, pain points, communication style, personality).
- ~30 `KnowledgeItem` (có mức `Importance`, phân loại `Category`; đánh dấu 5-6 item là hidden/chỉ mở khi hỏi đúng).
- `StakeholderKnowledgeAccess`: mỗi persona thấy đúng tập của mình; có vài item chỉ một persona biết.
- 8 `ScenarioBusinessRule`, 2 `ScenarioEvent` (E1, E2, chưa cần chạy ở Sprint 1).
- Artifact template BRD và Rubric: chỉ cần khai báo dữ liệu nếu bảng đã sẵn sàng; không bắt buộc hoàn thiện ở Sprint 1.
- Canary: ghi sẵn vài "hidden fact" để Sprint 4 kiểm tra rò rỉ.

### B3. API-02 Authoring, phần write (8h)
Module **Scenario**. Role `Instructor` hoặc `Admin`. Instructor chỉ sửa scenario do mình tạo (`CreatedByUserId`).

| # | Endpoint | Mô tả |
| --- | --- | --- |
| 15 | `POST /api/scenarios` | `{ applicationId, code, title, description }` -> tạo `Scenario` (`Draft`) kèm version 1 rỗng (draft). Code unique -> 409 nếu trùng |
| 16 | `POST /api/scenarios/{id}/versions` | Tạo version mới (`VersionNo` + 1), copy từ version gần nhất; chỉ cho phép khi không có draft nào đang mở |
| 17 | `GET /api/scenario-versions/{id}/definition` | Trả **toàn bộ** aggregate (kể cả hidden fields) cho chủ scenario |
| 18 | `PUT /api/scenario-versions/{id}/definition` | Thay toàn bộ aggregate **chỉ khi chưa publish**: context fields, personas, knowledge items, access mapping, business rules, events. Validate trong một transaction. Version đã publish -> 409 |
| 19 | `POST /api/scenario-versions/{id}/publish` | Validate tối thiểu rồi `IsPublished = true`, `PublishedAt`, `Scenario.Status = Published`. Sau publish **bất biến** |

Validate khi publish (trả danh sách lỗi):
- đủ `businessContext, businessProblem, businessObjectives, baRole, assignment`;
- tối thiểu 1 persona active;
- mỗi persona có ít nhất 1 knowledge item được cấp;
- không có `KnowledgeItem.Code` trùng trong version.

`definition` dùng DTO lồng nhau (`personas[]`, `knowledgeItems[]`, `knowledgeAccess[] {personaCode, knowledgeCode}`, `businessRules[]`, `events[]`) tham chiếu bằng **code** thay vì id để client không phải biết id trước.

> **Nếu trễ:** giữ endpoint 15, 17, 19; bỏ 16, 18 và seed scenario bằng script (B2). Làm đầy đủ ở Sprint 2.

### B4. API-04 BYOK validate (1h phía .NET)
| # | Endpoint | Mô tả |
| --- | --- | --- |
| 20 | `POST /api/ai/connections/validate` | Đọc `X-AI-Provider/Model/Key` -> gọi runtime `/v1/connections/validate` -> trả `{ valid, provider, model, message }`. Không lưu key |

### B5. Python AI Runtime (10h)
Thư mục `agent-runtime/` (cùng repo hoặc repo riêng, thống nhất với A).

| Endpoint runtime | Mô tả |
| --- | --- |
| `GET /health` | `{ "status": "healthy" }` |
| `POST /v1/connections/validate` | Gọi nhẹ tới provider (list model hoặc prompt 1 token); trả `valid` hoặc mã lỗi chuẩn hóa |
| `POST /v1/stakeholder/respond` | Theo contract mục 1; thay stub khi sẵn sàng |

Việc cần làm:
1. FastAPI skeleton, cấu hình, `X-Internal-Token` auth, log đã redact key (3h).
2. LangChain model adapter cho OpenAI và Gemini; BYOK key lấy từ request, không lưu (3h). Anthropic để sau.
3. Prompt stakeholder (3h): vai trò, giọng nói, goals/pain points, **cấm** tự bịa thông tin ngoài `allowedKnowledge`, không tiết lộ hidden item trừ khi learner hỏi trúng; không viết requirement/BRD thay learner; luôn trả lời trong vai.
4. Lọc và chọn knowledge liên quan (1h): Sprint 1 có thể dùng ranking đơn giản (từ khóa/embedding nhỏ) trên danh sách `allowedKnowledge` đã được .NET lọc. RAG đầy đủ với Qdrant để Sprint 2-4.
5. Structured output validate bằng pydantic, retry 1 lần khi sai schema; map lỗi provider sang `InvalidApiKey/RateLimited/Timeout/InvalidOutput`.
6. Test: unit test prompt builder + test contract bằng `pytest` với model giả.

---

## 4. Lịch theo ngày (gợi ý)

| Ngày | Dev A | Dev B |
| --- | --- | --- |
| 1 | A1.1 SQL Server + chạy auth test; chốt quy ước 0 | Chốt contract mục 1; skeleton FastAPI + `/health` |
| 2 | A1.3-A1.4 roles/policy/ownership; **nhận stub từ B** | Stub `respond`; bắt đầu seed OmniMart (persona + knowledge) |
| 3 | A2 catalogue read (endpoint 1-4), cần seed của B | Hoàn tất seed; B3 authoring endpoint 15, 17, 19 |
| 4 | A3 session + plan (endpoint 5-12) | B3 endpoint 16, 18; LangChain adapter + prompt thật |
| 5 | A4 interview endpoint 13-14 nối runtime thật; test tích hợp | B4 validate + hoàn thiện runtime; **buổi test tích hợp chung** |

Thứ Sáu buổi chiều: chạy gate cùng nhau với AI thật (không dùng stub), ghi lại lỗi còn lại sang Sprint 2.

---

## 5. Definition of Done của Sprint 1

- [ ] SQL Server chạy thật; auth test pass; roles và user demo đã seed.
- [ ] 20 endpoint .NET (mục 2-3) có Swagger, validation và phân quyền; learner không đọc được dữ liệu của learner khác.
- [ ] DTO phía Learner không chứa hidden knowledge (có test kiểm tra).
- [ ] `X-AI-Key` không xuất hiện trong log hoặc DB (có test hoặc kiểm tra thủ công).
- [ ] Runtime Python chạy bằng một lệnh (`uvicorn`), có README cách chạy và biến môi trường.
- [ ] Gate chạy được trên máy hai dev:
```text
register/login -> GET industries -> GET applications -> GET scenarios
-> GET context -> POST sessions -> GET agents -> PUT plan -> POST question
-> POST messages (AI thật) -> GET messages
```

## 6. Danh sách endpoint tổng hợp

| # | Method | Path | Owner |
| --- | --- | --- | --- |
| 1 | GET | `/api/industries` | A |
| 2 | GET | `/api/applications` | A |
| 3 | GET | `/api/scenarios` | A |
| 4 | GET | `/api/scenario-versions/{id}/context` | A |
| 5 | POST | `/api/sessions` | A |
| 6 | GET | `/api/sessions/{id}` | A |
| 7 | GET | `/api/sessions/{id}/agents` | A |
| 8 | GET | `/api/sessions/{id}/plan` | A |
| 9 | PUT | `/api/sessions/{id}/plan` | A |
| 10 | POST | `/api/sessions/{id}/plan/questions` | A |
| 11 | PATCH | `/api/planned-questions/{id}` | A |
| 12 | DELETE | `/api/planned-questions/{id}` | A |
| 13 | POST | `/api/sessions/{sessionId}/agents/{agentId}/messages` | A |
| 14 | GET | `/api/sessions/{sessionId}/agents/{agentId}/messages` | A |
| 15 | POST | `/api/scenarios` | B |
| 16 | POST | `/api/scenarios/{id}/versions` | B |
| 17 | GET | `/api/scenario-versions/{id}/definition` | B |
| 18 | PUT | `/api/scenario-versions/{id}/definition` | B |
| 19 | POST | `/api/scenario-versions/{id}/publish` | B |
| 20 | POST | `/api/ai/connections/validate` | B |
| R1 | GET | runtime `/health` | B |
| R2 | POST | runtime `/v1/connections/validate` | B |
| R3 | POST | runtime `/v1/stakeholder/respond` | B |

Tải cân bằng: A 14 endpoint .NET + SQL/roles/policy; B 6 endpoint .NET + 3 endpoint runtime + seed + prompt/LLM.
