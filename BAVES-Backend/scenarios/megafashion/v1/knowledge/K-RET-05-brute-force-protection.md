---
type: knowledge
title: Cơ chế Chống tấn công Brute-force mã Voucher
description: Quy định an toàn thông tin chống công cụ tự động quét mã giảm giá qua API.
tags: [security, rate-limit, brute-force]
baves:
  code: K-RET-05
  category: Security
  importance: Medium
  access:
    - persona: it-lead
      accessType: Knows
      isHidden: false
    - persona: mkt-lead
      accessType: Partial
      isHidden: false
    - persona: store-ops
      accessType: DoesNotKnow
      isHidden: false
    - persona: fin-manager
      accessType: DoesNotKnow
      isHidden: false
---
Do các chương trình voucher công khai thường bị các nhóm "săn mã" dùng bot hoặc script quét tự động (Brute-force attack) để đoán chuỗi mã giảm giá, hệ thống Central Promo Engine thiết lập cơ chế bảo vệ phân tầng:

1. Quy tắc giới hạn tần suất (Rate Limiting):
   Mỗi địa chỉ IP hoặc tài khoản người dùng chỉ được phép gọi API kiểm tra mã voucher tối đa 10 lần trong 60 giây.
2. Khóa tạm quyền nhập mã:
   Nếu một tài khoản nhập sai mã voucher 5 lần liên tiếp trong vòng 10 phút, quyền nhập mã của tài khoản đó trên cả App và Web sẽ bị tạm khóa trong 30 phút.
3. Mã hóa định dạng mã Voucher:
   Chuỗi mã voucher không được sử dụng các quy luật tăng tuần tự đơn giản (như VOUCHER01, VOUCHER02), mà phải bao gồm tiền tố chương trình + chuỗi ngẫu nhiên Base32 có độ dài tối thiểu 8 ký tự kèm mã kiểm tra checksum Luhn.
