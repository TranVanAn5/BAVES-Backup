---
type: knowledge
title: Quy tắc Phân bổ Chiết khấu theo Dòng hàng (Line-Item Allocation)
description: Công thức toán học phân bổ giá trị giảm giá trên tổng hóa đơn vào từng dòng sản phẩm phục vụ bài toán trả hàng và hạch toán thuế.
tags: [discount, allocation, finance, return]
baves:
  code: K-RET-02
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
      accessType: Partial
      isHidden: false
---
Quy định tài chính và kiểm toán của MegaFashion bắt buộc mọi khoản chiết khấu cấp đơn hàng (Order-level Discount, ví dụ Voucher giảm 200.000 VNĐ cho đơn từ 1.000.000 VNĐ) phải được phân bổ tỷ lệ thuận theo giá trị của từng dòng sản phẩm (Line Item) trong giỏ hàng trước khi in hóa đơn điện tử:

1. Công thức phân bổ chiết khấu cho dòng sản phẩm thứ i:
   `Discount_i = RoundDown( (Price_i * Qty_i / Total_Cart_Amount) * Total_Order_Discount )`
2. Xử lý phần dư làm tròn (Penny/Rounding Drift): Khoản chênh lệch vài trăm đồng do phép chia làm tròn xuống sẽ được cộng dồn giảm trừ vào dòng sản phẩm có giá trị lớn nhất trong hóa đơn.
3. Nguyên tắc hoàn tiền khi trả lại 1 sản phẩm:
   Khi khách hàng yêu cầu trả lại sản phẩm i trong vòng 7 ngày, số tiền thực hoàn cho khách sẽ là:
   `Refund_Amount = (Price_i * Qty_i) - Discount_i`
   Thu ngân tuyệt đối không được hoàn tiền theo nguyên giá niêm yết ban đầu của sản phẩm.
