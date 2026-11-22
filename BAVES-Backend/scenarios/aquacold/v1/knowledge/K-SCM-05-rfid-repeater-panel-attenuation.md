---
type: knowledge
title: Giới hạn Suy hao Tín hiệu Mạng Sóng Radio trong Panel Kho Lạnh
description: Khó khăn kỹ thuật khi truyền sóng không dây trong hầm lạnh và giải pháp bố trí thiết bị thu phát sóng công nghiệp.
tags: [iot, rf, attenuation, hardware]
baves:
  code: K-SCM-05
  category: System
  importance: Medium
  access:
    - persona: iot-lead
      accessType: Knows
      isHidden: false
    - persona: dc-director
      accessType: Partial
      isHidden: false
    - persona: qa-lead
      accessType: DoesNotKnow
      isHidden: false
    - persona: fleet-manager
      accessType: DoesNotKnow
      isHidden: false
---
Các hầm bảo quản âm sâu được xây dựng bằng các tấm panel sandwich polyurethane bọc tôn mạ kẽm dày 150mm. Cấu trúc kim loại này hoạt động như một lồng Faraday (Faraday Cage), làm suy hao hơn 85% tín hiệu sóng Wifi 2.4GHz/5GHz và sóng Bluetooth BLE thông thường:

1. Thách thức hạ tầng:
   - Cảm biến gắn trên pallet không thể truyền dữ liệu trực tiếp ra ngoài tường kho.
   - Sương giá đóng dày trên bề mặt anten thiết bị thu phát làm giảm tiếp xúc và tăng tỷ lệ mất gói tin (Packet Loss > 40%).
2. Thiết kế giải pháp khả thi:
   - Bố trí các trạm thu phát sóng trung gian (Repeater/Gateway) công nghiệp đạt chuẩn chống ẩm kháng đóng băng IP68 tại các cột bê tông chịu lực bên trong hầm lạnh.
   - Các trạm Gateway này được nối dây cáp quang hoặc cáp mạng chống cháy bọc thép luồn trong ống bảo ôn dẫn thẳng ra phòng máy chủ trung tâm.
   - Tần suất cảm biến phát tín hiệu (Telemetry Heartbeat) được tối ưu hóa ở mức 3 phút/lần ở trạng thái tĩnh, và tự động chuyển sang 30 giây/lần khi cảm biến gia tốc phát hiện pallet đang được di chuyển bằng xe nâng.
