---
type: knowledge
title: Cơ chế đồng bộ tồn kho và giới hạn kỹ thuật POS
description: Hệ thống POS tại quầy chỉ đồng bộ định kỳ theo chu kỳ 15-30 phút, gây độ trễ thông tin tồn kho.
tags: [it, technical, sync, pos, constraint]
baves:
  code: K-IT-01
  category: System
  importance: Critical
  access:
    - persona: it-lead
      accessType: Knows
      isHidden: false
    - persona: ops-manager
      accessType: Partial
      isHidden: false
    - persona: sales-lead
      accessType: DoesNotKnow
      isHidden: false
---
Hiện tại các máy POS tại 25 cửa hàng đang chạy phần mềm cũ được cài đặt cục bộ tại từng chi nhánh.

Dữ liệu tồn kho và đơn hàng không được đẩy tức thời (real-time) lên cloud mà thông qua một tiến trình chạy ngầm (background batch job) định kỳ mỗi 30 phút (hoặc 15 phút vào khung giờ cao điểm 18h - 21h).

Đội IT không thể chuyển ngay sang kiến trúc Event-Driven hoặc Webhook tức thời vì:
1. Đường truyền mạng tại một số cửa hàng tỉnh không ổn định.
2. Cơ sở dữ liệu trung tâm sẽ bị quá tải nếu nhận hàng trăm kết nối đồng thời từ toàn bộ máy tính tiền.

Giải pháp khả thi trước mắt: Có thể rút ngắn chu kỳ đồng bộ xuống 5 phút cho các mặt hàng bán chạy (Hot items), hoặc API chỉ đẩy biến động (delta update) thay vì toàn bộ danh mục kho.
