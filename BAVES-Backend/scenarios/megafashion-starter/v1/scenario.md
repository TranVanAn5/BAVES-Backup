---
type: scenario
title: "MegaFashion Starter — Spec điều kiện voucher trên Promo Engine"
description: "Beginner IT-BA: đặc tả rule voucher GIẢM50K và AC đồng bộ banner/app/POS (Odoo Retail Loyalty/POS)."
tags: [beginner, retail, promo, voucher, it-ba, babok-v3]
baves:
  code: BEG-PROMO-01
  versionNo: 1
  difficulty: Beginner
  isFreeAccess: false
  status: Published
  summary: "Làm rõ điều kiện voucher GIẢM50K trên Promotion Engine vs banner/POS; viết AC phần mềm."
  companyName: "MegaFashion Starter"
  businessContext: "MegaFashion Starter (Retail) chạy voucher GIẢM50K. Banner nói “mọi SP”, engine chỉ category Áo; POS đôi khi lỗi khác app. IT-BA đặc tả rule fields + AC — thiên phần mềm cấu hình promo."
  businessProblem: "1) Banner ≠ engine config. 2) Ba bên hiểu rule khác nhau. 3) Chưa có AC cho backlog cấu hình voucher."
  businessObjectives: "1) Bảng rule as-is/to-be. 2) Chỉ ra lệch kênh. 3) 3 US/AC. 4) Beginner IT-BA deliverables."
  baRole: "Junior IT-BA cho Promo Engine Retail — spec cấu hình phần mềm voucher."
  assignment: "Sprint 1: BAP + map (Marketing Ops, POS, CX). Sprint 2: Phỏng vấn lệch rule. Sprint 3: Rule table. Sprint 4: 3 US + AC."
  inScope: "- Một voucher GIẢM50K: min order, category, stacking, limit. - Sync mô tả điều kiện."
  outOfScope: "- Full loyalty points engine, AI personalization."
  knownConstraints: "- Không đổi giá gốc SKU; FREESHIP không stack với GIẢM50K."
---
Beginner Retail Promo — IT-BA lean.
