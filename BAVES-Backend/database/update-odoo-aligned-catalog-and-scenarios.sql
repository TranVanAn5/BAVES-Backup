/*
  Align Industries / Applications with Odoo-style verticals:
    RETAIL        → Odoo Retail (Sales, POS, eCom, Loyalty, store purchase)
    SUPPLY_CHAIN  → Odoo Inventory / Logistics (WMS) — NOT retail
    FINANCE       → Finance / FinTech lending software

  Move OMS under RETAIL. Refresh Scenario + ScenarioVersion copy toward IT-BA
  (software requirements for industry systems).

  Safe to re-run (idempotent UPDATEs).
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

/* -------------------------------------------------------------------------- */
/* 1) Industries                                                              */
/* -------------------------------------------------------------------------- */
UPDATE [content].[Industries]
SET [Name] = N'Bán lẻ (Retail)',
    [Description] = N'Odoo Retail: bán lẻ đa kênh, POS, eCommerce, loyalty và vận hành cửa hàng. Ứng dụng: OMS, Promo/Loyalty, Store Ops.'
WHERE [Code] = N'RETAIL';

IF NOT EXISTS (SELECT 1 FROM [content].[Industries] WHERE [Code] = N'RETAIL')
BEGIN
    INSERT INTO [content].[Industries] ([Code], [Name], [Description], [Status], [DisplayOrder], [CreatedAt])
    VALUES (N'RETAIL', N'Bán lẻ (Retail)', N'Odoo Retail: bán lẻ đa kênh, POS, eCommerce, loyalty và vận hành cửa hàng.', N'Active', 1, SYSDATETIMEOFFSET());
END;

UPDATE [content].[Industries]
SET [Name] = N'Chuỗi cung ứng & Kho vận (Supply Chain)',
    [Description] = N'Odoo Inventory / Logistics: WMS, cold chain, IoT kho — không bao gồm bán lẻ omnichannel.'
WHERE [Code] = N'SUPPLY_CHAIN';

IF NOT EXISTS (SELECT 1 FROM [content].[Industries] WHERE [Code] = N'SUPPLY_CHAIN')
BEGIN
    INSERT INTO [content].[Industries] ([Code], [Name], [Description], [Status], [DisplayOrder], [CreatedAt])
    VALUES (N'SUPPLY_CHAIN', N'Chuỗi cung ứng & Kho vận (Supply Chain)', N'Odoo Inventory / Logistics: WMS, cold chain, IoT kho.', N'Active', 2, SYSDATETIMEOFFSET());
END;

UPDATE [content].[Industries]
SET [Name] = N'Tài chính & FinTech (Finance)',
    [Description] = N'Finance / lending software: Digital LOS, eKYC, decision engine, tích hợp CIC.'
WHERE [Code] = N'FINANCE';

IF NOT EXISTS (SELECT 1 FROM [content].[Industries] WHERE [Code] = N'FINANCE')
BEGIN
    INSERT INTO [content].[Industries] ([Code], [Name], [Description], [Status], [DisplayOrder], [CreatedAt])
    VALUES (N'FINANCE', N'Tài chính & FinTech (Finance)', N'Finance / lending software: Digital LOS, eKYC, decision engine.', N'Active', 3, SYSDATETIMEOFFSET());
END;

DECLARE @RetailId int = (SELECT [Id] FROM [content].[Industries] WHERE [Code] = N'RETAIL');
DECLARE @ScId int = (SELECT [Id] FROM [content].[Industries] WHERE [Code] = N'SUPPLY_CHAIN');
DECLARE @FinId int = (SELECT [Id] FROM [content].[Industries] WHERE [Code] = N'FINANCE');

/* -------------------------------------------------------------------------- */
/* 2) Applications — names + Odoo industry ownership                          */
/* -------------------------------------------------------------------------- */
-- OMS belongs to RETAIL (was wrongly under SUPPLY_CHAIN)
UPDATE a
SET a.[IndustryId] = @RetailId,
    a.[Name] = N'OMS — Order Management (Retail Sales)',
    a.[Description] = N'Odoo Retail / Sales + Inventory: OMS đa kênh, đồng bộ tồn & trạng thái đơn (website, app, cửa hàng).'
