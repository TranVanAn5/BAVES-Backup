---
type: knowledge
title: Kênh và quy tắc thông báo trạng thái đơn hàng
description: Đơn hàng chỉ gửi email tự động khi đặt thành công và xuất kho; không có SMS hoặc thông báo khi hủy hoặc hoàn tiền.
tags: [sales, customer-service, notification, process]
baves:
  code: K-SAL-01
  category: Process
  importance: High
  access:
    - persona: sales-lead
      accessType: Knows
      isHidden: false
    - persona: ops-manager
      accessType: Partial
      isHidden: false
    - persona: it-lead
      accessType: Partial
      isHidden: false
---
Quy trình gửi thông báo cho khách hàng hiện tại:
1. Khi khách đặt hàng online: Hệ thống tự động gửi 1 email xác nhận đơn hàng kèm mã đơn.
2. Khi hàng được giao cho đơn vị vận chuyển: Hệ thống gửi 1 email thông báo mã vận đơn.

Các lỗ hổng lớn trong trải nghiệm khách hàng:
- Không có tin nhắn Zalo ZNS hoặc SMS. Nhiều khách hàng không có thói quen kiểm tra email thường xuyên.
- Khi đơn hàng bị hủy do hết tồn kho hoặc thanh toán thất bại, hệ thống KHÔNG hề gửi email thông báo lý do hủy. Khách chỉ phát hiện ra khi tự đăng nhập vào website hoặc gọi lên hotline phàn nàn.
- Khi xử lý hoàn tiền, khách cũng không nhận được bất kỳ cập nhật nào cho tới khi tiền về tài khoản ngân hàng.

Đội CSKH mong muốn: Bổ sung thông báo qua Zalo ZNS/SMS cho các mốc: Đã xác nhận đơn -> Đang đóng gói -> Đang giao hàng -> Đã hủy (kèm lý do) -> Đã hoàn tiền thành công.
