---
type: knowledge
title: "Luồng nhận cảnh báo và phản ứng as-is"
description: "Cách đội kho nhận tin và xử lý khi nhiệt độ lệch."
tags: [beginner, process]
baves:
  code: K-BEG-WMS-02
  category: Process
  importance: High
  access:
    - persona: warehouse-supervisor
      accessType: Knows
      isHidden: false
    - persona: night-shift-lead
      accessType: Knows
      isHidden: false
    - persona: qa-assistant
      accessType: Partial
      isHidden: false
---
As-is (rút gọn):

1. Cảm biến ghi nhiệt mỗi **5 phút** → gateway đẩy về dashboard nội bộ.
2. Rule đơn giản gửi Zalo khi vượt -15°C.
3. Người trực đọc tin → vào phòng máy / kiểm tra cửa kho → điều chỉnh hoặc gọi bảo trì.
4. QA hỏi lại sau để ghi biên bản (thường **sau 30–90 phút**).

Thời gian phản ứng trung bình ca ngày: **8–12 phút**. Ca đêm: **15–25 phút** (ước lượng Supervisor).
