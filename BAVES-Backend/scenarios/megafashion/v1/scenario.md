---
type: scenario
title: MegaFashion — Loyalty & Promo Engine (Retail)
description: IT Business Analyst đặc tả Central Promotion/Loyalty Engine đồng bộ realtime với POS và eCommerce (Odoo Retail: Loyalty, POS, eCommerce).
tags: [retail, loyalty, promotion-engine, pos-integration, babok-v3, it-ba]
baves:
  code: RET-001
  versionNo: 1
  difficulty: Intermediate
  summary: Đặc tả phần mềm Promo/Loyalty Engine realtime — chống double-redeem voucher, sync điểm POS↔App cho MegaFashion (Retail).
  companyName: MegaFashion Retail Corporation
  businessContext: >
    MegaFashion (Odoo Retail): 65 showroom, web, MegaClub app. Loyalty đang gắn POS cũ, Promo Engine nằm cloud eCom —
    hai hệ không realtime. IT-BA đặc tả central engine + API POS/App theo hướng module Loyalty/POS của Odoo.
  businessProblem: >
    1. Single-use voucher redeem kép do thiếu lock/validation realtime trên engine.
    2. Point accrual trễ 4–12h — không có event sync chuẩn.
    3. Return/refund sai điểm vì thiếu line-item allocation logic trên phần mềm.
    4. POS chậm khi evaluate stack rule phức tạp — cần NFR + rule engine spec.
  businessObjectives: >
    1. Spec validation voucher realtime (<1.5s) + idempotent redeem.
    2. API accrual/redeem đồng bộ App–POS ≤ 60s.
    3. Thuật toán hoàn điểm khi return; audit tài chính.
    4. BRD/SRS/US/RTM cho đội phát triển.
  baRole: >
    Bạn là IT Business Analyst (Retail Commerce Systems) — tập trung rule engine, API POS, data consistency;
    làm việc Marketing, Finance, Store Ops, IT Lead.
  assignment: >
    Sprint 1: BAP + landscape (POS, Promo cloud, App) + stakeholders.
    Sprint 2: Elicitation rules; BRD v1.
    Sprint 3: Flash-sale fraud event; SRS v1 (API, AC Gherkin).
    Sprint 4: POS offline impact; SRS v2 + RTM.
  inScope: >
    - Voucher/points lifecycle; realtime validation; return allocation; POS/API contracts.
  outOfScope: >
    - Đổi hardware POS; hoa hồng NV; SMS gateway contracts.
  knownConstraints: >
    - POS cũ, mạng chập chờn; discount budget ≤ 8% GMV; POS evaluate ≤ 10s/hóa đơn.
---
Retail Promo Engine — IT-BA lean (Odoo Loyalty/POS).