FROM [content].[Applications] a
WHERE a.[Code] = N'OMS';

IF NOT EXISTS (SELECT 1 FROM [content].[Applications] WHERE [Code] = N'OMS')
BEGIN
    INSERT INTO [content].[Applications] ([IndustryId], [Code], [Name], [Description], [Status], [DisplayOrder], [CreatedAt])
    VALUES (@RetailId, N'OMS', N'OMS — Order Management (Retail Sales)', N'OMS đa kênh Retail.', N'Active', 1, SYSDATETIMEOFFSET());
END;

UPDATE a
SET a.[IndustryId] = @RetailId,
    a.[Name] = N'Loyalty & Promotion Engine',
    a.[Description] = N'Odoo Retail Loyalty/POS: central promo engine, voucher realtime, điểm thưởng đa kênh.'
FROM [content].[Applications] a
WHERE a.[Code] = N'LMS_PROMO';

UPDATE a
SET a.[IndustryId] = @RetailId,
    a.[Name] = N'Store Ops & Purchasing Checklist',
    a.[Description] = N'Odoo Retail / Purchase nhẹ: checklist đặt hàng cửa hàng F&B, workflow đề xuất–duyệt–gửi NCC.'
FROM [content].[Applications] a
WHERE a.[Code] = N'CAFE_OPS';

UPDATE a
SET a.[IndustryId] = @ScId,
    a.[Name] = N'WMS Cold Chain',
    a.[Description] = N'Odoo Inventory / Warehouse: WMS kho lạnh, FEFO, QC hold, tích hợp IoT nhiệt độ.'
FROM [content].[Applications] a
WHERE a.[Code] = N'WMS_COLD';

UPDATE a
SET a.[IndustryId] = @FinId,
    a.[Name] = N'Digital LOS & Underwriting',
    a.[Description] = N'Finance lending software: Digital LOS, eKYC, decision engine, tích hợp CIC.'
FROM [content].[Applications] a
WHERE a.[Code] = N'DIGITAL_LOS';

/* -------------------------------------------------------------------------- */
/* Helper: update published ScenarioVersion by scenario code                  */
/* -------------------------------------------------------------------------- */

/* OMNI-001 */
UPDATE s
SET s.[Title] = N'OmniMart — Đồng bộ OMS đa kênh (Retail / Sales)',
    s.[Description] = N'IT-BA đặc tả cải tiến OMS cho chuỗi bán lẻ OmniMart (Odoo Retail): đồng bộ tồn, trạng thái đơn và API giữa eCommerce, POS và kho.'
FROM [scenario].[Scenarios] s WHERE s.[Code] = N'OMNI-001';

UPDATE v
SET
    v.[Summary] = N'Đặc tả phần mềm OMS đa kênh để đồng bộ trạng thái đơn, reservation tồn kho và thông báo khách giữa website, app và 25 cửa hàng OmniMart (Odoo Retail).',
    v.[BusinessContext] = N'OmniMart là chuỗi bán lẻ đồ gia dụng (Odoo industry: Retail) với 25 cửa hàng, website và app. Hệ thống mục tiêu gần mô hình Odoo Sales + Inventory + eCommerce: tiếp nhận đơn đa kênh, giữ/nhả tồn, cập nhật fulfillment. Hiện website, POS cửa hàng và file tồn DC Củ Chi chưa dùng một OMS thống nhất — dữ liệu lệch pha, CSKH và IT phải đối soát thủ công.',
    v.[BusinessProblem] = N'1. Lệch tồn ảo giữa eCommerce và POS: website còn “Available” nhưng DC/cửa hàng đã hết — hủy đơn muộn.
2. Reservation/release tồn không thống nhất giữa kênh (timeout, hold thủ công) → giam tồn ảo.
3. Trạng thái đơn (order state machine) khác nhau giữa web (EN) và POS (VI); không có API/event chuẩn.
4. Hoàn tiền/hủy đơn thiếu workflow phần mềm rõ (approval, audit log) — mâu thuẫn Ops / Finance / IT.',
    v.[BusinessObjectives] = N'1. Đặc tả OMS to-be: một catalog trạng thái đơn và reservation policy dùng chung.
