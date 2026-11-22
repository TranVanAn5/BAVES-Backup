---
type: knowledge
title: Quy định Thu hồi Điểm và Giá trị Tích lũy khi Trả hàng
description: Quy trình thu hồi điểm thưởng phát sinh và xử lý cấn trừ điểm âm khi khách trả hàng.
tags: [loyalty, points, clawback, refund]
baves:
  code: K-RET-03
  category: Policy
  importance: High
  access:
    - persona: fin-manager
      accessType: Knows
      isHidden: false
    - persona: store-ops
      accessType: Knows
      isHidden: false
    - persona: mkt-lead
      accessType: Partial
      isHidden: false
    - persona: it-lead
      accessType: DoesNotKnow
      isHidden: false
---
Khi khách hàng phát sinh giao dịch đổi/trả hàng, quy định xử lý điểm tích lũy MegaClub được áp dụng như sau:

1. Thu hồi điểm tích lũy sinh ra từ đơn hàng gốc (Earned Points Clawback):
   Số điểm đã cộng vào tài khoản khách hàng tương ứng với giá trị thực thu của sản phẩm bị trả lại sẽ bị hệ thống tự động trừ đi ngay khi biên bản trả hàng được tạo:
   `Clawed_Points = Floor( Net_Refund_Amount / 10.000 )`
2. Xử lý trường hợp khách đã tiêu hết điểm trước khi trả hàng (Negative Balance Handling):
   Nếu số dư điểm hiện tại trong tài khoản MegaClub của khách hàng nhỏ hơn `Clawed_Points` (khách đã dùng điểm vừa tích được để mua đơn khác), hệ thống sẽ:
   - Trừ số dư điểm của khách về mức 0.
   - Phần điểm bị thiếu sẽ được quy đổi thành tiền mặt theo tỷ lệ 1 điểm = 100 VNĐ và khấu trừ trực tiếp vào số tiền hoàn trả cho khách:
     `Deduction_Amount = (Clawed_Points - Current_Point_Balance) * 100`
3. Điểm đã dùng để thanh toán cho sản phẩm bị trả lại:
   Nếu đơn hàng gốc khách dùng điểm để thanh toán, số điểm tương ứng phân bổ cho món hàng đó sẽ được hoàn trả lại ví điểm của khách trong vòng 24 giờ.
