# BA-VES: Timeline Backend và AI trong 4 tuần

Ngày lập kế hoạch: 06/10/2026.

## 1. Mục tiêu và giả định

Mục tiêu là một scenario có thể chạy xuyên suốt trước hội đồng, không phải toàn bộ sản phẩm BA-VES đã sẵn sàng vận hành thương mại.

Luồng phải hoàn thành:

```text
Instructor cấu hình và publish scenario
Learner chọn Industry / Application / Scenario
Đọc context -> Lập kế hoạch -> Phỏng vấn stakeholder
Nhận coach feedback vừa đủ -> Phân tích -> Xác nhận
Soạn BRD -> Đóng băng phiên bản -> Nộp
AI đánh giá có evidence -> Learner yêu cầu Expert
Expert xem hồ sơ, chấm điểm -> Learner nhận kết quả cuối
```

Giả định nguồn lực cho timeline chuẩn:

- A: 1 người phụ trách ASP.NET Core, SQL Server, API và tích hợp nghiệp vụ.
- B: 1 người phụ trách Python AI runtime, LangChain/LangGraph, prompt, RAG và AI evaluation.
- Mỗi người có khoảng 6 giờ làm việc tập trung/ngày, 20 ngày làm việc: khoảng 240 giờ tổng cộng. Hai ngày cuối dành cho ổn định và dự phòng, không mở rộng phạm vi.
- Timeline này giả định A đã quen ASP.NET Core/EF Core và B đã làm được một LLM integration cơ bản. Nếu đồng thời phải học mới framework, cần giảm phạm vi ngay sau spike ngày 2.
- Desktop/UI được làm song song bởi người khác, nhận API contract từ tuần 1. Backend không tạo trang soạn thảo; backend cung cấp template, autosave, version và submission cho editor.
- Nếu chỉ có 1 người thực hiện cả A và B, dùng phạm vi giảm ở mục 13. Không cộng hai cột công việc mỗi ngày rồi coi đó là một ngày công của một người.
- Ngày 1..20 là ngày làm việc tương đối, chưa chốt lịch bắt đầu hay ngày hội đồng.

### Hiện trạng đã đối chiếu source

- .NET 9, EF Core, SQL Server, modular monolith; 9 module và 40 bảng trong baseline MVP.
- Có các model chính cho Scenario, Simulation, Planning, Analysis, Artifact và Assessment; có mapping theo schema của từng module.
- Đã có Register, Login, Refresh, Logout và `users/me` bằng Identity/JWT; lượt triển khai trước có 35 test xác thực pass trên database SQLite tạm.
- Chưa có API nghiệp vụ cho các module còn lại, chưa có AI runtime chạy thực tế.
- SQL Server thực chưa được xác nhận hoạt động. Khởi động database và chạy integration test trên SQL Server là công việc ngày 1, không để tới tuần 4.
- README tổng thể có nhiều chức năng roadmap. Các mục đó không được tính là đã triển khai chỉ vì có trong tài liệu.

## 2. Phạm vi giao trước hội đồng

### P0: Bắt buộc

| Nhóm | Kết quả cần có |
| --- | --- |
| Identity | Dùng lại API hiện có; kiểm tra quyền Learner, Instructor, Expert và ownership trên từng resource |
| Content / Scenario | 1 industry, 1 application, 1 scenario published; sửa cấu hình draft theo aggregate và publish được version mới |
| Scenario package | 3 stakeholder, khoảng 30 knowledge item, 8 business rule, 2 event, 1 BRD template, 1 rubric |
| Planning | Lưu mục tiêu, cách tiếp cận, thông tin cần thu thập, đầu ra dự kiến, câu hỏi và stakeholder mục tiêu |
| Simulation | Tạo phiên, phỏng vấn từng stakeholder, nhớ hội thoại, kiến thức riêng, xử lý timeout/retry, resume phiên |
| Coach | Feedback ngắn về kỹ thuật elicitation; không tiết lộ đáp án, không viết câu hỏi hay artifact thay learner |
| Nhịp doanh nghiệp | 2 sprint nhỏ với các time chunk; có ít nhất 1 thay đổi nghiệp vụ tác động thật vào context |
| Analysis | Finding, Requirement, BusinessRule, Constraint, Risk, OpenQuestion; dẫn chứng từ message; xác nhận với stakeholder |
| Artifact | Template BRD có phần stakeholder, yêu cầu, business rule, user story và acceptance criteria; draft, version, submission |
| AI assessment | Đánh giá bất đồng bộ, rubric cố định, evidence hợp lệ, điểm AI chỉ là điểm gợi ý |
| Expert | Request, hàng đợi, nhận request, đọc hồ sơ và đánh giá AI, chấm điểm cuối cùng |
| Reliability | Phân quyền, idempotency, job state, log đã redact, bộ test và kịch bản demo lặp lại |

### P1: Chỉ làm khi P0 đã ổn định

- Một cohort và assignment tối thiểu để Instructor giao cùng scenario cho nhóm learner.
- Trao đổi Learner–Expert bằng thread văn bản đơn giản, nếu bổ sung đúng bảng và quyền truy cập.
- SSE thông báo trạng thái job hoặc câu trả lời; API polling vẫn phải hoạt động độc lập.
- Export báo cáo PDF nếu thư viện và thời gian cho phép; P0 chỉ cần báo cáo JSON/HTML do desktop hiển thị.

### Không thuộc 4 tuần

Marketplace Expert tự động, thanh toán, video/voice, nhiều provider/model tùy ý, chợ scenario, AI tự sinh toàn bộ scenario không qua duyệt, agent tự chạy SQL, fine-tuning, microservices nghiệp vụ, workflow engine tổng quát, analytics lớn và editor BPMN/UML chuyên sâu.

## 3. Scenario dùng để demo

Tên đề xuất: **OmniMart - Cải tiến xử lý đơn hàng đa kênh**. Đây là doanh nghiệp và số liệu giả lập do nhóm tự xây dựng.

- Industry: Retail. Application: Order Management.
- Bối cảnh: đơn từ website và cửa hàng xử lý qua nhiều nguồn; trạng thái đơn, tồn kho và thông báo khách hàng không đồng nhất.
- Vấn đề: giữ hàng sai thời điểm, đơn bị xử lý trùng, quy trình hủy/hoàn tiền chưa rõ, kỳ vọng về tốc độ cập nhật khác nhau giữa các phòng ban.
- Assignment: learner làm BA, elicitation và đề xuất yêu cầu cho phiên bản cải tiến đầu tiên. Không yêu cầu lập trình hệ thống bán hàng.
- Đầu ra: một BRD hoàn chỉnh với phạm vi, stakeholder, requirements, rules, constraints, user stories, acceptance criteria và traceability.

| Stakeholder | Goals / pain points | Cách nói | Giới hạn kiến thức |
| --- | --- | --- | --- |
| Operations Manager | Giảm đơn trùng, xác định khi nào giữ/trả tồn kho; ngại tăng thao tác cho nhân viên | Ngắn, khó tính, yêu cầu learner hỏi cụ thể | Biết quy trình vận hành và ngoại lệ; không biết chi tiết API tích hợp |
| Sales / Customer Service Lead | Khách được thông báo đúng lúc, ít khiếu nại; ngại quy trình hủy đơn phức tạp | Nhiệt tình, có thể nói lan man | Biết nhu cầu khách hàng; chỉ biết một phần quy trình phê duyệt hoàn tiền |
| IT Lead | Tích hợp khả thi, phân quyền rõ, tránh sửa hệ thống lõi quá nhiều | Thận trọng, yêu cầu làm rõ phạm vi và số liệu | Biết giới hạn kỹ thuật; không tự quyết business policy |

