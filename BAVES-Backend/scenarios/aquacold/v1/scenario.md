---
type: scenario
title: AquaCold — Đặc tả WMS Cold Chain (Supply Chain / Inventory)
description: IT Business Analyst đặc tả WMS kho lạnh tích hợp IoT cho 3PL AquaCold — hướng Odoo Inventory/Warehouse + IoT integration.
tags: [supply-chain, wms, cold-chain, iot, systems-integration, babok-v3, it-ba]
baves:
  code: SCM-001
  versionNo: 1
  difficulty: Advanced
  summary: Đặc tả phần mềm WMS cold-chain (putaway, FEFO, cross-dock, QC hold) và tích hợp cảm biến IoT realtime cho AquaCold 3PL.
  companyName: AquaCold 3PL Logistics Services
  businessContext: >
    AquaCold vận hành 3 DC lạnh và 150 xe lạnh (Odoo industry: Supply Chain / Inventory & Logistics — không phải Retail).
    WMS hiện tại + giấy tay + Zalo cảnh báo nhiệt chưa thành hệ thống tích hợp. Mục tiêu: đặc tả WMS/IoT
    tương thích tư duy module Inventory/Barcode của Odoo, cộng NFR cho môi trường -25°C và GSP/GDP.
  businessProblem: >
    1. Không có workflow inbound phần mềm: pallet chờ sàn đệm quá lâu → excursion nhiệt.
    2. Thiếu end-to-end temperature trace trên hệ thống (kho ↔ xe) → tranh chấp QA/Fleet không chứng minh được.
    3. Location accuracy thấp vì putaway thủ công; WMS chưa directed putaway/barcode.
    4. Cross-dock không có matching engine — double handling.
  businessObjectives: >
    1. Đặc tả WMS inbound/putaway/picking/FEFO/dispatch + QC Hold tự động theo ngưỡng IoT.
    2. Định nghĩa integration IoT → WMS/e-POD (frequency, alert levels, immutable log).
    3. NFR: rugged device, connectivity trong panel kho, retention dữ liệu ≥ 5 năm.
    4. Bàn giao BRD/SRS/US/RTM cho đội phát triển WMS.
  baRole: >
    Bạn là IT Business Analyst (Supply Chain Systems) — tập trung yêu cầu phần mềm WMS/IoT,
    phỏng vấn DC Ops, QA, Fleet và WMS/IoT Engineer; bàn giao spec cho software delivery.
  assignment: >
    Sprint 1: BAP + landscape (WMS, IoT, handheld) + stakeholder map.
    Sprint 2: Elicitation; BRD v1 + rules (QC Hold, FEFO, excursion).
    Sprint 3: Sự cố mất điện/nhiệt; SRS v1 (alert workflow, directed putaway, IoT API).
    Sprint 4: Impact + SRS v2 + RTM.
  inScope: >
    - WMS cold-chain flows; IoT alert & traceability; barcode/QR location; cross-dock matching rules.
    - Hồ sơ BABOK cho phát triển phần mềm.
  outOfScope: >
    - Xây kho mới / mua xe; firmware sensor; đổi bảng giá 3PL.
  knownConstraints: >
    - Thiết bị chịu lạnh; sóng yếu trong panel; tuân thủ GSP/GDP lưu trữ dữ liệu.
---
Supply Chain WMS — IT-BA lean (Odoo Inventory-oriented).
