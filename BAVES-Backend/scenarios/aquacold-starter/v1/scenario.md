---
type: scenario
title: "AquaCold Starter — Alert & incident log trên WMS lạnh"
description: "Beginner IT-BA: đặc tả mức cảnh báo nhiệt độ và form nhật ký sự cố tối thiểu trong WMS cold-chain (Odoo Supply Chain / Inventory)."
tags: [beginner, supply-chain, wms, iot, it-ba, babok-v3]
baves:
  code: BEG-WMS-01
  versionNo: 1
  difficulty: Beginner
  isFreeAccess: true
  status: Published
  summary: "Làm rõ Info/Warning/Critical temperature alerts và data fields nhật ký sự cố cho WMS kho đông AquaCold Starter."
  companyName: "AquaCold Starter"
  businessContext: "AquaCold Starter (ngành Supply Chain / kho vận theo Odoo Inventory) có 1 kho đông ~800 pallet, IoT đo 5 phút/lần, cảnh báo đổ vào Zalo chung. Học viên là IT-BA: đặc tả phần mềm alert levels + incident log fields gắn WMS — không thiết kế routing xe."
  businessProblem: "1) Mọi alert giống nhau — không có severity trên hệ thống. 2) Không có schema nhật ký sự cố. 3) Ca đêm thiếu SLA acknowledge trên phần mềm."
  businessObjectives: "1) As-is/to-be luồng nhận alert. 2) Bảng severity + ngưỡng. 3) 3 US + AC cho incident log UI/API. 4) Deliverable Beginner IT-BA."
  baRole: "Junior IT-BA cho WMS/IoT module — yêu cầu phần mềm cảnh báo & nhật ký, không thuần quy trình giấy."
  assignment: "Sprint 1: BAP + map (Supervisor, QA, Night Lead) + system context IoT→channel. Sprint 2: Phỏng vấn ngưỡng & phản ứng. Sprint 3: Severity matrix + acknowledge flow. Sprint 4: 3 US + AC incident log."
  inScope: "- Temperature alerts 1 kho; severity; acknowledge; incident fields. - Beginner BA deliverables."
  outOfScope: "- Full WMS redesign, fleet routing, legal SLA 3PL phức tạp."
  knownConstraints: "- Giữ IoT hiện có; Zalo tạm nếu chưa có app; số liệu giả lập."
---
Beginner Supply Chain WMS — IT-BA lean.
