---
type: knowledge
title: Cơ chế Cắt cầu chì API khi Đối tác Gặp sự cố (Circuit Breaker)
description: Ràng buộc kiến trúc kỹ thuật dự phòng và xử lý hàng đợi khi cổng kết nối CIC hoặc API bên ngoài bị quá tải.
tags: [resilience, circuit-breaker, fallback, architecture]
baves:
  code: K-FIN-05
  category: System
  importance: Medium
  access:
    - persona: arch-lead
      accessType: Knows
      isHidden: true
      disclosureRule: Chỉ tiết lộ khi Learner hỏi sâu về rủi ro gián đoạn API của đối tác bên thứ ba hoặc phương án dự phòng khi cổng CIC bị sập.
    - persona: risk-lead
      accessType: Knows
      isHidden: false
    - persona: biz-director
      accessType: DoesNotKnow
      isHidden: false
    - persona: fraud-lead
      accessType: DoesNotKnow
      isHidden: false
    - persona: legal-mgr
      accessType: DoesNotKnow
      isHidden: false
---
Do cổng tra cứu thông tin tín dụng CIC quốc gia thường xuyên bị nghẽn mạng vào các khung giờ cao điểm (11h - 14h) hoặc trong các đợt bảo trì hệ thống cuối tuần, kiến trúc hệ thống áp dụng mẫu thiết kế Circuit Breaker:

1. Cơ chế Ngắt mạch (Trip Breaker):
   - Nếu tỷ lệ gọi API sang CIC thất bại hoặc thời gian phản hồi (Response Time) vượt quá 5.000ms đối với trên 50% yêu cầu trong 1 phút, cầu chì Circuit Breaker tự động chuyển sang trạng thái `OPEN` (Ngắt kết nối).
2. Quy định Tuyệt đối về Phòng ngừa Rủi ro:
   - Trong thời gian cầu chì `OPEN`, hệ thống TUYỆT ĐỐI KHÔNG ĐƯỢC PHÉP tự động bỏ qua bước kiểm tra CIC để giải ngân (Bypass CIC Check), bất kể sức ép từ phía Phòng Kinh doanh.
3. Luồng xử lý dự phòng (Fallback Graceful Degradation):
   - Toàn bộ các hồ sơ vay tiếp nhận trong giai đoạn này được chuyển vào Hàng đợi Bất đồng bộ (Asynchronous Queue qua Apache Kafka).
   - Màn hình ứng dụng của khách hàng hiển thị thông báo thân thiện: "Hệ thống đang tiến hành thẩm định dữ liệu tín dụng của bạn. Kết quả sẽ được gửi qua SMS trong vòng 30 phút".
   - Khi dịch vụ CIC phục hồi và cầu chì chuyển sang `HALF-OPEN` rồi `CLOSED`, tiến trình ngầm Worker sẽ tự động rút dữ liệu từ hàng đợi ra để xử lý lần lượt theo thứ tự FIFO (First In, First Out).
