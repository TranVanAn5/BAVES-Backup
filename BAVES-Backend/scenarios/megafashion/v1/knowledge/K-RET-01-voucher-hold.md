---
type: knowledge
title: Cơ chế Khóa tạm Voucher (Two-Phase Voucher Hold)
description: Giải pháp ngăn chặn dùng lặp mã voucher một lần bằng cơ chế giữ trạng thái hai pha.
tags: [voucher, system, fraud, process]
baves:
  code: K-RET-01
  category: System
  importance: Critical
  access:
    - persona: it-lead
      accessType: Knows
      isHidden: false
    - persona: mkt-lead
      accessType: Knows
      isHidden: false
    - persona: fin-manager
      accessType: Partial
      isHidden: false
    - persona: store-ops
      accessType: DoesNotKnow
      isHidden: false
---
Để ngăn chặn hoàn toàn việc khách hàng áp dụng cùng một mã voucher 1 lần (Single-use) đồng thời trên website và tại quầy thu ngân trong thời gian trễ đồng bộ, hệ thống Promo Engine trung tâm cần triển khai cơ chế Two-Phase Hold:

1. Giai đoạn 1 (Hold Phase): Ngay khi khách hàng nhập mã voucher vào giỏ hàng online hoặc nhân viên thu ngân quét mã barcode voucher tại máy POS, hệ thống gửi lệnh API `POST /api/vouchers/hold`. Voucher lập tức chuyển sang trạng thái `HOLD` kèm khóa phân tán (Distributed Lock qua Redis) trong thời hạn tối đa 15 phút. Trong thời gian này, bất kỳ kênh nào khác cố tình sử dụng mã này đều nhận phản hồi lỗi `VOUCHER_CURRENTLY_LOCKED`.
2. Giai đoạn 2 (Commit Phase): Khi khách thanh toán thành công, hệ thống gửi lệnh `POST /api/vouchers/redeem` để chuyển voucher sang trạng thái vĩnh viễn `USED` kèm ID hóa đơn thanh toán.
3. Cơ chế Tự động Nhả (Auto-Release): Nếu sau 15 phút mà đơn hàng online chưa thanh toán hoặc giỏ hàng POS bị thu ngân hủy (Cancel Bill), Redis TTL hết hạn và hệ thống tự động hoàn trả voucher về trạng thái `ACTIVE` cho khách hàng.
