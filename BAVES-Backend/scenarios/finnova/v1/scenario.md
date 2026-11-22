---
type: scenario
title: FinNova — Digital LOS & eKYC Decision Engine (Finance)
description: IT Business Analyst đặc tả Digital Loan Origination (LOS) — eKYC liveness, multi-tier decision funnel, tích hợp CIC API (ngành Finance / FinTech, tư duy Odoo Finance + custom lending apps).
tags: [finance, fintech, los, ekyc, api-integration, babok-v3, it-ba]
baves:
  code: FIN-001
  versionNo: 1
  difficulty: Advanced
  summary: Đặc tả phần mềm Digital LOS: eKYC Liveness, decision engine STP, phễu lọc API bên thứ ba cho FinNova (Finance).
  companyName: FinNova Digital Finance Corporation
  businessContext: >
    FinNova cho vay tiêu dùng số qua app (Odoo industry analogy: Finance — quy trình tín dụng số,
    không phải Retail/Inventory). Hệ thống LOS hiện ghép module mua ngoài + thẩm định thủ công.
    IT-BA đặc tả to-be: customer journey số, Decision Engine, tích hợp eKYC/CIC với NFR bảo mật PDPD.
  businessProblem: >
    1. eKYC yếu → fraud deepfake/CCCD giả; thiếu rule engine chống gian lận trên LOS.
    2. STP thấp (~22%): UI/ops queue thiếu dữ liệu quyết định; thiếu multi-tier funnel phần mềm.
    3. False positive gig workers do rule cứng trên hệ thống cũ.
    4. Gọi CIC sớm gây cost/latency — cần orchestration API có tầng lọc.
  businessObjectives: >
    1. Đặc tả LOS journey + Decision Matrix + hold/reject taxonomy.
    2. Spec tích hợp eKYC Liveness & CIC (timeout, retry, cost control).
    3. NFR: PII encryption, SLA < 5 phút STP, audit trail.
    4. BRD/SRS/US/RTM cho đội phát triển FinTech.
  baRole: >
    Bạn là IT Business Analyst (FinTech / Lending Systems) — thiên về kiến trúc luồng phần mềm,
    API, decision rules và tuân thủ dữ liệu; làm việc với Risk, Fraud, Legal, Enterprise Architect.
  assignment: >
    Sprint 1: BAP + system landscape LOS + stakeholder map.
    Sprint 2: Elicitation; BRD v1 + credit/fraud business rules.
    Sprint 3: Deepfake swarm event; SRS v1 (eKYC, decision engine, ops UI).
    Sprint 4: CIC outage / rate-limit impact; SRS v2 + RTM.
  inScope: >
    - Digital lending journey; decision funnel; eKYC/CIC integration contracts; ops screens.
    - BABOK software deliverables.
  outOfScope: >
    - Thay core banking/accounting; train ML scorecard; field collection.
  knownConstraints: >
    - NHNN / PDPD; AES-256 / TLS; decision engine < 3s nội bộ.
---
Finance Digital LOS — IT-BA lean.
