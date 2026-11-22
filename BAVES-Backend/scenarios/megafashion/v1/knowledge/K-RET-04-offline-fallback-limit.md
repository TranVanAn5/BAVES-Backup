---
type: knowledge
title: Giới hạn Cấp phép Ngoại tuyến POS (Offline Fallback Limit)
description: Ràng buộc kỹ thuật và hạn mức an toàn khi máy POS mất mạng internet.
tags: [pos, offline, security, fallback]
baves:
  code: K-RET-04
  category: System
  importance: Critical
  access:
    - persona: it-lead
      accessType: Knows
      isHidden: true
      disclosureRule: Chỉ tiết lộ khi Learner hỏi sâu về kịch bản mất mạng internet hoặc cơ chế dự phòng của quầy thu ngân.
    - persona: store-ops
      accessType: Knows
      isHidden: false
    - persona: fin-manager
      accessType: DoesNotKnow
      isHidden: false
    - persona: mkt-lead
      accessType: DoesNotKnow
      isHidden: false
---
Để đảm bảo cửa hàng vẫn bán được hàng khi showroom bị đứt cáp hoặc mất sóng 4G, kiến trúc POS Offline Fallback được giới hạn nghiêm ngặt nhằm tránh thất thoát:

1. Chế độ Ngoại tuyến (Offline Mode):
   Khi không thể kết nối tới Central Promo Engine sau 3 lần thử lại (timeout 2 giây/lần), máy POS tự động kích hoạt cờ `OFFLINE_MODE`.
2. Hạn chế sử dụng Voucher Online:
   Tuyệt đối KHÔNG cho phép máy POS offline xác thực các mã Voucher phát hành ngẫu nhiên trên App hoặc Voucher 1 lần (Single-use), vì không thể kiểm tra trạng thái khóa tạm.
3. Hạn mức ngoại lệ được cấp phép (Manager Override Voucher):
   Chỉ cho phép thu ngân quét các mã vạch giảm giá khẩn cấp được in sẵn trong két sắt của Cửa hàng trưởng (mỗi showroom tối đa 10 mã/ngày, giá trị cố định 30.000đ hoặc 50.000đ cho đơn trên 500.000đ).
   Bắt buộc Cửa hàng trưởng phải quẹt thẻ từ hoặc nhập mã PIN xác nhận trên phần mềm POS.
4. Cơ chế đồng bộ bù (Catch-up Sync):
   Mọi giao dịch bán hàng ngoại tuyến được lưu vào hàng đợi SQLite mã hóa cục bộ. Khi mạng phục hồi, POS tự động đẩy dữ liệu lên server trung tâm trong vòng 5 phút.