2. Định nghĩa API/events đồng bộ tồn & trạng thái (NFR: latency, idempotency).
3. Giảm hủy đơn do lệch tồn; rút ngắn hoàn tiền có audit trail trên hệ thống.
4. Bàn giao BRD + SRS + User Stories + RTM cho đội phát triển (không code thay ERP lõi).',
    v.[BARole] = N'Bạn là IT Business Analyst (Retail Systems) — thiên về phần mềm: khảo sát tích hợp Sales/Inventory, phỏng vấn Ops, CSKH và IT Lead, đặc tả OMS cải tiến theo hướng Odoo Retail apps (Sales, Inventory, Website).',
    v.[Assignment] = N'Sprint 1: BAP + stakeholder map (Ops, Sales/CS, IT) + as-is system landscape (web, POS, DC stock).
Sprint 2: Elicitation — order state machine, reservation rules; BRD v1 + Business Rules.
Sprint 3: Biến cố kỹ thuật/đồng bộ; SRS v1 (API sync, UI ops tối thiểu) + User Stories/AC.
Sprint 4: Impact analysis; SRS v2 + RTM đầu–cuối cho backlog phát triển.',
    v.[InScope] = N'- Order lifecycle đa kênh trên OMS: create → pay/confirm → reserve → pack → ship → complete/cancel/refund.
- Data model & API đồng bộ tồn/trạng thái; NFR cơ bản; audit log hủy/hoàn tiền.
- Hồ sơ BABOK: BAP, BRD, SRS, User Stories, RTM.',
    v.[OutOfScope] = N'- Viết code / thay toàn bộ POS hoặc ERP kế toán lõi.
- Đàm phán hợp đồng vận chuyển / cổng thanh toán.
- Loyalty phức tạp (thuộc application LMS_PROMO).',
    v.[KnownConstraints] = N'- Giữ POS hiện tại; OMS phải tích hợp qua API/file sync giai đoạn đầu.
- Ngân sách giai đoạn 1 & 4 sprint khảo sát; không downtime bán hàng cửa hàng.
- Ưu tiên giải pháp phần mềm có thể map sang module Sales/Inventory kiểu Odoo.'
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE s.[Code] = N'OMNI-001' AND v.[VersionNo] = 1;

/* BEG-OMS-01 */
UPDATE s
SET s.[Title] = N'OmniMart Starter — Chuẩn hóa order status trên OMS',
    s.[Description] = N'Beginner IT-BA: đặc tả catalog trạng thái đơn và màn hình/API tối thiểu cho OMS bán lẻ (Odoo Retail / Sales).'
FROM [scenario].[Scenarios] s WHERE s.[Code] = N'BEG-OMS-01';

UPDATE v
SET
    v.[Summary] = N'Chuẩn hóa danh mục trạng thái đơn trên OMS giữa website và cửa hàng OmniMart Starter; giảm ticket “đơn đang ở đâu?”.',
    v.[BusinessContext] = N'OmniMart Starter (ngành Retail theo Odoo) có website, app và 3 cửa hàng. Đơn online và tại quầy chưa dùng chung order status trên OMS: web hiển thị EN (New/Paid/Shipping/Done), cửa hàng ghi VI trên Excel/chat. Học viên đóng vai IT-BA làm rõ yêu cầu phần mềm tối thiểu (catalog trạng thái + trách nhiệm cập nhật + user story UI/API).',
    v.[BusinessProblem] = N'1) Hai hệ thống đặt tên trạng thái khác nhau — khách/CSKH hiểu lệch. 2) ~35% ticket hỏi trạng thái; CSKH gọi cửa hàng thủ công vì không có nguồn sự thật trên OMS. 3) Chưa có spec danh mục trạng thái + field cập nhật cho backlog dev.',
    v.[BusinessObjectives] = N'1) As-is/to-be order status catalog dùng chung. 2) Xác định actor cập nhật từng state (system vs user). 3) 3 user stories + AC cho màn hình/API trạng thái. 4) Deliverable Beginner IT-BA.',
    v.[BARole] = N'Junior IT Business Analyst cho sản phẩm OMS Retail — thiên về yêu cầu phần mềm (data field, UI ops, quy ước API), không chỉ quy trình giấy.',
    v.[Assignment] = N'Sprint 1: BAP ngắn + stakeholder map (Ops, CSKH, Store) + system context. Sprint 2: Phỏng vấn; notes về status & nguồn cập nhật. Sprint 3: State catalog as-is/to-be + RACI cập nhật. Sprint 4: 3 user stories + AC (UI hoặc API status).',
    v.[InScope] = N'- Order status lifecycle cơ bản trên OMS. - Catalog trạng thái, actor, field tối thiểu. - BAP, stakeholder map, notes, as-is/to-be, 3 US + AC.',
    v.[OutOfScope] = N'- Payment gateway, loyalty, routing kho phức tạp, BRD/SRS đầy đủ Intermediate.',
    v.[KnownConstraints] = N'- Không thay POS lõi; ưu tiên spec đồng bộ. - Số liệu giả lập đào tạo.'
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE s.[Code] = N'BEG-OMS-01' AND v.[VersionNo] = 1;

