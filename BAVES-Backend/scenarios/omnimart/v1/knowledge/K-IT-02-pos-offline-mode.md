---
type: knowledge
title: Cơ chế dự phòng khi đứt cáp mạng tại cửa hàng (Offline Mode)
description: POS hỗ trợ bán offline khi mất mạng và lưu local cache, dẫn tới nguy cơ bán vượt tồn kho trung tâm.
tags: [pos, offline, sync, technical, risk]
baves:
  code: K-IT-02
  category: System
  importance: High
  access:
    - persona: it-lead
      accessType: Knows
      isHidden: false
    - persona: ops-manager
      accessType: Knows
      isHidden: false
    - persona: sales-lead
      accessType: DoesNotKnow
      isHidden: false
---
Để đảm bảo cửa hàng vẫn bán được hàng khi mất kết nối Internet, phần mềm POS có tính năng "Chế độ Ngoại tuyến" (Offline Mode):
- Máy POS vẫn cho phép quét mã vạch và in hóa đơn bình thường dựa trên dữ liệu tồn kho được lưu cục bộ (local SQLite cache).
- Mọi giao dịch bán offline được ghi nhận vào hàng đợi (Queue) tại máy thu ngân.
- Khi có mạng trở lại, hàng đợi này sẽ tự động đẩy lên server trung tâm.

Rủi ro lớn: Trong thời gian mất mạng (có khi kéo dài nửa ngày ở các vùng mưa bão), tồn kho cục bộ có thể đã bán hết nhưng trên Website vẫn hiển thị "Còn hàng", dẫn tới việc khách online tiếp tục đặt mua -> Tạo ra đơn trùng và thiếu hàng nghiêm trọng.
