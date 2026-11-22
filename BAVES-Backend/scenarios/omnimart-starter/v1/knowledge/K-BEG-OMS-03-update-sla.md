---
type: knowledge
title: "Quy ước SLA cập nhật trạng thái nội bộ"
description: "Ops kỳ vọng cửa hàng cập nhật trong 15 phút; thực tế thường trễ."
tags: [beginner, policy]
baves:
  code: K-BEG-OMS-03
  category: Policy
  importance: Medium
  access:
    - persona: ops-coordinator
      accessType: Knows
      isHidden: false
    - persona: store-staff
      accessType: Partial
      isHidden: false
---
Quy ước nội bộ Ops (chưa thành policy chính thức trên hệ thống):

- Sau khi nhận đơn gán về cửa hàng: cập nhật **Chờ lấy / Confirmed** trong **15 phút**.
- Sau khi đóng gói xong: cập nhật **Packed / Đang gói xong** trước khi gọi shipper.
- Sau bàn giao shipper: cập nhật **Shipping** ngay.

CSKH được phép hẹn khách “kiểm tra lại trong 30 phút” nếu cửa hàng chưa phản hồi chat.
