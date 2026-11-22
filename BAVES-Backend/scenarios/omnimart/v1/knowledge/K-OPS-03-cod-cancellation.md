---
type: knowledge
title: Thời gian giữ đơn và quy trình xử lý đơn COD chưa giao
description: Đơn COD không nghe máy sau 3 lần gọi sẽ bị hủy tự động và hoàn tồn kho sau 48 giờ.
tags: [cod, order, cancellation, process]
baves:
  code: K-OPS-03
  category: Process
  importance: Medium
  access:
    - persona: ops-manager
      accessType: Knows
      isHidden: false
    - persona: sales-lead
      accessType: Knows
      isHidden: false
    - persona: it-lead
      accessType: DoesNotKnow
      isHidden: false
---
Với các đơn hàng chọn hình thức thanh toán khi nhận hàng (COD):
1. Nhân viên CSKH sẽ gọi điện xác nhận đơn tối đa 3 lần trong vòng 24 giờ kể từ lúc đặt đơn.
2. Nếu sau 3 lần không liên lạc được hoặc khách báo hủy, đơn sẽ chuyển sang trạng thái "Chờ hủy".
3. Hàng giữ trong kho sẽ được tự động giải phóng (trả về tồn khả dụng) sau đúng 48 giờ kể từ lúc tạo đơn nếu không được xác nhận.

Khó khăn: Nhiều trường hợp nhân viên kho quên cập nhật thủ công trạng thái "Hủy", khiến hàng tồn thực tế bị đóng băng ảo trên hệ thống cả tuần.
