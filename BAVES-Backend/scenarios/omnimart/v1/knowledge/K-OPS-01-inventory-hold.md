---
type: knowledge
title: Thời điểm giữ tồn kho cho đơn online
description: Đơn online hiện chỉ được giữ hàng sau khi thanh toán thành công, gây bán trùng tại cửa hàng.
tags: [inventory, order, process]
baves:
  code: K-OPS-01
  category: Process
  importance: High
  access:
    - persona: ops-manager
      accessType: Knows
      isHidden: false
    - persona: sales-lead
      accessType: Partial
      isHidden: false
    - persona: it-lead
      accessType: Partial
      isHidden: false
---
Hiện tại đơn đặt trên website chỉ được giữ tồn kho sau khi khách thanh toán thành công. Với đơn thanh toán khi nhận hàng (COD), hàng chỉ được giữ khi nhân viên kho xác nhận đơn, thường chậm 2 đến 4 giờ trong giờ cao điểm.

Trong khoảng thời gian đó, cửa hàng vẫn có thể bán cùng sản phẩm cho khách mua trực tiếp. Mỗi tuần có khoảng 30 đến 40 đơn online phải hủy hoặc đổi hàng vì hết hàng sau khi đã nhận đơn.

Đội vận hành muốn giữ hàng ngay khi đơn được tạo, nhưng lo rằng hàng bị giữ quá lâu cho các đơn COD mà khách không xác nhận sẽ làm cửa hàng mất doanh thu.