/* Ensure BEG-OMS / OMNI scenarios point at OMS under RETAIL */
UPDATE sc
SET sc.[ApplicationId] = a.[Id]
FROM [scenario].[Scenarios] sc
INNER JOIN [content].[Applications] a ON a.[Code] = N'OMS' AND a.[IndustryId] = @RetailId
WHERE sc.[Code] IN (N'OMNI-001', N'BEG-OMS-01');

/* SCM-001 */
UPDATE s
SET s.[Title] = N'AquaCold — Đặc tả WMS Cold Chain (Supply Chain / Inventory)',
    s.[Description] = N'IT-BA đặc tả WMS kho lạnh tích hợp IoT cho 3PL AquaCold — hướng Odoo Inventory/Warehouse.'
FROM [scenario].[Scenarios] s WHERE s.[Code] = N'SCM-001';

UPDATE v
SET
    v.[Summary] = N'Đặc tả phần mềm WMS cold-chain (putaway, FEFO, cross-dock, QC hold) và tích hợp cảm biến IoT realtime cho AquaCold 3PL.',
    v.[BusinessContext] = N'AquaCold vận hành 3 DC lạnh và 150 xe lạnh (Odoo industry: Supply Chain / Inventory & Logistics — không phải Retail). WMS hiện tại + giấy tay + Zalo cảnh báo nhiệt chưa thành hệ thống tích hợp. Mục tiêu: đặc tả WMS/IoT tương thích tư duy module Inventory/Barcode của Odoo, cộng NFR cho môi trường -25°C và GSP/GDP.',
    v.[BusinessProblem] = N'1. Không có workflow inbound phần mềm: pallet chờ sàn đệm quá lâu → excursion nhiệt.
2. Thiếu end-to-end temperature trace trên hệ thống (kho ↔ xe) → tranh chấp QA/Fleet không chứng minh được.
3. Location accuracy thấp vì putaway thủ công; WMS chưa directed putaway/barcode.
4. Cross-dock không có matching engine — double handling.',
    v.[BusinessObjectives] = N'1. Đặc tả WMS inbound/putaway/picking/FEFO/dispatch + QC Hold tự động theo ngưỡng IoT.
2. Định nghĩa integration IoT → WMS/e-POD (frequency, alert levels, immutable log).
3. NFR: rugged device, connectivity trong panel kho, retention dữ liệu ≥ 5 năm.
4. Bàn giao BRD/SRS/US/RTM cho đội phát triển WMS.',
    v.[BARole] = N'Bạn là IT Business Analyst (Supply Chain Systems) — tập trung yêu cầu phần mềm WMS/IoT, phỏng vấn DC Ops, QA, Fleet và WMS/IoT Engineer; bàn giao spec cho software delivery.',
    v.[Assignment] = N'Sprint 1: BAP + landscape (WMS, IoT, handheld) + stakeholder map.
Sprint 2: Elicitation; BRD v1 + rules (QC Hold, FEFO, excursion).
Sprint 3: Sự cố mất điện/nhiệt; SRS v1 (alert workflow, directed putaway, IoT API).
Sprint 4: Impact + SRS v2 + RTM.',
    v.[InScope] = N'- WMS cold-chain flows; IoT alert & traceability; barcode/QR location; cross-dock matching rules.
