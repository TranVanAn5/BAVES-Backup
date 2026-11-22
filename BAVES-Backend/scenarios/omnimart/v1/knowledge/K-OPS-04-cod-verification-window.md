---
type: knowledge
title: Quy trình đóng gói và điều kiện hủy đơn COD tại kho
description: Đơn hàng COD chỉ được hủy tự động nếu khách hàng không phản hồi cuộc gọi xác nhận sau 24 giờ.
tags: [operations, cod, cancellation, warehouse]
baves:
  code: K-OPS-04
  category: Process
  importance: High
  access:
    - persona: it-lead
      accessType: DoesNotKnow
      isHidden: false
    - persona: ops-manager
      accessType: Knows
      isHidden: false
    - persona: sales-lead
      accessType: Partial
      isHidden: false
---
Với đơn hàng giao tiền khi nhận (COD), kho luôn yêu cầu nhân viên CSKH gọi điện xác nhận địa chỉ trước khi nhân viên kho in phiếu đóng gói (pick-and-pack).

Nếu khách hàng không nhấc máy sau 3 lần gọi trong vòng 24 giờ, đơn sẽ được gắn cờ "Hủy do không liên lạc được" và hàng trong kho mới được giải phóng trở lại kệ tồn bán.
