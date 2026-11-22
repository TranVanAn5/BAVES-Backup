---
type: knowledge
title: Cơ chế Trừ hao Thời gian Mở cửa Thùng xe (Door Open Offset Logic)
description: Thuật toán phân biệt giữa việc vi phạm nhiệt độ do tắt máy lạnh và việc nhiệt độ tăng tạm thời khi tài xế mở cửa trả hàng.
tags: [transport, fleet, door-sensor, offset]
baves:
  code: K-SCM-03
  category: System
  importance: High
  access:
    - persona: fleet-manager
      accessType: Knows
      isHidden: false
    - persona: iot-lead
      accessType: Knows
      isHidden: false
    - persona: qa-lead
      accessType: Partial
      isHidden: false
    - persona: dc-director
      accessType: DoesNotKnow
      isHidden: false
---
Để giải quyết mâu thuẫn giữa đội xe tải và bộ phận giám sát chất lượng khi xe giao hàng chặng cuối cho nhiều siêu thị, hệ thống phân tích dữ liệu xe lạnh được bổ sung cơ chế Door Open Offset:

1. Tích hợp cảm biến mở cửa từ tính (Magnetic Door Sensor):
   - Khi tài xế mở cửa thùng sau để dỡ hàng, cảm biến gửi trạng thái `DOOR_STATUS = OPEN`.
2. Quy tắc dung sai nhiệt độ khi trả hàng:
   - Trong thời gian cửa mở không quá 10 phút/điểm giao hàng, hệ thống cho phép dung sai nhiệt độ không khí trong thùng xe tăng tối đa 4°C so với ngưỡng cài đặt mà không bị ghi nhận là vi phạm (Temperature Breach).
   - Sau khi cửa đóng lại (`DOOR_STATUS = CLOSED`), máy lạnh xe phải hạ nhiệt độ về ngưỡng chuẩn trong vòng tối đa 8 phút.
3. Nhận diện hành vi gian lận tắt máy lạnh:
   - Nếu nhiệt độ thùng xe tăng dần đều trong khi cảm biến báo cửa vẫn đóng (`DOOR_STATUS = CLOSED`) và động cơ xe đang nổ máy, hệ thống khẳng định tài xế đã chủ động tắt máy lạnh để tiết kiệm dầu và tự động kích hoạt cờ cảnh báo `SUSPECTED_AC_SHUTDOWN`.