Hai mâu thuẫn chủ đích để learner phát hiện:

- Sales mong muốn cập nhật gần thời gian thực, trong khi tích hợp hiện tại chỉ hỗ trợ chu kỳ đồng bộ.
- Operations muốn linh hoạt hoàn tiền, trong khi quy tắc phê duyệt phụ thuộc mức tiền và thẩm quyền.

Hai event:

- E1: Sau một mốc interview, thay đổi giới hạn tích hợp. Learner phải cập nhật constraint và hỏi lại stakeholder liên quan.
- E2: Sang sprint 2, phát sinh yêu cầu về tải trong chương trình bán hàng. Learner phải bổ sung NFR hoặc ghi nhận thông tin chưa đủ.

Không đặt mọi goal, pain point, business rule hay đáp án trong API context dành cho learner. Profile công khai chỉ chứa thông tin được phép biết trước phỏng vấn.

### Nhịp làm việc

Sprint 1: đọc context, lập BA plan, phỏng vấn và ghi nhận thông tin. Sprint 2: xử lý thay đổi, phân tích/xác nhận, hoàn thiện BRD và nộp.

Time chunk là cấu hình nghiệp vụ do backend tính theo đồng hồ server. Với bản demo, Instructor có thể trigger event qua endpoint được phân quyền hoặc dùng chế độ thời gian rút gọn trong môi trường demo. Không để learner tự chọn `demoMode`, tự sửa thời gian hay tự mở knowledge ẩn.

## 4. Kiến trúc đề xuất

Giữ toàn bộ nghiệp vụ và dữ liệu authoritative trong modular monolith `BAVES.Api`.

```text
Desktop
  -> BAVES.Api (.NET): Identity, ownership, rules, workflow, SQL Server
  -> AI integration client trong API
  -> Python AI runtime nội bộ: FastAPI + LangChain + LangGraph
  -> LLM provider qua BYOK

AI runtime:
  - Embedding model local
  - Qdrant local persisted index cho bản demo
  - LangGraph checkpoint SQLite cho một runtime instance
```

Python runtime là thành phần hỗ trợ AI, không phải nơi chứa lại toàn bộ module nghiệp vụ. Runtime không trực tiếp chỉnh SQL Server, không tự quyết quyền truy cập hay điểm cuối cùng.

