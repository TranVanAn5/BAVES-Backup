---
type: scenario
title: "FinNova Starter — Hold reason trên Digital LOS"
description: "Beginner IT-BA: đặc tả taxonomy hold eKYC và UI/API Ops trên Digital LOS (Odoo Finance / lending software)."
tags: [beginner, finance, los, ekyc, it-ba, babok-v3]
baves:
  code: BEG-LOS-01
  versionNo: 1
  difficulty: Beginner
  isFreeAccess: false
  status: Published
  summary: "Làm rõ 5 bước eKYC và hold reason fields trên Digital LOS FinNova Starter để Ops xử lý đúng SLA."
  companyName: "FinNova Starter"
  businessContext: "FinNova Starter (ngành Finance) chạy Digital LOS: CCCD + selfie, ~70% auto-pass, còn lại hold. Credit Ops ~120–150 hồ sơ/ngày, SLA 4 giờ. Học viên IT-BA đặc tả taxonomy hold + màn hình Ops — thiên phần mềm, không scoring nâng cao."
  businessProblem: "1) UI Ops không show hold reason — phải đọc log. 2) Onboarding gọi trùng vì thiếu field chuẩn. 3) Chưa có spec taxonomy cho backlog."
  businessObjectives: "1) As-is 5 bước eKYC. 2) Taxonomy hold + fields tối thiểu. 3) 3 US + AC UI Ops. 4) Beginner IT-BA deliverables."
  baRole: "Junior IT-BA cho Digital LOS — yêu cầu phần mềm màn hình/API hold reason."
  assignment: "Sprint 1: BAP + map (Ops, Onboarding, Compliance). Sprint 2: Phỏng vấn SLA & lý do hold. Sprint 3: eKYC as-is + taxonomy. Sprint 4: 3 US + AC."
  inScope: "- eKYC CCCD/selfie hold/reject; taxonomy; Ops UI fields. - Beginner deliverables."
  outOfScope: "- ML scoring, deepfake advanced, core banking thay thế."
  knownConstraints: "- Không PII thật; không đổi vendor eKYC; số liệu giả lập."
---
Beginner Finance LOS — IT-BA lean.
