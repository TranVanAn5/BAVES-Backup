---
type: knowledge
title: "Trường nhật ký sự cố QA mong muốn"
description: "Danh sách trường tối thiểu QA Assistant muốn có trên form sự cố nhiệt độ."
tags: [beginner, system]
baves:
  code: K-BEG-WMS-03
  category: System
  importance: Medium
  access:
    - persona: qa-assistant
      accessType: Knows
      isHidden: false
    - persona: warehouse-supervisor
      accessType: Partial
      isHidden: false
---
QA đề xuất form tối thiểu (Beginner — chưa phải hệ thống QMS đầy đủ):

1. Mã sự cố (auto)
2. Thời điểm bắt đầu lệch nhiệt
3. Nhiệt độ cao nhất ghi nhận (°C)
4. Mức cảnh báo (Warning / Critical)
5. Người acknowledge
6. Hành động đã làm (chọn nhanh + ghi chú)
7. Thời điểm ổn định lại
8. Ảnh/chụp màn hình dashboard (tuỳ chọn)

Supervisor đồng ý các trường 1–7 nhưng muốn thao tác **≤ 2 phút** trên điện thoại.