LangChain cung cấp phần model/tool/structured output; LangGraph phù hợp với workflow có bước và nhánh rõ ràng. Với BA-VES, chọn workflow được kiểm soát thay vì để một supervisor LLM tự quyết mọi bước. [Tài liệu workflow và agent của LangGraph](https://docs.langchain.com/oss/python/langgraph/workflows-agents).

### Ranh giới giao tiếp

- API chỉ gửi context đã được phép cấp cho đúng stakeholder và đúng session.
- Authoring/publish phải ghim content hash của index. Chỉ mở session khi index đúng version đã sẵn sàng, hoặc đã bật fallback direct permitted context được kiểm thử; không dùng index stale để trả lời cho version mới.
- Runtime nhận DTO nội bộ có `operationId`, `sessionId`, `agentId`, `scenarioVersionId`, references, prompt/model profile và correlation ID.
- Các ID/allowlist phải do API xây từ JWT và dữ liệu server; không tin `learnerId`, `allowedKnowledgeIds`, `reviewerId` do client tự gửi.
- Giao tiếp API–runtime có authentication nội bộ; bind loopback trong demo, TLS/mTLS hoặc mạng nội bộ có kiểm soát khi triển khai remote.
- Dữ liệu runtime trả về phải qua schema validation và business validation của API trước khi persist.
- SQL Server là nguồn chính cho transcript, artifact, submission và review. Vector index và checkpoint là dữ liệu hỗ trợ, có thể rebuild/reconcile.
- Qdrant local và SQLite checkpoint chỉ áp dụng cho demo một runtime instance, không coi đây là cấu hình nhiều replica production.

## 5. Những khoảng trống dữ liệu cần chốt ngày 1

Đây là **đề xuất thay đổi tương lai**, chưa được thực hiện trong lượt thiết kế timeline này. Baseline hiện vẫn là 40 bảng. Bổ sung bảng/cột phải có migration được review và cập nhật model validation tương ứng, không tuyên bố tiếp tục đúng nguyên baseline nếu đã mở rộng.

| Khoảng trống trong model hiện tại | Phương án P0 |
| --- | --- |
| `HumanReview` chỉ có kết quả hoàn tất, không có request hay trạng thái nhận việc | Thêm `assessment.ReviewRequests`: SubmissionId, LearnerId, AssignedReviewerId?, Status, RequestNote?, RequestedAt, AcceptedAt?, CompletedAt?; một request đang mở/submission |
| Không có job AI bền vững, retry lease hay idempotency state | Thêm module AI integration và `ai.AIOperations`: loại job, owner, scenario version/session/submission reference, idempotency key, request hash, status, lease, attempts, input snapshot/reference, result, model/prompt revision, usage, timestamps và error code |
| Coach feedback cần được lấy lại sau reconnect | Lưu kết quả coach có schema trong AIOperations; chưa cần thêm một bảng CoachFeedback riêng |
| `KnowledgeAccessType.Partial` chưa có nội dung dành riêng cho quyền partial | Thêm `StakeholderKnowledgeAccess.AllowedContent?`; bắt buộc có bản nội dung rút gọn với quyền Partial. Không đưa full Content cho LLM rồi nhắc nó giấu bớt |
| `ScenarioVersion` chưa có cấu hình work style/time chunk | Thêm `WorkStyleJson` được validate và đóng băng cùng scenario version |
| `BAPlan` chưa có trường lưu đầu ra dự kiến độc lập | Thêm `ExpectedOutputsJson`, hoặc chọn hợp đồng nội dung Approach có cấu trúc ngay ngày 1; không để UI yêu cầu một trường backend không lưu được |
| Submission chưa ghim rubric cho independent learner | Thêm `Submission.RubricId`; publish rubric là immutable. AI và Expert cùng dùng rubric đã ghim, không lấy bản mới nhất tùy thời điểm |
| Event có thể bị trigger hai lần | Nếu mỗi event chỉ xảy ra một lần trong MVP, thêm unique `(SessionId, ScenarioEventId)` và test request đồng thời |

ReviewRequests không chứa điểm 0 giả để đại diện cho request pending. HumanReviews chỉ được tạo khi Expert thực sự hoàn tất đánh giá.

Các trạng thái đề xuất:

- AI operation: `Queued -> Running -> Completed / Failed / RequiresCredential`; retry có lease, số lần tối đa và thời điểm retry.
- Review request: `Pending -> Accepted -> Completed`, hoặc `Rejected / Cancelled` theo quyền và điều kiện.
- Giữ các enum hiện tại `SessionStatus`, `SubmissionStatus` cho vòng đời chính; phase planning/interview/analysis được quản lý riêng, không gán thêm giá trị giả vào enum cũ.

### Quy tắc transaction và version

- Pin `ScenarioVersionId` từ khi tạo session. Published version, template dùng để đánh giá và rubric không sửa trực tiếp; tạo version mới.
- API save draft chỉ sửa draft. ArtifactVersion đã dùng trong submission không được sửa/xóa qua API.
- Một Submission hiện tham chiếu một ArtifactVersion. P0 dùng một BRD có đủ các section thay vì hứa nộp một bộ nhiều artifact mà model chưa hỗ trợ.
- Submission, pending assessment và AI operation phải được tạo nhất quán. Với một SQL Server database, chốt application orchestrator chia sẻ SQL connection/transaction giữa DbContext liên quan, trong execution strategy. Không tự thêm entity của module khác vào một DbContext.
- Snapshot đánh giá gồm artifact version, plan, analysis và transcript tại thời điểm nộp, hoặc các references immutable tương đương. Không đánh giá dữ liệu đang thay đổi rồi gọi đó là kết quả của bản nộp cũ.
- Không giữ transaction SQL mở trong lúc chờ LLM. Tạo operation trước, gọi AI ngoài transaction, sau đó persist kết quả và side effect một cách idempotent.
- Runtime/checkpoint/SQL không có một transaction chung. Mỗi operation có ID, kiểm tra trạng thái commit và reconciliation; retry không được nhân đôi message, reveal hoặc assessment.

## 6. Danh mục API

Đây là API contract **đề xuất**, không phải danh sách endpoint đã có. Dùng prefix `/api`; phần Identity dưới đây đã triển khai. Các route nghiệp vụ còn lại sẽ được tạo theo timeline.

Ký hiệu quyền: L = Learner, I = Instructor, E = Expert. Mọi resource route vẫn phải kiểm tra ownership/assignment, không chỉ kiểm tra role.

### 6.1. Identity hiện có

| Method | Route | Mục đích |
| --- | --- | --- |
| POST | `/api/auth/register` | Tạo Learner |
| POST | `/api/auth/login` | Nhận token pair |
| POST | `/api/auth/refresh` | Rotate refresh token |
| POST | `/api/auth/logout` | Thu hồi session đăng nhập |
| GET | `/api/users/me` | Profile và roles hiện tại |

Tài khoản Instructor/Expert cho demo được seed bằng công cụ quản trị; không cho public Register tự cấp role.

### 6.2. Content và Scenario - tuần 1

| Method | Route | Quyền / chức năng |
| --- | --- | --- |
| GET | `/api/industries` | L/I: catalogue |
| GET | `/api/applications?industryId=...` | L/I: application theo industry |
| GET | `/api/scenarios?applicationId=...` | L/I: bản scenario được phép xem |
| GET | `/api/scenario-versions/{versionId}/context` | L: brief và stakeholder public; không trả knowledge ẩn |
| POST | `/api/scenarios` | I: tạo scenario |
| POST | `/api/scenarios/{scenarioId}/versions` | I: tạo draft mới/clone bản cũ |
| GET | `/api/scenario-versions/{versionId}/definition` | I là chủ scenario: cấu hình authoring đầy đủ |
| PUT | `/api/scenario-versions/{versionId}/definition` | I: lưu aggregate personas, knowledge access, rules, events và work style của draft |
| POST | `/api/scenario-versions/{versionId}/publish` | I: validate package rồi publish; không sửa bản published |

Không triển khai hàng loạt CRUD cho từng bảng để chỉ có một scenario. Seed/import dùng cùng validation với authoring service; không có endpoint import tùy ý cho learner.

### 6.3. Session và Planning - tuần 1

| Method | Route | Quyền / chức năng |
| --- | --- | --- |
| POST | `/api/sessions` | L: mở phiên từ published version/assignment hợp lệ |
| GET | `/api/sessions/{sessionId}` | L là owner: trạng thái, phiên bản và phase |
| GET | `/api/sessions/{sessionId}/agents` | L: stakeholder public của phiên |
| GET, PUT | `/api/sessions/{sessionId}/plan` | L: đọc/lưu BA plan |
| POST | `/api/sessions/{sessionId}/plan/questions` | L: thêm câu hỏi, information need và target stakeholder |
| PATCH, DELETE | `/api/planned-questions/{questionId}` | L là owner: sửa hoặc xóa câu hỏi draft |

### 6.4. BYOK, Interview và Coach - tuần 1..2

| Method | Route | Quyền / chức năng |
| --- | --- | --- |
| GET | `/api/ai/providers` | User: provider/model capabilities đã kiểm thử, không lộ secret |
| POST | `/api/ai/connections` | User: tạo kết nối BYOK tạm; trả connectionId và expiry, không trả key |
| POST | `/api/ai/connections/{connectionId}/validate` | Owner: kiểm tra key/model bằng request nhỏ |
| DELETE | `/api/ai/connections/{connectionId}` | Owner: xóa secret tạm khỏi memory |
| POST | `/api/sessions/{sessionId}/agents/{agentId}/messages` | L: gửi một lượt interview; trả 202 và operationId |
| GET | `/api/sessions/{sessionId}/agents/{agentId}/messages` | L: transcript theo cursor; không trả retrieval nội bộ |
| GET | `/api/ai/operations/{operationId}` | Owner: status/result đã lọc; dùng polling |
| POST | `/api/sessions/{sessionId}/coach-feedback` | L: yêu cầu coach cho cửa sổ hội thoại hiện tại |
| GET | `/api/sessions/{sessionId}/coach-feedback` | L: feedback đã lưu; không gọi LLM lại khi đọc |
| GET | `/api/sessions/{sessionId}/events` | L: event đã công bố cho phiên |
| POST | `/api/sessions/{sessionId}/events/{eventId}/trigger` | I có quyền quản lý phiên: trigger demo/manual event hợp lệ |

Các POST phát sinh AI nhận `Idempotency-Key`; trùng key và cùng payload trả lại operation cũ, trùng key khác payload trả 409. Một lượt AI đang chạy/agent/session; lượt sau phải chờ hoặc nhận 409. Polling không gọi lại model.

SSE P1: `/api/sessions/{sessionId}/stream` để thông báo kết quả đã commit. Nếu stream token trước validation, không thể thu hồi nội dung đã lộ; P0 chỉ công bố câu trả lời sau kiểm tra.

### 6.5. Analysis và Confirmation - tuần 2

| Method | Route | Quyền / chức năng |
| --- | --- | --- |
| GET, POST | `/api/sessions/{sessionId}/analysis-items` | L: danh sách/thêm item |
| PATCH, DELETE | `/api/analysis-items/{itemId}` | L: sửa/archive theo trạng thái; xóa chỉ khi chưa bị khóa/tham chiếu |
| POST | `/api/analysis-items/{itemId}/evidence` | L: gắn message cùng session |
| DELETE | `/api/analysis-items/{itemId}/evidence/{linkId}` | L: bỏ evidence khi còn được sửa |
| POST | `/api/analysis-items/{itemId}/confirmation` | L: gửi proposition tới stakeholder được chọn, tạo AI operation |

Confirmation trả `confirmed / disputed / insufficient_information` và supporting message references. Chỉ cập nhật item sang Confirmed khi outcome phù hợp và evidence đã persist; phản hồi của stakeholder mô phỏng không phải chứng nhận đúng tuyệt đối.

### 6.6. Artifact, Submission và Assessment - tuần 3

| Method | Route | Quyền / chức năng |
| --- | --- | --- |
| GET | `/api/artifact-templates/{templateId}` | L: template published và section schema |
| POST, GET | `/api/sessions/{sessionId}/artifacts` | L: tạo/lấy BRD draft |
| GET | `/api/artifacts/{artifactId}` | L owner, reviewer được cấp quyền: đọc artifact |
| PUT | `/api/artifacts/{artifactId}/draft` | L owner: autosave JSON; optimistic concurrency |
| POST | `/api/artifacts/{artifactId}/versions` | L: đóng băng một version |
| POST | `/api/sessions/{sessionId}/submissions` | L: nộp ArtifactVersion; pin rubric và tạo AI operation |
| GET | `/api/submissions/{submissionId}` | L owner / assigned reviewer: hồ sơ submission |
| GET | `/api/submissions/{submissionId}/ai-feedback` | L owner / reviewer: trạng thái và feedback công khai |
| POST | `/api/submissions/{submissionId}/ai-feedback/retry` | L owner: retry failure hợp lệ, có BYOK connection mới nếu cần |
| GET | `/api/submissions/{submissionId}/report` | L owner / reviewer: evidence và kết quả AI/Human đã công bố |

Autosave dùng server revision/ETag hoặc rowversion được chốt ngày 1; stale write trả 409/412 theo contract. Không tin điểm, ownership, thời gian submit hay plain text do client tự khai. Backend kiểm tra structured sections và tạo plain text có canonical format cho assessment.

### 6.7. Expert - tuần 3..4

| Method | Route | Quyền / chức năng |
| --- | --- | --- |
| POST | `/api/submissions/{submissionId}/review-requests` | L owner: yêu cầu Expert; không tự chọn người chấm trái policy |
| GET | `/api/review-requests?scope=mine-or-queue` | L: request của mình; E: queue đã lọc; I: cohort được quản lý |
| POST | `/api/review-requests/{requestId}/accept` | E/I hợp lệ: nhận request bằng conditional update |
| GET | `/api/review-requests/{requestId}/case` | Assigned E/I: plan, transcript, analysis, artifact, AI feedback, rubric |
| POST | `/api/review-requests/{requestId}/complete` | Assigned E/I: criterion scores + comments; backend tính FinalScore |

Expert nhận xét và điểm cuối độc lập với AI. Không tự chuyển SuggestedScore thành FinalScore. Instructor chỉ chấm cohort mình quản lý, trừ khi đồng thời có quyền Expert cho independent learner.

### 6.8. Cohort và trao đổi P1

- `POST /api/cohorts`; `POST /api/cohorts/{id}/members`; `POST /api/cohorts/{id}/assignments`; `GET /api/cohorts/{id}/progress`.
- `POST /api/review-requests/{id}/messages`; `GET /api/review-requests/{id}/messages` chỉ sau khi thêm ReviewMessages và kiểm tra participant permission.

Nếu P1 chưa làm, ghi đúng là demo independent learner và Instructor authoring, không tuyên bố đã hoàn thiện quản lý cohort hay chat Expert.

## 7. Cấu hình LLM, Embedding và BYOK

### 7.1. Chọn LLM

Trong 4 tuần, chỉ hỗ trợ **1 provider BYOK và 1 chat model chính đã kiểm thử**, dùng chung model đó với các prompt profile khác nhau. Model chấm mạnh hơn là lựa chọn P1, không cần ba model riêng để có ba loại agent.

Ngày 2 chốt `providerId` và model ID chính xác dựa trên key sẵn có, tài liệu provider, khả năng tiếng Việt, structured output, giới hạn context và benchmark nội bộ. Không chọn theo tên mới nhất hoặc giả định mọi model hỗ trợ temperature/tool calling giống nhau. Lưu model ID/revision, provider và prompt version ở từng operation để audit.

| Profile | Dữ liệu đầu vào | Tham số ban đầu để thử | Không được làm |
| --- | --- | --- | --- |
| Stakeholder | Persona, knowledge được phép, event của đúng agent, memory, recent transcript | Temperature 0.3..0.5 nếu hỗ trợ; output khoảng 400 tokens | Biết thông tin của persona khác; tiết lộ private context; làm BA thay learner |
| Coach | Learner plan, transcript đã diễn ra, public events, BABOK checklist tự biên soạn | Temperature 0..0.2; output khoảng 450 tokens; mỗi 3 lượt hoặc learner yêu cầu | Đọc hidden answer key để gợi đáp án; tạo bộ câu hỏi mẫu hay BRD hoàn chỉnh |
| Assessor | Submission snapshot, pinned rubric, learner evidence, expected facts nội bộ đúng scenario | Temperature 0..0.1; output khoảng 2.500 tokens | Tự cho FinalScore; bỏ qua rubric; tạo evidence ID không có thật |

Các số này là cấu hình thử nghiệm, không phải cam kết latency/chất lượng. Temperature 0 không bảo đảm kết quả tuyệt đối giống nhau. Đặt timeout, cap input/output, retry giới hạn và đo token usage; không chạy vòng sửa/chấm vô hạn.

### 7.2. Embedding

Embedding dùng để tìm thông tin liên quan về ngữ nghĩa; LLM dùng để sinh câu trả lời, feedback và nhận xét. Embedding không phải agent, không chấm điểm cuối và không tự quyết quyền knowledge.

Đề xuất demo: `BAAI/bge-m3` chạy local trong AI runtime, dùng dense vector 1024 chiều. Đây là model multilingual; cần benchmark tiếng Việt trên câu hỏi thật của scenario và đo RAM/latency trên máy demo trước khi chốt. [Model card chính thức của BAAI](https://huggingface.co/BAAI/bge-m3).

Chọn embedding local giúp retrieval không phụ thuộc learner có key embedding cùng provider với chat hay không. Nếu máy không đáp ứng, chuyển sang một embedding API được hỗ trợ trong ngày 2 và chốt cách cấp credential; không đổi model âm thầm giữa tuần.

- Knowledge ngắn: mỗi fact là một document, không cắt nhỏ máy móc.
- Tài liệu dài: thử chunk 300..500 tokens, overlap 50..80; không trộn facts có quyền disclosure khác nhau vào cùng chunk.
- Dùng cùng model/revision/dimension cho index và query. Thay model phải rebuild một collection mới.
- Lưu metadata: scenarioVersionId, personaId hoặc accessible view ID, knowledgeItemId, access type, disclosure policy revision, content hash và embedding revision.
- Chỉ embed lại khi content hash thay đổi; publish scenario version mới không ghi đè index version cũ đang phục vụ session.
- Interview: bắt đầu với top-k 3..5 sau lọc quyền; không retrieval đủ liên quan thì stakeholder nói chưa rõ/không biết, không bịa.
- Coach corpus tách khỏi answer key; assessment dùng facts/rubric đầy đủ được API cấp riêng.
- Với 30 facts, có thể cấp trực tiếp toàn bộ tập facts nhỏ đã được phép để kiểm tra đường cơ sở. RAG chỉ được giữ nếu đo được lợi ích; không xem thêm vector database là tự động làm hệ thống tốt hơn.

Qdrant có local mode lưu trên disk và integration LangChain, phù hợp với demo một instance không cần dựng thêm vector server. [Qdrant integration chính thức](https://docs.langchain.com/oss/python/integrations/vectorstores/qdrant).

### 7.3. Pipeline retrieval có giới hạn quyền

```text
Scenario package đã được Instructor duyệt
-> Tạo view Knows / Partial riêng cho persona
-> Embed và index theo scenario version + content hash

Learner hỏi
-> API xác minh owner, session, agent và trạng thái
-> API tính allowed knowledge IDs + disclosure gates + event effects
-> Query embedding
-> Retrieve trong allowlist, đúng version và đúng persona
-> Kiểm tra lại IDs/content view từ nguồn authoritative
-> Build prompt từ nội dung được phép
-> LLM trả structured response
-> API kiểm tra schema, evidence và persist kết quả
```

Bộ lọc phải được áp dụng trước khi cấp nội dung cho LLM, không tìm cả corpus rồi để prompt tự giấu. Nếu dùng Qdrant, kết hợp các điều kiện bắt buộc theo version/persona/allowlist; bộ lọc là một lớp bổ sung, không thay thế authorization của API. [Tài liệu filtering của Qdrant](https://qdrant.tech/documentation/search/filtering/).

`IsHidden` và `DisclosureRule` cần một rule schema giới hạn do backend thực thi, ví dụ min interaction count, prerequisite fact hoặc phase. Không cho LLM diễn giải chuỗi rule tự do để mở quyền. Với Partial, index/cấp AllowedContent, tuyệt đối không cấp full Content.

### 7.4. BYOK

- Desktop lưu key trong credential storage của hệ điều hành; không lưu plaintext trong file config hay bảng database.
- Với mô hình demo API gọi AI runtime trên server, key **vẫn đi qua API/runtime** qua kết nối bảo vệ và sống tạm trong memory. Chỉ nói “không persist”, không tuyên bố “key không rời desktop”.
- POST connection nhận key một lần; connectionId gắn với user, TTL và provider/model. API và runtime không dùng biến môi trường global cho key của nhiều user.
- Key đi tới model adapter bằng runtime context/dependency không checkpointable; không nằm trong LangGraph state, metadata, queue payload, tracing hoặc exception.
- Việc dùng runtime context không tự chứng minh secret được bảo vệ. Phải kiểm tra thực tế checkpoint, queue payload và log trong test; ứng dụng chủ động loại bỏ credential khỏi mọi dữ liệu serialize.
- Không log Authorization header, API key, body tạo connection hay prompt đầy đủ có dữ liệu nhạy cảm. LangSmith/remote tracing tắt mặc định cho demo trừ khi đã có quyết định về dữ liệu và redaction.
- Sau restart, job cần key chuyển RequiresCredential; desktop reconnect và user yêu cầu resume/retry. Không lưu key chỉ để job tự chạy lại sau nhiều giờ.
- Không chấp nhận URL provider tùy ý từ client; dùng provider/base URL allowlist để tránh SSRF và gửi key sai đích.
- Kết quả AI ngoài hệ thống bằng BYOK do learner kiểm soát là feedback hỗ trợ. Nếu cần assessment có độ tin cậy cao, dùng môi trường server kiểm soát và Expert làm quyết định cuối; local agent của learner không phải nguồn điểm chính thức đáng tin.

### 7.5. Ví dụ cấu hình cho bản demo

Đây là cấu hình mục tiêu cho runtime sắp xây, **chưa phải các setting đang được API hiện tại đọc**. Model ID lấy từ biến môi trường sau khi chốt provider ngày 2; không có API key trong YAML.

```yaml
runtime:
  bind: "127.0.0.1"
  port: 8000
  instance_count: 1
llm:
  provider_id: "${BAVES_LLM_PROVIDER}"
  chat_model_id: "${BAVES_CHAT_MODEL_ID}"
  credential_source: "per-user-ephemeral-connection"
  request_timeout_seconds: 45
  max_transport_retries: 1
profiles:
  stakeholder:
    prompt_version: "stakeholder-v1"
    temperature_if_supported: 0.4
    max_output_tokens: 400
  coach:
    prompt_version: "coach-v1"
    temperature_if_supported: 0.1
    max_output_tokens: 450
    every_completed_learner_turns: 3
  assessor:
    prompt_version: "assessor-v1"
    temperature_if_supported: 0.0
    max_output_tokens: 2500
embedding:
  model_id: "BAAI/bge-m3"
  revision: "${BAVES_EMBEDDING_REVISION}"
  dimension: 1024
  retrieval_top_k: 4
vector_store:
  type: "qdrant-local"
  path: "./data/scenario-index"
checkpoint:
  type: "sqlite"
  path: "./data/agent-checkpoints.sqlite"
limits:
  concurrent_turns_per_agent: 1
  structured_output_repairs: 1
  publish_response_only_after_validation: true
observability:
  remote_tracing_enabled: false
  log_credentials: false
  log_full_prompt: false
```

Giới hạn context phải tính bằng tokenizer/model adapter đã chọn; summary cộng recent messages không được vượt budget input. Transport retry có thể vẫn tạo thêm một lần gọi provider khi lần trước chưa rõ kết quả, vì vậy tính usage/retry và chỉ commit side effect một lần bằng operation ID.

## 8. Quản lý agent bằng LangChain và LangGraph

### 8.1. Ba workflow, không phải một agent toàn quyền

1. Interview workflow: một persona được chọn bởi learner, một cửa sổ kiến thức riêng, một memory namespace riêng.
2. Coach workflow: quan sát tiến trình và cách hỏi; không đóng vai stakeholder hay tự điều hướng learner tới đáp án.
3. Assessment workflow: xử lý submission immutable và rubric; kết quả gợi ý để Expert xem.

Không tạo 3 LLM cùng trao đổi tự do với nhau trong mỗi lượt. Một lượt interview chỉ gọi stakeholder đang được chọn; coach chạy riêng khi đủ ngưỡng. Tiết kiệm chi phí bằng context và tần suất, không bằng chia sẻ nhầm memory/knowledge.

### Công cụ và các node được cấp cho từng workflow

| Tool / hàm xác định | Workflow được dùng | Ràng buộc |
| --- | --- | --- |
| `retrieve_permitted_knowledge(query)` | Stakeholder | Chỉ tìm trong allowed views do API đã cấp; LLM không sửa persona/session/allowlist |
| `read_relevant_events()` | Stakeholder / Coach | Chỉ event đã công bố cho đúng audience; không chứa private effects ngoài quyền |
| `read_agent_memory()` | Stakeholder | Đúng thread agent, không đọc summary của stakeholder khác |
| `read_observed_evidence()` | Coach | Chỉ transcript learner đã tạo, không đọc answer key |
| `read_frozen_submission()` | Assessor | Input snapshot đúng operation, không đọc draft mới nhất |
| `read_pinned_rubric()` | Assessor | Chỉ rubric version đã pin; không có quyền sửa |
| `validate_references()` / `calculate_weighted_score()` | Backend/node xác định | Chạy bằng code, không nhờ LLM quyết định ID hợp lệ hay tự tính FinalScore |

Phần lớn các hàm trên được gọi trực tiếp theo graph cố định. Chỉ bọc thành LangChain tools khi thật sự cần model chọn gọi; giới hạn vòng lặp tool và validate arguments. Không cấp `execute_sql`, shell, web search hay file-system write cho stakeholder/coach trong MVP.

### 8.2. State và persistence

Các trường state dự kiến: operationId, sessionId, agentId, scenarioVersionId, message references, allowed knowledge references, selected evidence, memory summary, event references, response và validation result. Không có API key hoặc toàn bộ corpus knowledge.

- Thread key interview: `session:{id}:agent:{id}`; coach có namespace riêng; assessment dùng `submission:{id}:assessment:{runId}`.
- API tạo thread ID từ resource đã authorize; không cho client tự chọn namespace của người khác.
- Dùng checkpoint SQLite persistent cho demo một instance; không chỉ dùng in-memory saver rồi hứa resume sau restart.
- Transcript và memory đã được backend chấp nhận là authoritative. Checkpoint giữ state thực thi; lúc reconnect phải đối chiếu operation đã commit để tránh thêm message lần thứ hai.
- Memory riêng agent chỉ tóm tắt hội thoại của agent đó, facts được phép và event liên quan. Không tóm tắt tất cả transcript vào một shared summary.

LangGraph phân biệt checkpoint theo thread và store cho dữ liệu ngoài graph state; persistent checkpointer cần được chọn khi muốn giữ state qua restart. [Tài liệu persistence](https://docs.langchain.com/oss/python/langgraph/persistence).

### 8.3. Interview graph

```text
Validate request/authorized context (API)
-> Load persona state và relevant events
-> Apply deterministic knowledge/disclosure policy
-> Retrieve allowed facts
-> Build bounded context
-> Generate stakeholder JSON
-> Validate references, schema và allowed views
-> Nếu lỗi: tối đa 1 repair; vẫn lỗi thì operation Failed
-> Commit Message + revealed knowledge state + operation result (API)
-> Trả kết quả đã lọc
-> Queue coach nếu đủ ngưỡng; không chặn interview vì coach lỗi
```

LLM chỉ đề xuất các fact đã dùng. Việc đánh dấu IsRevealed phải kiểm tra fact nằm trong allowlist, thực sự được biểu đạt trong response và lưu RevealMessageId hợp lệ. Test schema/ID không đủ để chứng minh không lộ nội dung; cần thêm test hành vi với canary facts và kiểm tra thủ công.

### 8.4. Coach graph

```text
Load plan + transcript đã diễn ra + checklist
-> Tìm quan sát có dẫn chứng message
-> Sinh strengths / gaps / unclearPoints / improvementHint
-> Validate message IDs và giới hạn độ dài/hint
-> Lưu feedback
```

Ví dụ feedback phù hợp: “Bạn đã làm rõ bước xử lý hiện tại; tiêu chí đo mức độ thành công vẫn còn chưa cụ thể.” Feedback không phù hợp: “Hãy hỏi IT đúng câu X để biết đáp án Y” hoặc “Đây là bộ yêu cầu hoàn chỉnh để nộp”.

### 8.5. Assessment graph

```text
Load frozen artifact + plan/transcript/analysis snapshot + pinned rubric
-> Deterministic checks: đủ section, refs hợp lệ, required fields
-> LLM đánh giá từng criterion với evidence
-> Validate score ranges và evidence ownership
-> Tối đa 1 repair nếu output sai schema
-> Backend tính SuggestedScore theo weight
-> Persist AIPracticeFeedback Completed/Failed + operation usage
-> Chờ learner tạo Expert request, không tự tạo HumanReview
```

Đề xuất structured output bằng Pydantic và khả năng native của provider nếu có, hoặc tool strategy tương ứng; API vẫn phải validate business references sau parse. [Tài liệu structured output](https://docs.langchain.com/oss/python/langchain/structured-output).

Ví dụ feedback schema, không phải response API hoàn chỉnh:

```json
{
  "criterionCode": "TRACEABILITY",
  "score": 3,
  "maxScore": 5,
  "evidenceMessageIds": [101, 108],
  "analysisItemIds": [21],
  "artifactSectionKeys": ["requirements"],
  "observation": "Yêu cầu có nguồn dẫn chứng nhưng còn một acceptance criterion chưa gắn evidence.",
  "suggestion": "Làm rõ điều kiện kiểm thử và nguồn xác nhận.",
  "uncertainty": "medium"
}
```

Rubric tự biên soạn, cần Instructor/Expert duyệt; đây không phải bộ điểm chuẩn chính thức của IIBA:

| Criterion | Weight |
| --- | --- |
| Elicitation và xử lý thông tin chưa rõ | 20% |
| Phân tích, phát hiện mâu thuẫn và xác nhận | 20% |
| Chất lượng requirement / acceptance criteria | 25% |
| Business rule và constraint | 15% |
| Traceability / evidence | 10% |
| BRD completeness và consistency | 10% |

Backend tính `score_100 = sum(weight_percent * criterion_score / criterion_max_score)` và kiểm tra tổng weight = 100. Confidence của LLM chỉ là self-report/uncertainty indicator, không coi là xác suất đúng đã được hiệu chuẩn.

BABOK corpus cho coach là checklist/tóm tắt do nhóm được phép sử dụng và Expert duyệt. Không đưa nguyên sách có bản quyền vào vector store khi chưa có quyền; không gọi feedback là chứng nhận tuân thủ BABOK hay kết quả thi. [Giới thiệu BABOK từ IIBA](https://www.iiba.org/career-resources/a-business-analysis-professionals-foundation-for-success/babok/).

## 9. Timeline 20 ngày

Mỗi ngày kết thúc bằng contract/test cập nhật, không đợi tới thứ sáu mới tích hợp. A và B dùng mock adapter trong unit test, nhưng gate cuối tuần phải chạy bằng LLM thật cho các lượt đại diện.

### Tuần 1: Nền tảng và scenario package

| Ngày | A: Backend .NET | B: AI integration | Deliverable / tiêu chí hoàn thành |
| --- | --- | --- | --- |
| 1 | Audit code, bật SQL Server, init DB mới an toàn; kiểm tra auth trên SQL Server; chốt schema gaps, quyền và migration strategy | Dựng Python runtime skeleton/health; chốt contract API–runtime, dependency versions và credential handling | API + SQL Server + runtime cùng chạy; kế hoạch migration được review; không dùng EnsureCreated cho 9 module trên SQL Server |
| 2 | Seed users/roles, industry/application, scenario draft; DTO public/private, migration Partial/work style và BYOK connection routes tối thiểu | Chọn 1 provider/model bằng benchmark nhỏ; cấu hình BYOK, redaction; cùng BA soạn personas và 30 facts | Scenario package v1 và access matrix được duyệt; connection validate được, không persist key |
| 3 | Content routes + authoring aggregate + version/publish; template/rubric seed qua module owner | Embed facts/partial views; Qdrant local persisted index; retrieval theo version/persona và content hash | Instructor sửa draft/publish được; Learner không gọi được definition; index rebuild được |
| 4 | Session create/read, pin version, tạo SessionAgents/knowledge states; ownership checks | Persona prompt + bounded context + model adapter; test no-knowledge, role riêng, lời hỏi injection | Session mới có 3 agent; run standalone của từng agent đúng persona và knowledge boundary |
| 5 | Plan/question CRUD, expected outputs, Swagger/API examples; nối desktop test client | Interview graph baseline, structured response và schema validation; checkpoint namespace | Gate W1: Login -> chọn scenario -> đọc context -> tạo session -> lưu plan; ít nhất 1 interview smoke bằng LLM thật |

### Tuần 2: Interview, coach, events và analysis

| Ngày | A: Backend .NET | B: AI integration | Deliverable / tiêu chí hoàn thành |
| --- | --- | --- | --- |
| 6 | AI operations/worker, messaging routes, request idempotency và một lượt đang chạy/agent | Interview graph đầy đủ, allowlist retrieval, reveal proposals và validated output | POST message -> 202 -> poll -> transcript đã commit; gửi lại cùng key không tạo message mới |
| 7 | Transcript paging, reveal persistence, đọc coach feedback; cancellation/error contract | Coach workflow từ transcript/public context; per-agent summary, bounded history | Chuyển qua lại 3 stakeholder không trộn memory; coach nêu 1..3 quan sát, không lộ hidden facts |
| 8 | Time chunk policy, event trigger, unique trigger và session events DTO | Event effect overlay đúng agent; xử lý câu hỏi về thông tin vừa thay đổi | E1 thay đổi câu trả lời của stakeholder liên quan; event không chỉ là thông báo UI; trigger đồng thời chỉ ghi 1 lần |
| 9 | Analysis CRUD, evidence links, kiểm tra message/session match | Confirmation workflow, structured confirmed/disputed/insufficient-information outcome | Learner lưu yêu cầu có evidence và xác nhận được; yêu cầu sai bị disputed, không tự bật Confirmed |
| 10 | Tích hợp plan/interview/analysis; regression ownership, retry và resource state | Bộ câu hỏi chuẩn + injection probes; restart/reconcile test; chỉnh retrieval threshold | Gate W2: phỏng vấn 3 agent, nhận coach, gặp change event, phân tích và xác nhận; transcript resume được sau restart |

### Tuần 3: Artifact, submission và AI assessment

| Ngày | A: Backend .NET | B: AI integration | Deliverable / tiêu chí hoàn thành |
| --- | --- | --- | --- |
| 11 | Template endpoints, artifact create/read; define JSON schema/section keys với desktop | Build canonical assessment input từ section keys, refs và rubric; chuẩn bị 3 BRD mẫu | Desktop render template và tải draft; các section key giống nhau giữa editor, API và grader |
| 12 | Autosave + optimistic concurrency; freeze ArtifactVersion; không cho sửa version đã nộp | Deterministic validator cho section/evidence; assessor output schema | Save draft không đổi frozen version; hai editor stale update nhận conflict, không mất nội dung |
| 13 | Submission atomicity, rubric pin, pending feedback/operation, job polling và retry | Assessment graph qua rubric; scoring/references validation; capped repair | Nộp một lần tạo đúng submission + job; response nhanh với pending state; thiếu credential có trạng thái rõ |
| 14 | AI feedback/report routes; persist provider/model/prompt/snapshot hash/usage | Evaluate BRD tốt, trung bình, yếu; so sánh nhận xét của Expert; sửa rubric/prompt trước freeze | AI feedback có evidence thật; backend tự tính điểm; không để hallucinated ID đi vào report |
| 15 | ReviewRequests CRUD tối thiểu, queue/permission; tạo hồ sơ tổng hợp review | Chốt prompt/rubric versions; test artifact prompt injection và grading consistency | Gate W3: từ scenario tới BRD submit và AI feedback chạy thật; learner tạo được Expert request |

### Tuần 4: Expert, hardening và hội đồng

| Ngày | A: Backend .NET | B: AI integration | Deliverable / tiêu chí hoàn thành |
| --- | --- | --- | --- |
| 16 | Expert accept/complete, conditional claim, criterion validation và HumanReview; Instructor cohort policy nếu có P1 | Chuẩn bị evidence view cho Expert; phân biệt missing evidence với nội dung chưa đúng | Hai Expert cùng accept chỉ 1 người thắng; final score do backend tính từ điểm criterion của đúng reviewer |
| 17 | End-to-end SQL Server integration; report cuối; test user A/B, reviewer unauthorized, snapshot immutability | Failure tests: key sai, quota/rate limit, timeout, output sai, worker restart; đo usage | Luồng learner -> Expert hoàn chỉnh; lỗi AI không làm mất artifact/transcript; resume không nhân đôi dữ liệu |
| 18 | Kiểm thử tải nhỏ, concurrency, backup/restore; sửa P0 defects | Chạy golden set + adversarial eval trên model/config đã freeze; đo recall/latency/cost | Có báo cáo số liệu đo, không chỉ video happy path; không còn lỗi nghiêm trọng trong test suite |
| 19 | Feature freeze; deploy demo; seed/reset script cho demo DB riêng; OpenAPI + collection + release checklist | Preload embedding/index, verify provider key/quota; khóa prompt và model config; chuẩn bị recorded fallback | Release candidate, backup, demo account và hướng dẫn start/reset; không reset database chứa dữ liệu thật |
| 20 | Buffer sửa lỗi chặn demo; diễn tập 3 lần từ DB demo sạch; hỗ trợ hội đồng | Đối chiếu kết quả live với baseline; xác nhận fallback được gắn nhãn rõ | Demo 10..15 phút chạy lặp lại; có evidence, logs và known limitations; không thêm tính năng mới |

### Gate không được bỏ qua

- W1: scenario package được duyệt, publish/authorization đúng và ít nhất một lời đáp AI thật.
- W2: interview, coach, event, analysis và confirmation hoạt động cùng nhau.
- W3: BRD đã nộp immutable và AI feedback đã hoàn tất, có traceability.
- W4: Expert final review, regression suite, report và demo lặp lại từ dữ liệu sạch.

Nếu gate bị trễ hơn 1 ngày: bỏ P1 trước; tiếp theo bỏ SSE và dùng polling, giản lược số event/template; không bỏ ownership, knowledge isolation, immutable submission hoặc evidence validation.

## 10. Kiểm thử và tiêu chí nghiệm thu

Các con số bên dưới là **mục tiêu kiểm thử đề xuất**, chưa phải kết quả đạt được.

### API và dữ liệu

- Auth hiện có chạy thêm trên SQL Server thực; invalid JWT trả 401, sai role/ownership trả 403 hoặc 404 theo policy đã chốt.
- User A không đọc/sửa session, plan, messages, artifact, submission hay operation của user B.
- Learner không gọi được scenario definition, sửa rubric, trigger Instructor-only event hoặc gửi điểm HumanReview.
- Cross-module logical IDs được validate: evidence message, analysis item, persona, artifact version và rubric thuộc cùng phiên/scenario phù hợp.
- Published version và submitted ArtifactVersion không đổi sau authoring/autosave.
- Idempotency key conflict đúng; event không nhân đôi; hai refresh interview/job hay expert accept không tạo side effect đôi.
- Một worker chết sau LLM response nhưng trước/giữa persist không gây double transcript khi chạy lại.
- Autosave/retry không ghi đè dữ liệu mới; key hết hạn không khiến job âm thầm dùng key của user khác.

### AI evaluation

Golden set đề xuất: 30 câu hỏi nghiệp vụ có expected facts, 15 câu thử vượt quyền/đổi persona/prompt injection, 10 câu ngoài kiến thức và 3 BRD đã được Expert đánh giá sơ bộ.

- Retrieval: đo Recall@5 với câu hỏi có knowledge được phép; mục tiêu ban đầu >= 90% trên tập câu hỏi nhỏ, báo cáo cả số lượng và phương pháp tính.
- Knowledge isolation: 0 trường hợp canary fact bị tiết lộ trong bộ test đã định nghĩa; không gọi kết quả này là bảo đảm tuyệt đối ngoài bộ test.
- Grounding: 100% evidence IDs trong kết quả đã persist là references hợp lệ và đúng owner/session; kiểm tra thêm nội dung thực sự hỗ trợ nhận xét.
- Coach: trên các mẫu được review, không xuất hiện lời giải hoàn chỉnh, chỉ dẫn mở hidden fact hay câu hỏi áp đặt learner.
- Assessment: BRD mẫu tốt/trung bình/yếu được xếp đúng thứ tự theo review độc lập; mục tiêu chênh với baseline Expert trong khoảng được nhóm thống nhất, ví dụ 10/100 điểm. Ba mẫu chỉ là smoke calibration, không đủ chứng minh chất lượng chấm tổng quát.
- Chạy lặp vài lần để đo biến động; lưu prompt/model/input versions. Các lần chạy đã dùng token phải được tính usage kể cả retry thất bại.

### Hiệu năng và vận hành

- Đặt mục tiêu non-AI API p95 < 500 ms trên workload demo nhỏ; ghi rõ máy, số concurrent users và dataset khi báo cáo.
- Interview dùng 202 + status; mục tiêu thông thường hoàn tất trong khoảng 15 giây và timeout được xử lý rõ. Latency phụ thuộc provider/mạng, cần đo trước khi cam kết.
- Assessment có thể mất 30..120 giây; chạy background, không khóa editor hay chờ trong transaction SQL.
- Giới hạn một operation interview/agent; cap token input/output, retry tối đa, request rate limit và tổng số operation/session.
- Log correlation IDs, status, durations, provider/model, token counts, error codes. Không có key/password/access token trong log hay tracing.
- Smoke test start/stop API, SQL Server và runtime; embedding model/index đã tải sẵn để không download giữa buổi demo.

## 11. Rủi ro và phương án xử lý

| Rủi ro | Dấu hiệu sớm | Xử lý |
| --- | --- | --- |
| SQL Server/Docker chưa chạy | Ngày 1 không connect được | Giải quyết ngay; SQL Server integration là gate, không thay bằng SQLite rồi coi đã test production provider |
| Desktop/editor chậm | Ngày 5 chưa dùng được API contract | Mock UI/collection cho backend, giao section JSON sớm; báo rõ giới hạn nếu hội đồng chỉ xem API demo |
| AI nói sai persona/lộ facts | Golden set tuần 2 fail | Kiểm tra allowed content/context trước prompt; thêm canary tests; không chỉ tăng độ mạnh prompt |
| Local embedding chậm hoặc RAM không đủ | Benchmark ngày 2..3 không đạt | Chốt embedding API thay thế hoặc direct permitted context cho corpus nhỏ; giữ index/query tương thích |
| BYOK key/quota/mạng hết giữa demo | Connection validate hoặc smoke call fail | Kiểm tra trước buổi; tài khoản demo riêng; recorded transcript gắn nhãn replay, không giả vờ là AI live |
| Job mất sau restart | Không thấy pending job/resume | Durable operation state, lease recovery, reconciliation và RequiresCredential khi key không còn |
| Rubric/model thay đổi làm điểm khó giải thích | Kết quả không ghim version | Freeze phiên bản trước RC; lưu snapshot hash; Expert làm đánh giá cuối |
| Quá nhiều endpoint/CRUD | Gate tuần 1/2 trễ | Dùng aggregate authoring, một BRD và một provider; cắt toàn bộ P1 |
| Content scenario thiếu logic | Stakeholder trả lời mâu thuẫn ngoài chủ đích | BA/Instructor review facts, access matrix, event effects và rubric trong tuần 1 |

## 12. Kịch bản và gói bàn giao hội đồng

### Demo 10..15 phút

1. Instructor mở draft, thay một cấu hình và publish version; chỉ ra session pin version nào.
2. Learner Login, chọn scenario, đọc context và lưu BA plan đã chuẩn bị.
3. Hỏi một câu cho Operations, một câu cho Sales, rồi một câu đối chiếu với IT; thể hiện khác biệt persona và knowledge.
4. Xem coach feedback ngắn, chứng minh nó không viết lời giải.
5. Trigger một event hợp lệ; hỏi lại và thấy constraint/câu trả lời thay đổi.
6. Ghi một requirement/conflict với message evidence; xác nhận một proposition, minh họa cả agreed hoặc disputed.
7. Mở BRD draft đã nhập sẵn các phần dài, sửa một section, freeze version và nộp. Không gõ cả BRD trong thời gian demo.
8. Xem AI feedback theo criterion và nhấp evidence; bản nộp không đổi khi draft sửa tiếp.
9. Tạo Expert request; đăng nhập Expert, accept, xem AI nhận xét và chấm độc lập.
10. Learner xem báo cáo điểm AI gợi ý và điểm Expert cuối. Thử một request vượt quyền hoặc một knowledge probe để trình bày giới hạn truy cập.

AI call dài có trạng thái pending rõ. Có submission đã chấm sẵn để tiếp tục trình bày nếu mạng lỗi, nhưng phải nói rõ đó là bản chuẩn bị, không phải kết quả vừa chạy live.

### Gói bàn giao

- Source API/runtime, dependency lock/version và cấu hình không chứa secret.
- Migration/init scripts, seed demo idempotent và reset chỉ dành cho database demo đã xác minh tên/path.
- OpenAPI, HTTP/Postman collection và scenario runner cho luồng xuyên suốt.
- Scenario package: business brief, access matrix, disclosure rules, event effects, template và rubric.
- Prompt profiles/version, embedding/index revision, structured schemas và sample output đã redact.
- Integration/evaluation reports, environment specs, latency/usage measurements và known limitations.
- Demo script, tài khoản role đã seed, backup và hướng dẫn start/reconnect BYOK.

## 13. Khi chỉ có một người Backend + AI

Giữ luồng đầy đủ về mặt học tập nhưng giảm bề rộng triển khai:

- Seed một scenario đã duyệt, authoring bằng aggregate import/CLI; không làm giao diện/API quản trị nhiều bảng.
- 3 stakeholder, một event có tác động thật, một BRD, một rubric, một provider/model.
- Dùng direct permitted context làm baseline cho 30 facts; embedding/RAG chỉ thêm sau khi interview đúng và không lộ knowledge. Không cần vector server riêng.
- Coach theo yêu cầu hoặc mỗi 3 lượt, không real-time mỗi message; polling thay SSE.
- Expert được gán thủ công cho một request; không matching marketplace, cohort CRUD hoặc chat Expert.
- Tuần 1: SQL/auth/seed/session/plan + runtime skeleton. Tuần 2: interview/coach/analysis/confirmation. Tuần 3: BRD/submission/AI assessment + Expert tối thiểu. Tuần 4: regression, integration, report và demo.
- Nếu tới cuối tuần 2 chưa có interview thật và evidence flow: điều chỉnh phạm vi hoặc deadline, không hứa hoàn thành hệ thống đa role đầy đủ trong 4 tuần.

## 14. Thứ tự bắt đầu ngay

Ngày 1 ưu tiên: **SQL Server hoạt động -> chốt scenario package và access matrix -> chốt schema gaps/API contract -> AI runtime smoke call**. Không bắt đầu bằng viết toàn bộ CRUD hoặc dựng một bộ agent tổng quát.

Định nghĩa hoàn thành của tháng này: **một learner có thể thực hành trọn scenario, dữ liệu và evidence được lưu đúng, AI hỗ trợ đúng giới hạn, và Expert có thể đưa ra đánh giá cuối cùng có thể kiểm tra lại**.