- Hồ sơ BABOK cho phát triển phần mềm.',
    v.[OutOfScope] = N'- Xây kho mới / mua xe; firmware sensor; đổi bảng giá 3PL.',
    v.[KnownConstraints] = N'- Thiết bị chịu lạnh; sóng yếu trong panel; tuân thủ GSP/GDP lưu trữ dữ liệu.'
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE s.[Code] = N'SCM-001' AND v.[VersionNo] = 1;

/* BEG-WMS-01 */
UPDATE s
SET s.[Title] = N'AquaCold Starter — Alert & incident log trên WMS lạnh',
    s.[Description] = N'Beginner IT-BA: đặc tả mức cảnh báo nhiệt độ và form nhật ký sự cố tối thiểu trong WMS cold-chain.'
FROM [scenario].[Scenarios] s WHERE s.[Code] = N'BEG-WMS-01';

UPDATE v
SET
    v.[Summary] = N'Làm rõ Info/Warning/Critical temperature alerts và data fields nhật ký sự cố cho WMS kho đông AquaCold Starter.',
    v.[BusinessContext] = N'AquaCold Starter (ngành Supply Chain / kho vận theo Odoo Inventory) có 1 kho đông ~800 pallet, IoT đo 5 phút/lần, cảnh báo đổ vào Zalo chung. Học viên là IT-BA: đặc tả phần mềm alert levels + incident log fields gắn WMS — không thiết kế routing xe.',
    v.[BusinessProblem] = N'1) Mọi alert giống nhau — không có severity trên hệ thống. 2) Không có schema nhật ký sự cố. 3) Ca đêm thiếu SLA acknowledge trên phần mềm.',
    v.[BusinessObjectives] = N'1) As-is/to-be luồng nhận alert. 2) Bảng severity + ngưỡng. 3) 3 US + AC cho incident log UI/API. 4) Deliverable Beginner IT-BA.',
    v.[BARole] = N'Junior IT-BA cho WMS/IoT module — yêu cầu phần mềm cảnh báo & nhật ký, không thuần quy trình giấy.',
    v.[Assignment] = N'Sprint 1: BAP + map (Supervisor, QA, Night Lead) + system context IoT→channel. Sprint 2: Phỏng vấn ngưỡng & phản ứng. Sprint 3: Severity matrix + acknowledge flow. Sprint 4: 3 US + AC incident log.',
    v.[InScope] = N'- Temperature alerts 1 kho; severity; acknowledge; incident fields. - Beginner BA deliverables.',
    v.[OutOfScope] = N'- Full WMS redesign, fleet routing, legal SLA 3PL phức tạp.',
    v.[KnownConstraints] = N'- Giữ IoT hiện có; Zalo tạm nếu chưa có app; số liệu giả lập.'
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE s.[Code] = N'BEG-WMS-01' AND v.[VersionNo] = 1;

UPDATE sc
SET sc.[ApplicationId] = a.[Id]
FROM [scenario].[Scenarios] sc
INNER JOIN [content].[Applications] a ON a.[Code] = N'WMS_COLD'
WHERE sc.[Code] IN (N'SCM-001', N'BEG-WMS-01');

/* FIN-001 */
UPDATE s
SET s.[Title] = N'FinNova — Digital LOS & eKYC Decision Engine (Finance)',
    s.[Description] = N'IT-BA đặc tả Digital LOS: eKYC liveness, multi-tier decision funnel, tích hợp CIC API (Finance / FinTech).'
FROM [scenario].[Scenarios] s WHERE s.[Code] = N'FIN-001';

UPDATE v
SET
    v.[Summary] = N'Đặc tả phần mềm Digital LOS: eKYC Liveness, decision engine STP, phễu lọc API bên thứ ba cho FinNova (Finance).',
    v.[BusinessContext] = N'FinNova cho vay tiêu dùng số qua app (Odoo industry analogy: Finance — quy trình tín dụng số, không phải Retail/Inventory). Hệ thống LOS hiện ghép module mua ngoài + thẩm định thủ công. IT-BA đặc tả to-be: customer journey số, Decision Engine, tích hợp eKYC/CIC với NFR bảo mật PDPD.',
    v.[BusinessProblem] = N'1. eKYC yếu → fraud deepfake/CCCD giả; thiếu rule engine chống gian lận trên LOS.
