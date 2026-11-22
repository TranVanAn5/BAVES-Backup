---
type: scenario
title: OmniMart — Đồng bộ OMS đa kênh (Retail / Sales)
description: IT Business Analyst đặc tả cải tiến Order Management System cho chuỗi bán lẻ OmniMart — đồng bộ tồn, trạng thái đơn và API giữa eCommerce, POS và kho (hướng Odoo Retail + Sales/Inventory).
tags: [retail, oms, omni-channel, systems-integration, babok-v3, it-ba]
baves:
  code: OMNI-001
  versionNo: 1
  difficulty: Intermediate
  summary: Đặc tả phần mềm OMS đa kênh để đồng bộ trạng thái đơn, reservation tồn kho và thông báo khách giữa website, app và 25 cửa hàng OmniMart (Odoo Retail).
  companyName: OmniMart Retail Corporation
  businessContext: >
    OmniMart là chuỗi bán lẻ đồ gia dụng (Odoo industry: Retail) với 25 cửa hàng, website và app.
    Hệ thống mục tiêu gần mô hình Odoo Sales + Inventory + eCommerce: tiếp nhận đơn đa kênh,
    giữ/nhả tồn, cập nhật fulfillment. Hiện website, POS cửa hàng và file tồn DC Củ Chi chưa dùng
    một OMS thống nhất — dữ liệu lệch pha, CSKH và IT phải đối soát thủ công.
  businessProblem: >
    1. Lệch tồn ảo giữa eCommerce và POS: website còn “Available” nhưng DC/cửa hàng đã hết — hủy đơn muộn.
    2. Reservation/release tồn không thống nhất giữa kênh (timeout, hold thủ công) → giam tồn ảo.
    3. Trạng thái đơn (order state machine) khác nhau giữa web (EN) và POS (VI); không có API/event chuẩn.
    4. Hoàn tiền/hủy đơn thiếu workflow phần mềm rõ (approval, audit log) — mâu thuẫn Ops / Finance / IT.
  businessObjectives: >
    1. Đặc tả OMS to-be: một catalog trạng thái đơn và reservation policy dùng chung.
    2. Định nghĩa API/events đồng bộ tồn & trạng thái (NFR: latency, idempotency).
    3. Giảm hủy đơn do lệch tồn; rút ngắn hoàn tiền có audit trail trên hệ thống.
    4. Bàn giao BRD + SRS + User Stories + RTM cho đội phát triển (không code thay ERP lõi).
  baRole: >
    Bạn là IT Business Analyst (Retail Systems) — thiên về phần mềm: khảo sát tích hợp Sales/Inventory,
    phỏng vấn Ops, CSKH và IT Lead, đặc tả OMS cải tiến theo hướng Odoo Retail apps (Sales, Inventory, Website).
  assignment: >
    Sprint 1: BAP + stakeholder map (Ops, Sales/CS, IT) + as-is system landscape (web, POS, DC stock).
    Sprint 2: Elicitation — order state machine, reservation rules; BRD v1 + Business Rules.
    Sprint 3: Biến cố kỹ thuật/đồng bộ; SRS v1 (API sync, UI ops tối thiểu) + User Stories/AC.
    Sprint 4: Impact analysis; SRS v2 + RTM đầu–cuối cho backlog phát triển.
  inScope: >
    - Order lifecycle đa kênh trên OMS: create → pay/confirm → reserve → pack → ship → complete/cancel/refund.
    - Data model & API đồng bộ tồn/trạng thái; NFR cơ bản; audit log hủy/hoàn tiền.
    - Hồ sơ BABOK: BAP, BRD, SRS, User Stories, RTM.
  outOfScope: >
    - Viết code / thay toàn bộ POS hoặc ERP kế toán lõi.
    - Đàm phán hợp đồng vận chuyển / cổng thanh toán.
    - Loyalty phức tạp (thuộc application LMS_PROMO).
  knownConstraints: >
    - Giữ POS hiện tại; OMS phải tích hợp qua API/file sync giai đầu.
    - Ngân sách giai đoạn 1 & 4 sprint khảo sát; không downtime bán hàng cửa hàng.
    - Ưu tiên giải pháp phần mềm có thể map sang module Sales/Inventory kiểu Odoo.
---
Nguồn nội dung BA-VES: IT-BA cho ngành Retail (Odoo). Không đặt đáp án ẩn vào đây.
