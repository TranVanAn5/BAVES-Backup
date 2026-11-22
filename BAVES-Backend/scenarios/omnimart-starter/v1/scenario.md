---
type: scenario
title: "OmniMart Starter — Chuẩn hóa order status trên OMS"
description: "Beginner IT-BA: đặc tả catalog trạng thái đơn và màn hình/API tối thiểu cho OMS bán lẻ (Odoo Retail / Sales)."
tags: [beginner, retail, oms, order-status, it-ba, babok-v3]
baves:
  code: BEG-OMS-01
  versionNo: 1
  difficulty: Beginner
  isFreeAccess: true
  status: Published
  summary: "Chuẩn hóa danh mục trạng thái đơn trên OMS giữa website và cửa hàng OmniMart Starter; giảm ticket “đơn đang ở đâu?”."
  companyName: "OmniMart Starter"
  businessContext: "OmniMart Starter (ngành Retail theo Odoo) có website, app và 3 cửa hàng. Đơn online và tại quầy chưa dùng chung order status trên OMS: web hiển thị EN (New/Paid/Shipping/Done), cửa hàng ghi VI trên Excel/chat. Học viên đóng vai IT-BA làm rõ yêu cầu phần mềm tối thiểu (catalog trạng thái + trách nhiệm cập nhật + user story UI/API)."
  businessProblem: "1) Hai hệ thống đặt tên trạng thái khác nhau — khách/CSKH hiểu lệch. 2) ~35% ticket hỏi trạng thái; CSKH gọi cửa hàng thủ công vì không có nguồn sự thật trên OMS. 3) Chưa có spec danh mục trạng thái + field cập nhật cho backlog dev."
  businessObjectives: "1) As-is/to-be order status catalog dùng chung. 2) Xác định actor cập nhật từng state (system vs user). 3) 3 user stories + AC cho màn hình/API trạng thái. 4) Deliverable Beginner IT-BA."
  baRole: "Junior IT Business Analyst cho sản phẩm OMS Retail — thiên về yêu cầu phần mềm (data field, UI ops, quy ước API), không chỉ quy trình giấy."
  assignment: "Sprint 1: BAP ngắn + stakeholder map (Ops, CSKH, Store) + system context. Sprint 2: Phỏng vấn; notes về status & nguồn cập nhật. Sprint 3: State catalog as-is/to-be + RACI cập nhật. Sprint 4: 3 user stories + AC (UI hoặc API status)."
  inScope: "- Order status lifecycle cơ bản trên OMS. - Catalog trạng thái, actor, field tối thiểu. - BAP, stakeholder map, notes, as-is/to-be, 3 US + AC."
  outOfScope: "- Payment gateway, loyalty, routing kho phức tạp, BRD/SRS đầy đủ Intermediate."
  knownConstraints: "- Không thay POS lõi; ưu tiên spec đồng bộ. - Số liệu giả lập đào tạo."
---
Beginner Retail OMS — IT-BA lean.