2. STP thấp (~22%): UI/ops queue thiếu dữ liệu quyết định; thiếu multi-tier funnel phần mềm.
3. False positive gig workers do rule cứng trên hệ thống cũ.
4. Gọi CIC sớm gây cost/latency — cần orchestration API có tầng lọc.',
    v.[BusinessObjectives] = N'1. Đặc tả LOS journey + Decision Matrix + hold/reject taxonomy.
2. Spec tích hợp eKYC Liveness & CIC (timeout, retry, cost control).
3. NFR: PII encryption, SLA < 5 phút STP, audit trail.
4. BRD/SRS/US/RTM cho đội phát triển FinTech.',
    v.[BARole] = N'Bạn là IT Business Analyst (FinTech / Lending Systems) — thiên về kiến trúc luồng phần mềm, API, decision rules và tuân thủ dữ liệu; làm việc với Risk, Fraud, Legal, Enterprise Architect.',
    v.[Assignment] = N'Sprint 1: BAP + system landscape LOS + stakeholder map.
Sprint 2: Elicitation; BRD v1 + credit/fraud business rules.
Sprint 3: Deepfake swarm event; SRS v1 (eKYC, decision engine, ops UI).
Sprint 4: CIC outage / rate-limit impact; SRS v2 + RTM.',
    v.[InScope] = N'- Digital lending journey; decision funnel; eKYC/CIC integration contracts; ops screens.
- BABOK software deliverables.',
    v.[OutOfScope] = N'- Thay core banking/accounting; train ML scorecard; field collection.',
    v.[KnownConstraints] = N'- NHNN / PDPD; AES-256 / TLS; decision engine < 3s nội bộ.'
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE s.[Code] = N'FIN-001' AND v.[VersionNo] = 1;

/* BEG-LOS-01 */
UPDATE s
SET s.[Title] = N'FinNova Starter — Hold reason trên Digital LOS',
    s.[Description] = N'Beginner IT-BA: đặc tả taxonomy hold eKYC và UI/API Ops trên Digital LOS.'
FROM [scenario].[Scenarios] s WHERE s.[Code] = N'BEG-LOS-01';

UPDATE v
SET
    v.[Summary] = N'Làm rõ 5 bước eKYC và hold reason fields trên Digital LOS FinNova Starter để Ops xử lý đúng SLA.',
    v.[BusinessContext] = N'FinNova Starter (ngành Finance) chạy Digital LOS: CCCD + selfie, ~70% auto-pass, còn lại hold. Credit Ops ~120–150 hồ sơ/ngày, SLA 4 giờ. Học viên IT-BA đặc tả taxonomy hold + màn hình Ops — thiên phần mềm, không scoring nâng cao.',
    v.[BusinessProblem] = N'1) UI Ops không show hold reason — phải đọc log. 2) Onboarding gọi trùng vì thiếu field chuẩn. 3) Chưa có spec taxonomy cho backlog.',
    v.[BusinessObjectives] = N'1) As-is 5 bước eKYC. 2) Taxonomy hold + fields tối thiểu. 3) 3 US + AC UI Ops. 4) Beginner IT-BA deliverables.',
    v.[BARole] = N'Junior IT-BA cho Digital LOS — yêu cầu phần mềm màn hình/API hold reason.',
    v.[Assignment] = N'Sprint 1: BAP + map (Ops, Onboarding, Compliance). Sprint 2: Phỏng vấn SLA & lý do hold. Sprint 3: eKYC as-is + taxonomy. Sprint 4: 3 US + AC.',
    v.[InScope] = N'- eKYC CCCD/selfie hold/reject; taxonomy; Ops UI fields. - Beginner deliverables.',
    v.[OutOfScope] = N'- ML scoring, deepfake advanced, core banking thay thế.',
    v.[KnownConstraints] = N'- Không PII thật; không đổi vendor eKYC; số liệu giả lập.'
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE s.[Code] = N'BEG-LOS-01' AND v.[VersionNo] = 1;

UPDATE sc
SET sc.[ApplicationId] = a.[Id]
FROM [scenario].[Scenarios] sc
INNER JOIN [content].[Applications] a ON a.[Code] = N'DIGITAL_LOS'
WHERE sc.[Code] IN (N'FIN-001', N'BEG-LOS-01');

