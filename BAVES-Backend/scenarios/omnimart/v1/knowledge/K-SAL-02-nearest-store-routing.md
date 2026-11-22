---
type: knowledge
title: Cơ chế gán cửa hàng fulfill gần nhất (Order Routing Logic)
description: Đơn hàng online ưu tiên được giao từ cửa hàng cùng quận/huyện để tối ưu phí ship và thời gian giao 2 giờ.
tags: [sales, routing, fulfillment, store]
baves:
  code: K-SAL-02
  category: Process
  importance: High
  access:
    - persona: it-lead
      accessType: Partial
      isHidden: false
    - persona: ops-manager
      accessType: Knows
      isHidden: false
    - persona: sales-lead
      accessType: Knows
      isHidden: false
---
Kênh Sales cam kết dịch vụ "Giao nhanh 2 giờ" cho khách hàng nội thành. Để đạt được cam kết này, hệ thống phải tự động định tuyến đơn hàng về cửa hàng gần địa chỉ người nhận nhất còn đủ hàng.

Nếu cửa hàng gần nhất hết hàng mà hệ thống báo ảo (do POS chậm đồng bộ), đơn sẽ bị chuyển lòng vòng sang cửa hàng khác xa hơn, làm trễ hẹn 2 giờ và khách hàng thường hủy đơn ngay lập tức.
