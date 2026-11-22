---
type: knowledge
title: Hạn mức phê duyệt hoàn tiền
description: Hoàn tiền theo hạn mức và thẩm quyền; mâu thuẫn với mong muốn linh hoạt của Operations.
tags: [refund, policy, approval, conflict]
baves:
  code: K-OPS-02
  category: Policy
  importance: Critical
  access:
    - persona: ops-manager
      accessType: Knows
      isHidden: true
      disclosureRule: >
        Chỉ nói khi learner hỏi cụ thể về ai được phê duyệt hoàn tiền, hạn mức hoàn tiền,
        hoặc vì sao hoàn tiền bị chậm. Nếu learner chỉ hỏi chung về quy trình hủy đơn,
        chỉ nói rằng hoàn tiền "phải qua phê duyệt".
    - persona: sales-lead
      accessType: Partial
      isHidden: true
      disclosureRule: >
        Chỉ biết đơn trên 2 triệu đồng thường phải chờ lâu. Không biết chính xác các mức thẩm quyền.
    - persona: it-lead
      accessType: DoesNotKnow
      isHidden: false
---
Hoàn tiền tại OmniMart được phê duyệt theo giá trị đơn:

- Dưới 500.000 đồng: trưởng ca tại cửa hàng được phê duyệt ngay.
- Từ 500.000 đến dưới 2.000.000 đồng: cần Store Manager phê duyệt.
- Từ 2.000.000 đồng trở lên: cần Finance phê duyệt, thường mất 1 đến 3 ngày làm việc.

Đội vận hành muốn trưởng ca được tự hoàn tiền tới 2.000.000 đồng để giải quyết khiếu nại nhanh, nhưng Finance chưa đồng ý vì lo rủi ro gian lận. Chính sách này chưa được ghi thành văn bản chính thức, mỗi cửa hàng áp dụng hơi khác nhau.