/* RET-001 */
UPDATE s
SET s.[Title] = N'MegaFashion — Loyalty & Promo Engine (Retail)',
    s.[Description] = N'IT-BA đặc tả Central Promotion/Loyalty Engine đồng bộ realtime với POS và eCommerce (Odoo Retail).'
FROM [scenario].[Scenarios] s WHERE s.[Code] = N'RET-001';

UPDATE v
SET
    v.[Summary] = N'Đặc tả phần mềm Promo/Loyalty Engine realtime — chống double-redeem voucher, sync điểm POS↔App cho MegaFashion (Retail).',
    v.[BusinessContext] = N'MegaFashion (Odoo Retail): 65 showroom, web, MegaClub app. Loyalty đang gắn POS cũ, Promo Engine nằm cloud eCom — hai hệ không realtime. IT-BA đặc tả central engine + API POS/App theo hướng module Loyalty/POS của Odoo.',
    v.[BusinessProblem] = N'1. Single-use voucher redeem kép do thiếu lock/validation realtime trên engine.
2. Point accrual trễ 4–12h — không có event sync chuẩn.
3. Return/refund sai điểm vì thiếu line-item allocation logic trên phần mềm.
4. POS chậm khi evaluate stack rule phức tạp — cần NFR + rule engine spec.',
    v.[BusinessObjectives] = N'1. Spec validation voucher realtime (<1.5s) + idempotent redeem.
2. API accrual/redeem đồng bộ App–POS ≤ 60s.
3. Thuật toán hoàn điểm khi return; audit tài chính.
4. BRD/SRS/US/RTM cho đội phát triển.',
    v.[BARole] = N'Bạn là IT Business Analyst (Retail Commerce Systems) — tập trung rule engine, API POS, data consistency; làm việc Marketing, Finance, Store Ops, IT Lead.',
    v.[Assignment] = N'Sprint 1: BAP + landscape (POS, Promo cloud, App) + stakeholders.
Sprint 2: Elicitation rules; BRD v1.
Sprint 3: Flash-sale fraud event; SRS v1 (API, AC Gherkin).
Sprint 4: POS offline impact; SRS v2 + RTM.',
    v.[InScope] = N'- Voucher/points lifecycle; realtime validation; return allocation; POS/API contracts.',
    v.[OutOfScope] = N'- Đổi hardware POS; hoa hồng NV; SMS gateway contracts.',
    v.[KnownConstraints] = N'- POS cũ, mạng chập chờn; discount budget ≤ 8% GMV; POS evaluate ≤ 10s/hóa đơn.'
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE s.[Code] = N'RET-001' AND v.[VersionNo] = 1;

/* BEG-PROMO-01 */
UPDATE s
SET s.[Title] = N'MegaFashion Starter — Spec điều kiện voucher trên Promo Engine',
    s.[Description] = N'Beginner IT-BA: đặc tả rule voucher GIẢM50K và AC đồng bộ banner/app/POS.'
FROM [scenario].[Scenarios] s WHERE s.[Code] = N'BEG-PROMO-01';

UPDATE v
SET
    v.[Summary] = N'Làm rõ điều kiện voucher GIẢM50K trên Promotion Engine vs banner/POS; viết AC phần mềm.',
    v.[BusinessContext] = N'MegaFashion Starter (Retail) chạy voucher GIẢM50K. Banner nói “mọi SP”, engine chỉ category Áo; POS đôi khi lỗi khác app. IT-BA đặc tả rule fields + AC — thiên phần mềm cấu hình promo.',
    v.[BusinessProblem] = N'1) Banner ≠ engine config. 2) Ba bên hiểu rule khác nhau. 3) Chưa có AC cho backlog cấu hình voucher.',
    v.[BusinessObjectives] = N'1) Bảng rule as-is/to-be. 2) Chỉ ra lệch kênh. 3) 3 US/AC. 4) Beginner IT-BA deliverables.',
    v.[BARole] = N'Junior IT-BA cho Promo Engine Retail — spec cấu hình phần mềm voucher.',
    v.[Assignment] = N'Sprint 1: BAP + map (Marketing Ops, POS, CX). Sprint 2: Phỏng vấn lệch rule. Sprint 3: Rule table. Sprint 4: 3 US + AC.',
    v.[InScope] = N'- Một voucher GIẢM50K: min order, category, stacking, limit. - Sync mô tả điều kiện.',
    v.[OutOfScope] = N'- Full loyalty points engine, AI personalization.',
    v.[KnownConstraints] = N'- Không đổi giá gốc SKU; FREESHIP không stack với GIẢM50K.'
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE s.[Code] = N'BEG-PROMO-01' AND v.[VersionNo] = 1;

UPDATE sc
SET sc.[ApplicationId] = a.[Id]
FROM [scenario].[Scenarios] sc
INNER JOIN [content].[Applications] a ON a.[Code] = N'LMS_PROMO'
WHERE sc.[Code] IN (N'RET-001', N'BEG-PROMO-01');

/* BEG-CAFE-01 */
UPDATE s
SET s.[Title] = N'BeanCounter Café — App checklist đặt hàng (Retail Store Ops)',
    s.[Description] = N'Beginner IT-BA: đặc tả ứng dụng checklist đặt nguyên liệu tuần cho cửa hàng F&B bán lẻ (Odoo Retail / Purchase).'
FROM [scenario].[Scenarios] s WHERE s.[Code] = N'BEG-CAFE-01';

UPDATE v
SET
    v.[Summary] = N'Đặc tả phần mềm checklist nhu cầu tuần + workflow đề xuất/duyệt/gửi NCC cho BeanCounter Café (Retail).',
    v.[BusinessContext] = N'BeanCounter Café là cửa hàng F&B độc lập xếp ngành Retail (Odoo Retail — Purchase/Inventory cửa hàng nhỏ). Hiện đặt sữa/đậu/ly qua Zalo + giấy dán tủ lạnh + Excel kế toán. IT-BA thiết kế yêu cầu app/checklist số hóa tối thiểu (không full ERP).',
    v.[BusinessProblem] = N'1) Không có nguồn nhu cầu tuần trên hệ thống → thiếu hàng giữa tuần. 2) Đặt trùng vì không share checklist số. 3) Chưa có spec workflow đề xuất–duyệt–gửi NCC trên phần mềm.',
    v.[BusinessObjectives] = N'1) As-is/to-be quy trình đặt hàng + data fields checklist. 2) RACI trên hệ thống. 3) 3 US + AC cho app checklist. 4) Beginner IT-BA deliverables.',
    v.[BARole] = N'Junior IT-BA cho Retail store ops software — thiên yêu cầu app checklist/purchase nhẹ, không đàm phán hợp đồng NCC.',
    v.[Assignment] = N'Sprint 1: BAP + map (Quán trưởng, Barista, Kế toán) + system context. Sprint 2: Phỏng vấn. Sprint 3: Checklist data model + workflow. Sprint 4: 3 US + AC.',
    v.[InScope] = N'- Checklist nhu cầu tuần; workflow đề xuất/duyệt/gửi; fields tối thiểu. - Beginner deliverables.',
    v.[OutOfScope] = N'- App đặt hàng NCC full marketplace; mở chi nhánh; đổi hợp đồng NCC.',
    v.[KnownConstraints] = N'- 1 cơ sở; ưu tiên mobile-web đơn giản; số liệu giả lập.'
FROM [scenario].[ScenarioVersions] v
INNER JOIN [scenario].[Scenarios] s ON s.[Id] = v.[ScenarioId]
WHERE s.[Code] = N'BEG-CAFE-01' AND v.[VersionNo] = 1;

UPDATE sc
SET sc.[ApplicationId] = a.[Id]
FROM [scenario].[Scenarios] sc
INNER JOIN [content].[Applications] a ON a.[Code] = N'CAFE_OPS'
WHERE sc.[Code] = N'BEG-CAFE-01';

COMMIT TRAN;

SELECT i.[Code] AS Industry, i.[Name], a.[Code] AS AppCode, a.[Name] AS AppName
FROM [content].[Applications] a
INNER JOIN [content].[Industries] i ON i.[Id] = a.[IndustryId]
ORDER BY i.[DisplayOrder], a.[DisplayOrder], a.[Code];
